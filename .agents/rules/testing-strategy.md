---
description: Testing strategy focusing on NUnit, Classical Detroit School, and Avalonia.Headless UI/ViewModel integration testing.
---

# Testing Strategy: Classical School & Headless UI Testing (Eede)

Eede では **古典派 (Detroit School / Classical School)** のテスト戦略に従い、リファクタリング耐性と高い信頼性を担保します。

---

## 1. 黄金律: リファクタリング耐性 (Refactoring Resistance)
- **公開 API の振る舞いを検証せよ**: プライベートメソッドや内部の実装詳細（フィールド値）を直接テストせず、公開メソッドやプロパティの出力・状態変化をテストせよ。
- **脆いテスト (Brittle Tests) の排除**: 内部実装を変更しただけで（外部の振る舞いは変わっていないのに）失敗するテストは悪である。テストはブラックボックスとして書くこと。

---

## 2. モック方針 (Mocking Rules)

依存オブジェクトを厳格に区別せよ：

### A. 管理下依存 (Managed Dependencies) -> **モック禁止（実体を使用）**
- **対象**: Domain モデル（`Picture`, `Position`, `PictureArea`, `DrawingSession`, `AnimationPattern` 等）、不変な値オブジェクト、純粋関数。
- **原則**: 常に実体（具象インスタンス）を用いてテストすること。ドメインオブジェクトをモックすると、結合バグが見逃されテストが実装詳細に癒着する。

### B. 管理外依存 (Unmanaged Dependencies) -> **モック推奨（インターフェース経由）**
- **対象**: ファイルシステム（`IFileSystem`, `IPictureFileIO`）、Velopack アップデート（`IAppUpdater`, `IUpdateManagerWrapper`）、ブラウザ起動（`IExternalBrowserLauncher`）、OS クリップボード（`IClipboard`）。
- **原則**: インターフェース（DIP）を抽出し、Moq でモックして決定論的にテストすること。

---

## 3. Avalonia.Headless.NUnit による UI / E2E 結合テスト

Eede では `Avalonia.Headless.NUnit`（`[AvaloniaTest]`）を利用した Headless テストが標準装備されています。

### テストパターン:
1. **ViewModel + UseCase 結合テスト**:
   - `MainViewModel` や `WelcomeViewModel` にモックサービスを注入し、コマンド実行や Interaction（ダイアログ、クローズ確認）の非同期フローをテストする。
2. **Headless XAML View 結合テスト**:
   - `Window` に `WelcomeView` や `PictureContainer` を載せて `window.Show()` し、Visual Tree（`GetVisualDescendants()`）からボタン等の UI コントロールを取得して、`IsVisible` や `Text`、バインディングの連動を検証する。

```csharp
[AvaloniaTest]
public async Task UpdateFlow_WhenUpdateAvailable_ShouldShowApplyButton()
{
    // Arrange
    var vm = new WelcomeViewModel(...);
    var view = new WelcomeView { DataContext = vm };
    var window = new Window { Content = view };
    window.Show();

    // Act
    ...

    // Assert
    var applyButton = view.GetVisualDescendants().OfType<Button>()
        .FirstOrDefault(b => b.Command == vm.ApplyUpdateCommand);
    Assert.That(applyButton!.IsVisible, Is.True);
}
```

---

## 4. テスト構造: AAA (Arrange-Act-Assert) パターン
全テストは明確に Arrange, Act, Assert の 3 フェーズで記述すること。
複数のアサーションを行う場合は `Assert.Multiple(() => { ... });` を活用すること。

---

## 5. Jules 流エッジケース・例外パス・境界値テスト網羅規律
新規クラス作成時やリファクタリング時は、ハッピーパスだけでなく以下のエッジケーステストを義務付ける：
1. **コンストラクタの Null ガード**: null 非許容引数に対する `ArgumentNullException` のスロー検証。
2. **境界値・無効入力**: インデックス境界外（`ArgumentOutOfRangeException`）や無効スキーム・フォーマット（`ArgumentException`）の検証。
3. **例外発生時のフォールバック**: 外部依存（ファイル・通信・復元データ等）が破損・失敗した際に、例外フィルターで補足され安全にフォールバック/状態遷移するかの検証。
4. **非同期テストの健全性**: テスト内での `Task.Wait()` や `.Result` による同期ブロッキングを厳禁とし、`async Task` と `await` で記述すること。

---

## 6. テスト保守性・カプセル化・プラットフォーム互換規約 (Test Maintainability & Integrity)

