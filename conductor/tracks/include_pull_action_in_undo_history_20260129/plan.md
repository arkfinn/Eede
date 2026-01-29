# Implementation Plan: Include Pull Action in Undo History (TDD & Legacy Code Approach)

## Phase 0: Safety Net Construction (仕様化テストによる保護)
**目標:** 既存の `DrawingSession` の振る舞いをテストコードで完全に固定し、リファクタリング中の退行（デグレ）を防ぐ。

- [x] Task: `DrawingSession` の既存の Undo/Redo 動作に対する仕様化テスト（Characterization Tests）を作成する。 [439dc3a]
    - [ ] `Push` 後の `CurrentPicture` の状態遷移。
    - [ ] `Undo` / `Redo` 後の `CurrentPicture` と `SelectingArea` の復元確認。
    - [ ] `Push` が連続した場合のスタックの挙動確認。
- [x] Task: Conductor - User Manual Verification 'Phase 0: Safety Net' (Protocol in workflow.md) [checkpoint: d03e710]

## Phase 1: Polymorphic History Model (TDDによるドメイン拡張)
**目標:** テスト駆動で新しい履歴モデルを導入し、`DrawingSession` が異種の履歴を扱えるようにする。

- [x] Task: 履歴アイテムの型定義を作成する。 [8bbc0cd]
    - [ ] `IHistoryItem` インターフェースを定義。
    - [ ] `CanvasHistoryItem` を作成し、既存の `Picture` + `PictureArea` を保持させる。
    - [ ] `DockActiveHistoryItem` を作成し、`DockId`, `Position`, `Picture`(差分) を保持させる。
- [ ] Task: `DrawingSession` のリファクタリング (TDD)。
    - [ ] **Red:** `DrawingSession` に `PushDockUpdate` メソッドを追加し、それを呼んだ後に `Undo` しても `CurrentPicture` (作業エリア) が変わらないことを期待するテストを書く。
    - [ ] **Green:** `DrawingSession` 内部のスタックを `IHistoryItem` 型に変更し、`PushDockUpdate` と `Undo` の最小実装を行う。
    - [ ] **Refactor:** 既存の `Push` メソッドも内部で `CanvasHistoryItem` を使うように変更する（既存テストが通ることを維持）。
    - [ ] **Red:** `Undo` 時に「何がUndoされたか」を知るための戻り値 (`UndoResult`) をテストで要求する。
    - [ ] **Green:** `Undo` / `Redo` のシグネチャを変更（または拡張）し、Undoされたアイテム情報を返すように実装する。
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Domain Model' (Protocol in workflow.md)

## Phase 2: Application Layer Integration (振る舞いの接続)
**目標:** ドメインの変更をアプリケーション層に波及させ、実際の機能として動作させる。

- [ ] Task: `MainViewModel` と `DockPictureViewModel` の連携テストを作成する。
    - [ ] `DockPictureViewModel` の特定（ID）と更新が正しく行われるかのテスト。
- [ ] Task: `DockPictureViewModel` に一意な ID を実装する。
- [ ] Task: `MainViewModel` の `Pull` 処理を修正する。
    - [ ] 差分抽出ロジックの実装（テスト必須）。
    - [ ] `DrawingSession` への Push 処理追加。
- [ ] Task: Undo/Redo 時のイベントハンドリング実装。
    - [ ] `DrawingSession` から返された `DockActiveHistoryItem` を元に、対象ドック画像を復元する処理。
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Integration' (Protocol in workflow.md)
