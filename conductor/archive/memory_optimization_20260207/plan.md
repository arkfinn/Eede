# 実装計画: 巨大画像対応のためのメモリ最適化と差分Undo (Legacy Refactoring Style)

## フェーズ 0: 全体的なセーフティネットの確認
- [x] Task: Git ステータスの確認 (Phase 1: Assess & Secure) [40d6684]
    - [x] `git status` を実行し、クリーンな状態であることを確認する。

## フェーズ 1: ビットマップリソース管理の徹底 [checkpoint: 47d9736]
### ステップ 1-1: 現状の振る舞いの固定 (Phase 2: Lock Down)
- [x] Task: `DockPictureViewModel` の仕様化テストの作成
- [x] Task: `MainViewModel` (DrawableCanvasViewModel) の仕様化テストの作成
- [x] Task: `AnimationViewModel` の仕様化テストの作成
### ステップ 1-2: 改善の実装 (Phase 3: Divide & Conquer)
- [x] Task: `DockPictureViewModel` の Bitmap 破棄実装 (既に一部実装済み、確認と強化) [b1e5726]
- [x] Task: `MainViewModel` (DrawableCanvasViewModel) の Bitmap 破棄実装 (既に一部実装済み、確認と強化) [b1e5726]
- [x] Task: `AnimationViewModel` の Bitmap 破棄実装 [b1e5726]
- [x] Task: 改善後のテスト通過確認 [b1e5726]
- [x] Task: Conductor - User Manual Verification 'フェーズ 1: ビットマップリソース管理' (Protocol in workflow.md) [47d9736]

## フェーズ 2: GPUスケーリングへの移行 [checkpoint: 3b288d7]
### ステップ 2-1: 現状の描画フローの固定
- [x] Task: 拡大描画結果の仕様化テスト (Golden Master) [3b288d7]
### ステップ 2-2: スケーリング方式の切り替え
- [x] Task: ViewModel への表示サイズプロパティ追加 (既に一部実装済み、確認と修正) [3b288d7]
- [x] Task: View (XAML) のスケーリング設定適用 (既に一部実装済み、確認と修正) [3b288d7]
- [x] Task: ドメイン層からの拡大処理排除 [3b288d7]
- [x] Task: スナップショット比較テストの実行 [3b288d7]
- [x] Task: Conductor - User Manual Verification 'フェーズ 2: GPUスケーリングへの移行' (Protocol in workflow.md) [3b288d7]

## フェーズ 3: 差分Undoインフラの構築 [checkpoint: 74aa0bd]
### ステップ 3-1: 現状の履歴システムの固定
- [x] Task: `DrawingSession` Undo/Redo の仕様化テスト [74aa0bd]
### ステップ 3-2: 差分保存の実装
- [x] Task: `DiffPictureHistory` の実装と統合 (DiffHistoryItem として実装) [74aa0bd]
- [x] Task: 仕様化テストの実行 [74aa0bd]
- [x] Task: Conductor - User Manual Verification 'フェーズ 3: 差分Undoインフラ' (Protocol in workflow.md) [74aa0bd]

## フェーズ 4: ツールの差分報告対応 [checkpoint: 43f652c]
### ステップ 4-1: 各ツールの描画結果の固定
- [x] Task: 各ツール（ペン、矩形、塗りつぶし）の描画結果の仕様化テスト [03b80da]
### ステップ 4-2: 差分報告への移行
- [x] Task: `IDrawingTool` インターフェース拡張と各ツールの実装更新 (IDrawStyle を拡張) [03b80da]
- [x] Task: 仕様化テストによる描画結果の不変確認 [03b80da]
- [x] Task: Conductor - User Manual Verification 'フェーズ 4: ツールの差分報告対応' (Protocol in workflow.md) [43f652c]

## フェーズ 5: 最終統合と品質確認 [checkpoint: 2435eff]
- [x] Task: 巨大画像によるメモリ負荷検証 [2435eff]
- [x] Task: 不要になったコードの削除と最終リファクタリング [2435eff]
- [x] Task: Conductor - User Manual Verification 'フェーズ 5: 最終確認' (Protocol in workflow.md) [2435eff]