### A. バグ追従テスト (Bug-Tracing Test) の排除と整数除算ゼロ丸め境界
- **整数除算の罠**: C# の整数除算がゼロ方向へ丸められる（Truncate toward zero）特性により、オフセット手前の負数差分（例: `-4 / 10 = 0`）が 0 と評価され、境界内と誤判定されるバグが発生しやすい。
- **バグ追従テストの禁止**: 既存コードの不具合挙動に合わせて「負数なのに 0 が返ることを期待値とするテスト」を固定化してはならない。テスト Fail 時にテスト入力を変更して誤魔化すクイックハックを厳禁とし、ドメインの物理契約（オフセット手前は `-1`）としてプロダクトコードのバグを根本解決すること。

### B. テスト都合の可視性変更禁止とカプセル化保護
- テスト側から直接インスタンス化や検証を行いたいという理由だけで、プロダクトコードのクラスやメソッドを `internal` から `public` へ漏出させてはならない。
- 必ず `.csproj` に `<InternalsVisibleTo Include="Eede.Tests" />` を設定し、カプセル化を厳格に保護した上で `internal` な型・コンストラクタをテストすること。

### C. テストヘルパーのクロスプラットフォーム化と CA1416 警告根絶
- プロダクトコードが SkiaSharp 等でクロスプラットフォーム化されているにもかかわらず、テストヘルパー（`PictureHelper` 等）が古い Windows 固有 API（`System.Drawing.Common` 等）に依存してビルド警告（CA1416）を垂れ流すアンチパターンを根絶すること。
- プロダクトコード自身のコーデック（`SkiaSharpPictureCodec` 等）をテストヘルパー内で直接再利用し、CA1416 警告ゼロ化と同時に Classical Testing（実体契約テスト）として機能させること。

### D. Avalonia Headless テストの `[AvaloniaTest]` 属性と Inconclusive 負債管理
- Avalonia の UI コントロールやレンダリングオブジェクト（`WriteableBitmap` 等）を検証するテストでは、通常の `[Test]` ではなく必ず `[AvaloniaTest]` 属性を用いて `IPlatformRenderInterface` の初期化を確実に行うこと。
- `Assert.Inconclusive` によるテストのスキップはテストランナー上で「合格」に見える偽陽性を生むため、放置せず解消するか、技術的負債として明示的に管理すること。

### E. C# / NUnit テストスイート高密度集約規律 (NUnit Test Consolidation Protocol)
- 正常系・例外系ごとにクラス乱立したボイラープレートテスト（1クラス1メソッドの分散ファイル）は、NUnit の `[TestCase]` を駆使してドメイン／ユースケース単位の単一テストファイルへ高密度集約する。
- 単なるハッピーパスへの後退（カバレッジデグレード）は禁止とし、null ガード、境界値（0 / 中間 / 末尾）、範囲外例外などの契約検証をテーブル駆動で 100% 保持したままファイル数を削減する。

### F. Infrastructure 層カバレッジ向上戦略 (Infrastructure Logic Coverage Protocol)
- WASM JSInterop やインストーラ等の環境・プラットフォーム依存コードを無理にモック化せず、ファイルパーサー（バイナリリーダー等）やデータリポジトリなどの純粋アルゴリズム・データ変換ロジックを優先的にテスト対象とする。
- 切り詰めデータやオフセット境界値のテストを網羅することで、未カバーだった例外パスを安全に攻略し、プロダクトコードの脆弱性（負数インデックス例外等）を未然に防止・修復する。

---

## 7. テスト実行静寂化とトークン防衛規律 (Quiet Testing & Token Defense)

### A. dotnet test 静寂化オプションの徹底
- `dotnet test` 実行時は、不要なバナーや通過ログによるコンテキスト汚染を遮断するため、原則として以下の静寂化フラグを併用せよ：
  `dotnet test --nologo -v q --logger "console;verbosity=minimal"`
- 修正箇所の検証時は `--filter FullyQualifiedName~<ClassName>` で対象テストクラスに限定して実行せよ。

### B. 巨大テストファイルのピンポイント・ウィンドウ規約
- 300行を超えるテストファイルを引数なしでいきなり全体 `view_file` することを厳禁とする。
- PowerShell の `Select-String -Path <file> -Pattern "\[(Test|TestCase|AvaloniaTest)\]"` でテスト一覧を特定し、目的のテストブロック前後100行（`StartLine`/`EndLine`）のみをピンポイント取得せよ。

### C. マルチターゲットフレームワーク（net10.0;net10.0-windows）テストハング防止規律
- ソリューションが `TargetFrameworks: net10.0;net10.0-windows` のマルチターゲットかつ Avalonia Headless を含む場合、引数なしの `dotnet test` は両フレームワークのテストホストが並行起動し、UI ディスパッチャの競合やハングを引き起こすリスクがある。
- 高速かつ決定論的な検証を行う際は、必ず `-f net10.0` を明示指定し、`dotnet test -f net10.0 --filter "FullyQualifiedName~<ClassName>"` で単一フレームワークかつピンポイントに対象テストを実行すること。


