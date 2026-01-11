# 選択範囲の移動機能 実装プラン (作業エリア版)

## Phase 1: ドメインロジックの強化 [checkpoint: 8759d7a]
選択範囲の移動に伴う画像データの操作ロジックを `Picture` クラスおよび `Selection` 関連クラスに実装します。

- [x] Task: `Selection` クラスの拡張 (はみ出し判定などの追加) (55eaf98)
- [x] Task: `Picture` クラスへの移動用メソッドの追加 (05bc3b6)
- [x] Task: Conductor - User Manual Verification 'Phase 1: ドメインロジック' (Protocol in workflow.md) (8759d7a)

## Phase 2: 作業エリア統合の修正 (座標・倍率対応)
`DrawableCanvasViewModel` における移動ロジックを修正し、ズーム倍率に関わらず正しく動作するようにします。

- [x] Task: `DrawableCanvasViewModel` への `ISelectionState` の導入と基本遷移
- [x] Task: 描画ツールとの干渉解消および右クリックキャンセル
- [x] Task: ズーム（倍率）を考慮した座標計算の完全修正 (eeaccca)
    - [x] Write Tests
    - [x] Implement
- [~] Task: ドロップ確定時の座標精度の修正
    - [ ] Write Tests
    - [ ] Implement
- [ ] Task: Conductor - User Manual Verification 'Phase 2: 座標・倍率対応' (Protocol in workflow.md)

## Phase 3: プレビュー表示と Undo 連携
ドラッグ中のプレビュー表示の修正と、選択範囲の座標復旧を含む Undo 登録を実装します。

- [ ] Task: ズーム対応した移動プレビュー表示の修正
    - [ ] Write Tests
    - [ ] Implement
- [ ] Task: 選択範囲の座標を含む Undo 登録の実装
    - [ ] Write Tests
    - [ ] Implement
- [ ] Task: Conductor - User Manual Verification 'Phase 3: 最終確認' (Protocol in workflow.md)
