# トラック仕様: Serviceクラスの撲滅とドメイン知識の返却

## 概要
現在 `Eede.Application/Services` やプロジェクト内の各所に散在している「Service」クラスを解体・整理します。
ドメイン知識（状態管理や計算ロジック）を適切なドメインオブジェクト（集約、Value Object）へ返却し、副作用を伴う操作は UseCase または Infrastructure Port へ再配置することで、ドメインモデル貧血症を解消し、テスト容易性と保守性を向上させます。

## 機能要件

### 1. AnimationService の解体と再構築
- **ドメイン集約の導入**: `AnimationService` が保持していた `List<AnimationPattern>` を、ドメイン層の新しい集約（例: `AnimationPatterns` または `AnimationSequence`）に移動します。
- **UseCase 化**: `Add`, `Replace`, `Remove` 等の操作を、それぞれ独立した UseCase（例: `AddAnimationPatternUseCase`）として実装します。
- **状態管理の統合**: 現在のパターンリストの状態管理を、既存のセッション管理の仕組みに統合します。

### 2. ICanvasBackgroundService の整理
- **Value Object 化**: 背景の定義（グリッド、透明パターン等）をドメイン層の Value Object（`CanvasBackgroundStyle`）として定義します。
- **計算ロジックの抽出**: 背景描画に必要な計算（グリッド座標等）を `BackgroundCalculator` として抽出し、UIフレームワークに依存せずテスト可能にします。
- **View への委譲**: 実際の `Graphics` を使用した描画処理は、View（または Presentation 層の描画アダプター）に移動し、Application 層からの `System.Drawing` 依存を排除します。

### 3. インフラ系サービスの Port/Adapter 化
- **ディレクトリ整理**: `Eede.Application/Services` を廃止し、インターフェースを機能に応じたディレクトリ（例: `Eede.Application/Infrastructure`）へ移動します。
- **名称変更**: `IClipboardService` → `IClipboard`、`IStorageService` → `IFileStorage` 等、具体的で簡潔な名称に変更します。
- **UseCase 経由の利用**: ViewModel からこれらのサービスを直接呼び出すのではなく、必ず UseCase を介して利用するようにリファクタリングします。

## 非機能要件
- **TDD (Test-Driven Development)**: リファクタリング前に既存の挙動を固定するテストを記述し、変更後も挙動が変わらないことを担保します。
- **オニオンアーキテクチャの遵守**: Application 層が特定の UI フレームワークや描画ライブラリに依存しないようにします。

## 完了条件 (Acceptance Criteria)
- [ ] `Eede.Application/Services` ディレクトリが削除されていること。
- [ ] `AnimationService` が解体され、ドメイン集約と UseCase に分割されていること。
- [ ] `ICanvasBackgroundService` が廃止され、背景設定が VO 化および描画処理が Presentation 層に移動していること。
- [ ] `IClipboard` 等の外部サービス利用がすべて UseCase を経由していること。
- [ ] すべてのユニットテストがパスし、カバレッジが維持（または向上）していること。

## アウトオブスコープ
- アニメーション機能自体の新機能追加。
- 描画エンジン自体の Avalonia ネイティブ描画への完全移行（今回は依存の整理に留める）。
