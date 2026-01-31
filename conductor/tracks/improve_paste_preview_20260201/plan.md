# 実装計画: 非破壊プレビュー付きコピー＆ペーストの改善 (DDD & レガシー改善)

## フェーズ 0: 安全網の構築（仕様化テスト）
現在の挙動を壊さないためのベースラインを作成します。

- [x] Task: 既存の `PasteUseCase` の挙動を固定するテストを作成
- [x] Task: Git のクリーン状態を確認し、リファクタリングの準備を整える
- [ ] Task: Conductor - User Manual Verification 'Phase 0' (Protocol in workflow.md)

## フェーズ 1: ドメイン層の洗練（DDD 知識の集約）
`DrawingSession` を中心としたドメインモデルを、今回の新しい仕様（非破壊プレビュー）に適合するように拡張します。

- [ ] Task: `SelectionPreviewInfo` VO の拡張
    - [ ] `IsNonDestructive`（非破壊的かどうか）または `PreviewType` を導入し、移動（破壊的）とペースト（非破壊的）を区別できるようにする
- [ ] Task: `DrawingSession` 集約のビジネスロジック実装
    - [ ] `PushPastePreview()` メソッドの実装：外部からビットマップを受け取り、キャンバスを汚さずにプレビューとして保持する
    - [ ] `CommitPreview()` メソッドの整理：プレビュー中のみ実行可能で、実行時に初めてキャンバスを更新する
- [ ] Task: **古典派テストの実装**: `DrawingSession` に対して「入力（ビットマップ）を与え、状態（PreviewInfo）が正しく変化し、確定後にキャンバスが期待通り更新される」ことを検証するテストを作成
- [ ] Task: Conductor - User Manual Verification 'Phase 1' (Protocol in workflow.md)

## フェーズ 2: アプリケーション層の疎結合化（UseCase リファクタリング）
UseCase クラスを 1 クラス 1 メソッドの原則に従い、ドメインロジックの呼び出しに専念させます。

- [ ] Task: `PasteUseCase` の修正
    - [ ] 直接の `Draw` 命令を排除し、`DrawingSession.PushPastePreview()` の呼び出しに置き換える
- [ ] Task: **統合テストの修正**: フェーズ 0 で作成したテストを、新しい仕様（ペースト直後は未確定であること）に合わせて更新し、Green にする
- [ ] Task: Conductor - User Manual Verification 'Phase 2' (Protocol in workflow.md)

## フェーズ 3: プレゼンテーション層の整合（自動確定と UI 連携）
UI 側の都合による「確定トリガー」を、ドメイン層の `CommitPreview` に正しく委譲します。

- [ ] Task: `DrawableCanvasViewModel` の改善
    - [ ] ツール切り替え、範囲外クリック等のイベントを `CommitPreview` ユースケース（または既存の確定処理）へ適切に中継するロジックを実装
- [ ] Task: UI 状態の同期
    - [ ] プレビュー確定時、またはキャンセル（Undo）時に、ViewModel の状態がドメイン層と不整合を起こさないように同期する
- [ ] Task: **UI コンポーネントテスト**: Headless 環境を使用して、クリック操作等で正しく確定処理が走ることを検証
- [ ] Task: Conductor - User Manual Verification 'Phase 3' (Protocol in workflow.md)

## フェーズ 4: 最終的な整合性確認とクリーンアップ
- [ ] Task: Undo 1回でペースト操作全体がキャンセルされ、キャンバスが汚れていないことを再確認
- [ ] Task: **リファクタリング**: プレビューに関連する重複コードを VO や Entity 内で共通化し、DRY 原則を適用
- [ ] Task: Conductor - User Manual Verification 'Phase 4' (Protocol in workflow.md)
