# 実装計画：DrawableCanvasViewModel の責務分離（第一段階：表示計算の純粋化）

## フェーズ 1: 現状分析とテスト基盤の整備
- [ ] Task: `DrawableCanvasViewModel` のプロパティ依存関係の精査
- [ ] Task: 既存の表示計算を検証する単体テストの作成
- [ ] Task: Conductor - User Manual Verification 'フェーズ 1: 現状分析とテスト基盤の整備' (Protocol in workflow.md)

## フェーズ 2: 計算ロジックの抽出（Humble Object 化）
- [ ] Task: `CanvasViewCalculator` クラス（純粋なC#クラス）の作成
- [ ] Task: `CanvasViewCalculator` に対する単体テストの実装
- [ ] Task: Conductor - User Manual Verification 'フェーズ 2: 計算ロジックの抽出（Humble Object 化）' (Protocol in workflow.md)

## フェーズ 3: ViewModel のリファクタリング
- [ ] Task: `DrawableCanvasViewModel` への `CanvasViewCalculator` の導入
- [ ] Task: 冗長な計算プロパティの置き換え
- [ ] Task: 不要なプロパティの整理
- [ ] Task: Conductor - User Manual Verification 'フェーズ 3: ViewModel のリファクタリング' (Protocol in workflow.md)

## フェーズ 4: 最終確認とクリーンアップ
- [ ] Task: 全単体テストの実行
- [ ] Task: 手動動作確認
- [ ] Task: Conductor - User Manual Verification 'フェーズ 4: 最終確認とクリーンアップ' (Protocol in workflow.md)
