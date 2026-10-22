---
description: C# 12 / .NET 8 / ReactiveUI coding standards and quality guidelines for Eede.
---

# Coding Standards: C# 13 / .NET 10 & ReactiveUI (Eede)

## 1. 認知負荷の最小化: 「5行ルール」と単一責任
- **メソッドの行数**: 複雑な処理を行うメソッドは、1メソッドあたり **5〜10行程度** を目安に分割せよ。
- **抽出 (Extract Method)**: 論理的な段落（入力検証、計算、状態更新、外部通知）ごとにプライベートメソッドへ切り出すこと。
- **多態性 (Polymorphism)**: 型による分岐（`switch` や `if-else` の連鎖）は、Strategy パターン等の多態性（`IDrawStyle`, `IImageBlender` 等）に置き換えること。
- **命名における共通接頭辞・接尾辞の禁止（XxxService追放）と能動アクター命名原則**:
  - `Service`, `Manager`, `Helper` などの曖昧なゴミ箱サフィックス（接尾辞）は絶対禁止。
  - そのクラスやインターフェースが果たす具体的・能動的な責務を表す名詞（能動アクター）で命名すること（例: `UpdateService` ➔ `AppUpdater`, `SessionRecoveryService` ➔ `SessionRecoverer`, `ExternalBrowserService` ➔ `ExternalBrowserLauncher`, `ThemeService` ➔ `ThemeDetector`, `PictureIOService` ➔ `PictureFileIO`, `SelectionService` ➔ `SelectionClipboard`, `AnimationPatternService` ➔ `AnimationPatternEditor`）。
  - 名前空間や物理ディレクトリも曖昧な `Services` ではなく責務名（`Launchers`, `Theming`, `Coordinators` 等）で分離すること。

## 2. C# / .NET 規約 & Jules プラクティス
- **Nullable 参照型**: `#nullable enable` を徹底し、null 警告（CS8600〜CS8629）を放置しないこと。
- **不変性 (Immutability)**: ドメインの値オブジェクトには `readonly record struct` または `sealed record` を積極的に採用すること。
- **ドメイン水際防御と 64bit 整数演算原則 (Domain Gatekeeper & Overflow Defense)**:
  - 画像寸法や配列バイト数の計算において、32bit 符号付き整数の乗算（`width * 4 * height`）は $2^{31}-1$ を超えるとオーバーフローして 0 や負数になり、バッファ過小確保（Heap Buffer Overflow / CWE-190 / CWE-787）を招く。
  - 必ずドメイン境界（`PictureSize` 等）で上限寸法（`MaxDimension = 16384`）を物理検証し、バイト数計算は `long totalBytes = (long)size.Width * COLOR_32BIT * size.Height;` と `Array.MaxLength` 検証、および `checked` 演算を徹底すること。下流の `unsafe` ポインタ処理への不正入力侵入を水際で 100% 遮断せよ。
- **ドメイン境界値制約とセンチネル値（MaxValue）整合性の原則**:
  - ドメインモデルに上限値・下限値チェック（例: `MaxDimension = 16384`）を導入する際は、モデル側に `public static readonly PictureSize MaxValue = new(MaxDimension, MaxDimension);` などの型安全な定数/プロパティを提供すること。
  - 「無制限」「無限大」の意図で既存コードに潜んでいた `int.MaxValue` や raw センチネル値のハードコード（`new PictureSize(int.MaxValue, int.MaxValue)` 等）を必ず同時に撲滅・置換し、予期せぬ未処理 `ArgumentException` による UI クラッシュを防止せよ。
- **ファイルストレージにおけるパス解決と Basename キャッシュ衝突防止**:
  - 静的メモリキャッシュ（`AvaloniaFileStorage` 等）において、ルート化されたローカルファイルパス（`Path.IsPathRooted(path) == true`）を検索・書き込みする際は、絶対に `Path.GetFileName(path)`（ファイル名単体）へフォールバックしてはならない。別ディレクトリの同名ファイルへのサイレント上書き・破壊を招くため、完全一致キーのみで解決すること（WASM/ブラウザの相対仮想 URI のみフォールバックを許可）。
- **ゼロアロケーション & ホットパス最適化 (Jules Practice)**:
  - 描画ループや履歴の Undo/Redo 内で、ステートレスな不変インスタンス（`new DirectImageBlender()` 等）を毎回 `new` するのを禁止し、ループ外で 1 度だけインスタンス化して再利用すること。
  - コレクション走査やインデックス取得の途中で安易に `.ToList()` / `.ToArray()` を呼ばないこと。
  - ピクセル配列やバッファのバルクコピーには、1 バイトごとのループ代入を避け `Span.Slice(...).CopyTo(...)` やバルク転送を活用すること。
- **堅牢な例外処理の原則 (Jules Practice)**:
  - 裸の `catch { }` や `catch (Exception) { }` は禁止。
  - `catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or JsonException)` のように例外フィルターを用い、予期される例外のみを安全にトラップすること（バグや `NullReferenceException` をサイレントに隠蔽させない）。
  - フォールバックや例外無視を行う場合であっても空ブロックは禁止。Release ビルドで消去される `Debug.WriteLine` ではなく、診断が確実に残る `System.Diagnostics.Trace.WriteLine(...)` を使用すること。

## 3. ReactiveUI & MVVM 規約
- **プロパティ定義**: `[Reactive] public partial string Title { get; set; }` または `RaiseAndSetIfChanged` を使用すること。
- **Observable 連携**:
  - 状態派生プロパティには `ObservableAsPropertyHelper<T>`（`[ObservableAsProperty]`）を使用し、`WhenAnyValue` からストリームを生成すること。
  - コレクションの購読（`WhenAnyValue`, `StatusChanged` 等）は、`CompositeDisposable`（`_disposables`）で確実に管理・破棄すること。
- **コマンド定義**:
  - 同期処理: `ReactiveCommand.Create(...)`
  - 非同期処理: `ReactiveCommand.CreateFromTask(...)`
  - 実行可否（`canExecute`）ストリームを適切にバインドすること。
- **コレクション反復の安全性**:
  - `ObservableCollection` の要素を非同期ループ（`foreach` + `await`）で走査する際は、ループ内で要素の削除・追加が起きない設計とするか、必要に応じてスナップショットを用いること。
