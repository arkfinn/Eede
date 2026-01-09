# 選択範囲の移動機能 実装プラン

## Phase 1: ドメインロジックの強化
選択範囲の移動に伴う画像データの操作ロジックを `Picture` クラスおよび `Selection` 関連クラスに実装します。

- [~] Task: `Selection` クラスの拡張 (はみ出し判定などの追加)
    - [ ] Write Tests
    - [ ] Implement
- [ ] Task: `Picture` クラスへの移動用メソッドの追加
    - [ ] Write Tests
    - [ ] Implement
- [ ] Task: Conductor - User Manual Verification 'Phase 1: ドメインロジック' (Protocol in workflow.md)

## Phase 2: UI状態遷移の実装
`ISelectionState` を使用したドラッグ移動の状態遷移ロジックを実装します。

- [ ] Task: `DraggingState` の詳細実装 (プレビュー情報の提供)
    - [ ] Write Tests
    - [ ] Implement
- [ ] Task: `SelectedState` から `DraggingState` への遷移実装 (ドラッグ開始時に元のピクセルを消去するロジック含む)
    - [ ] Write Tests
    - [ ] Implement
- [ ] Task: ドラッグ中のキャンセル処理の実装 (右クリック)
    - [ ] Write Tests
    - [ ] Implement
- [ ] Task: Conductor - User Manual Verification 'Phase 2: UI状態遷移' (Protocol in workflow.md)

## Phase 3: UI表示とViewModelの連携
ドラッグ中のプレビュー表示と、確定時の画像更新処理を実装します。

- [ ] Task: `PictureContainer` でのプレビュー表示の実装
    - [ ] Write Tests (ViewModel連携のモックテスト)
    - [ ] Implement (Avalonia UI)
- [ ] Task: 移動確定時の `ViewModel` 連携と Undo 登録
    - [ ] Write Tests
    - [ ] Implement
- [ ] Task: Conductor - User Manual Verification 'Phase 3: UI表示と連携' (Protocol in workflow.md)
