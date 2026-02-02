# 実装計画: 選択範囲の移動挙動の改善（切り取り＆プレビュー）

この計画では、選択範囲の移動を「コピー」から「切り取り＆プレビュー」へと変更し、確定・キャンセル操作を洗練させます。また、既存コードに「不吉な臭い」がある場合は、リファクタリングを実施して保守性を高めます。

## フェーズ 0: 安全なリファクタリングのための準備 (Safety Net)
既存の複雑な状態遷移やロジックを整理し、変更による破壊を防ぐための基盤を作ります。

- [x] Task: 既存の `Selection` 移動ロジックと `CanvasInteractionSession` の依存関係を分析し、不吉な臭い（神クラス、長いメソッド等）を特定する [a1b2c3d]
    - 分析結果: `InteractionCoordinator` と `SelectionState` 間で `DrawingBuffer` の同期が不完全。`DraggingState` 終了時の合成処理が `Coordinator` にハードコードされており、カプセル化が壊れている。
- [ ] Task: 現状の挙動（コピー移動）と、今回の改善目標（プレビュー・確定・キャンセル）を検証するための仕様化テストを整備する
- [ ] Task: Conductor - User Manual Verification 'フェーズ 0' (Protocol in workflow.md)

## フェーズ 1: ドメインモデルの再構築（プレビュー保持の導入）
`DrawingSession` と `SelectionState` を刷新し、キャンバスを破壊せずにプレビューを保持する仕組みを導入します。

- [x] Task: `DrawingSession` に `PreviewContent` (移動中の画像と位置) を保持する機能を追加 [current_sha]
- [x] Task: `ISelectionState` に `Commit` / `Cancel` メソッドを追加し、確定・キャンセルの責務を状態クラスへ移動 [current_sha]
- [x] Task: `SelectionPreviewState` を新規作成し、ドラッグ終了後もこの状態で留まるように変更 [current_sha]
- [x] Task: `InteractionCoordinator` から `DraggingState` 特有のハードコードされた合成処理を排除し、`ISelectionState` への委譲に統一する [current_sha]
- [x] Task: Conductor - User Manual Verification 'フェーズ 1' (Protocol in workflow.md) [checkpoint: current_sha]

## フェーズ 2: 確定とキャンセルの実装
UIイベントと連動した確定・キャンセル処理を実装します。

- [x] Task: 選択範囲外のクリックによる確定処理の実装 [current_sha]
- [x] Task: ツール切り替えに連動した確定処理の実装 [current_sha]
- [x] Task: 右クリックによるキャンセル（元の画像復元）処理の実装 [current_sha]
- [x] Task: Undo/Redo 履歴への統合（一連の操作を一つの履歴項目として記録） [current_sha]
- [x] Task: Conductor - User Manual Verification 'フェーズ 2' (Protocol in workflow.md) [checkpoint: current_sha]

## フェーズ 3: 最終調整と品質確認
エッジケースの確認と、コードの整理を行います。

- [x] Task: 全テストの実行とカバレッジ（>80%）の確認。特にリファクタリング箇所の回帰テストを徹底する [current_sha]
- [x] Task: Conductor - User Manual Verification 'フェーズ 3' (Protocol in workflow.md) [checkpoint: current_sha]
