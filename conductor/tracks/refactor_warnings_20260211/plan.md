# 実装計画: ビルド時のワーニング解消とレガシーコードのリファクタリング

## フェーズ 0: 現状固定 (Safety Net) [checkpoint: 2322c10]
リファクタリングによる先退（デグレード）を防ぐため、主要プロジェクトのテスト基盤を強化する。

- [x] Task: 既存テストの実行とベースライン確認 (eb57cdf)
- [x] Task: `Eede.Domain` の主要クラス (`DrawingSession`, `Coordinate`, `Picture`) に対する仕様化テストの拡充 (9034bf2)
- [x] Task: `Eede.Presentation` の ViewModel における重要プロパティの挙動を確認するテストの追加 (9034bf2)
- [x] Task: Conductor - User Manual Verification 'フェーズ 0: 現状固定 (Safety Net)' (Protocol in workflow.md)

## フェーズ 1: ドメイン層の段階的 Nullable 有効化
依存関係の最内部から Null 安全を確立し、ドメインモデルを洗練させる。

- [x] Task: `Eede.Domain.SharedKernel` および `Files` の `#nullable enable` 化と警告解消 (d9863dc)
- [x] Task: `Eede.Domain.ImageEditing` および `Pictures` の `#nullable enable` 化と警告解消 (2059fe9)
- [ ] Task: `Eede.Domain` プロジェクト全体の `Nullable` 設定を `enable` に変更
- [ ] Task: 警告解消に伴うドメインモデルの改善（コンストラクタ注入の強化、Null Object パターンの適用）
- [ ] Task: Conductor - User Manual Verification 'フェーズ 1: ドメイン層の段階的 Nullable 有効化' (Protocol in workflow.md)

## フェーズ 2: アプリケーション・インフラ層の刷新
ドメイン層の変更を上位層へ伝播させ、古い API を更新する。

- [ ] Task: `Eede.Application` の `#nullable enable` 化と警告解消
- [ ] Task: `Eede.Infrastructure` のリファクタリングと非推奨 API (`Obsolete`) の解消
- [ ] Task: 外部ライブラリ (Avalonia 等) の更新に伴う影響調査と対応
- [ ] Task: Conductor - User Manual Verification 'フェーズ 2: アプリケーション・インフラ層の刷新' (Protocol in workflow.md)

## フェーズ 3: プレゼンテーション層の警告一掃
下位層の Null 安全化の恩恵を受け、UI層の大量の警告を効率的に解消する。

- [ ] Task: `Eede.Presentation` (ViewModels) の `#nullable enable` 化と警告解消
- [ ] Task: `Eede.Presentation` (Views) のコードビハインドにおける警告解消
- [ ] Task: 全プロジェクトに対する `.editorconfig` に基づくコードスタイルの一括適用
- [ ] Task: 未使用コード (using, 変数) の削除と最新 C# 構文への置換
- [ ] Task: Conductor - User Manual Verification 'フェーズ 3: プレゼンテーション層の警告一掃' (Protocol in workflow.md)

## フェーズ 4: 最終検証とクリーンアップ
全ての警告が解消され、品質が向上したことを確認する。

- [ ] Task: 全テストスイートの最終実行とカバレッジ確認
- [ ] Task: `dotnet build` を実行し、警告が 0 であることを確認
- [ ] Task: Conductor - User Manual Verification 'フェーズ 4: 最終検証とクリーンアップ' (Protocol in workflow.md)
