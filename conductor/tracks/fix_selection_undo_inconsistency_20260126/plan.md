# Implementation Plan: 範囲選択ツールのドラッグ移動後におけるアンドゥ時の不整合修正

DrawingSession の履歴管理に選択範囲のメタデータを追加し、UI側の選択枠と同期させることで、アンドゥ時の不整合を解消します。

## 専門家のアドバイス (Expert Consultation)
- unit-test-architect: DrawingSession の単体テストを強化し、画像と選択範囲が常にペアで復元されることを検証する。
- efactor-legacy-code: データ構造の変更に伴う影響範囲（呼び出し元）を特定し、型安全に修正する。

## Phase 0: 準備とバグ再現テスト
現在の不具合をコードレベルで再現し、修正後のパスを保証します。

- [ ] Task: Eede.Domain.Tests に DrawingSession の履歴管理（選択範囲保持）に関するテストを追加
- [ ] Task: Eede.Presentation.Tests で、アンドゥ時に SelectingArea が更新されない現在の挙動を確認するテストを追加（現在は失敗するはず）
- [ ] Task: Conductor - User Manual Verification 'Phase 0' (Protocol in workflow.md)

## Phase 1: DrawingSession の拡張 (Domain) [checkpoint: 38b10a6]
履歴スタックに選択範囲の情報を含めるようにデータ構造を修正します。

- [x] Task: 履歴アイテムを保持するための内部レコード `HistoryItem(Picture Picture, PictureArea? SelectingArea)` を定義
- [x] Task: `UndoStack` / `RedoStack` の型を `ImmutableStack<HistoryItem>` に変更
- [x] Task: `Push`, `Undo`, `Redo` メソッドを `HistoryItem` を扱うように修正
- [x] Task: `CurrentSelectingArea` プロパティを追加し、現在のセッションにおける選択範囲を公開
- [x] Task: Phase 0 のドメインテストをパスさせる
- [x] Task: Conductor - User Manual Verification 'Phase 1' (Protocol in workflow.md)

## Phase 2: UseCase と Provider の更新 (Application)
ドメイン層の変更をアプリケーション層へ反映します。

- [ ] Task: IDrawingSessionProvider および実装クラスを更新し、セッション更新時に選択範囲も考慮するように修正
- [ ] Task: IPictureEditingUseCase (および実装) の各メソッドで、更新後の選択範囲を DrawingSession に渡せるように拡張
- [ ] Task: Conductor - User Manual Verification 'Phase 2' (Protocol in workflow.md)

## Phase 3: ViewModel の同期と最終確認 (Presentation)
UI側のプロパティをセッション状態と同期させます。

- [ ] Task: DrawableCanvasViewModel で DrawingSessionProvider.SessionChanged を購読し、SelectingArea を同期
- [ ] Task: DrawingSessionViewModel からの Push 時に、現在の SelectingArea を含めるように修正
- [ ] Task: 全てのテスト（Phase 0 の再現テスト含む）を実行し、修正を確認
- [ ] Task: Conductor - User Manual Verification 'Phase 3' (Protocol in workflow.md)
