# Implementation Plan: Include Pull Action in Undo History (TDD & Legacy Code Approach)

## Phase 0: Safety Net Construction (仕様化テストによる保護)
**目標:** 既存の `DrawingSession` の振る舞いをテストコードで完全に固定し、リファクタリング中の退行（デグレ）を防ぐ。

- [x] Task: `DrawingSession` の既存の Undo/Redo 動作に対する仕様化テスト（Characterization Tests）を作成する。 [439dc3a]
    - [x] `Push` 後の `CurrentPicture` の状態遷移。
    - [x] `Undo` / `Redo` 後の `CurrentPicture` と `SelectingArea` の復元確認。
    - [x] `Push` が連続した場合のスタックの挙動確認。
- [x] Task: Conductor - User Manual Verification 'Phase 0: Safety Net' (Protocol in workflow.md) [checkpoint: d03e710]

## Phase 1: Polymorphic History Model (TDDによるドメイン拡張)
**目標:** テスト駆動で新しい履歴モデルを導入し、`DrawingSession` が異種の履歴を扱えるようにする。

- [x] Task: 履歴アイテムの型定義を作成する。 [8bbc0cd]
    - [ ] `IHistoryItem` インターフェースを定義。
    - [ ] `CanvasHistoryItem` を作成し、既存の `Picture` + `PictureArea` を保持させる。
    - [ ] `DockActiveHistoryItem` を作成し、`DockId`, `Position`, `Picture`(差分) を保持させる。
- [x] Task: `DrawingSession` のリファクタリング (TDD)。 [46196c2]
    - [x] **Red:** `DrawingSession` に `PushDockUpdate` メソッドを追加し、それを呼んだ後に `Undo` しても `CurrentPicture` (作業エリア) が変わらないことを期待するテストを書く。
    - [x] **Green:** `DrawingSession` 内部 Sora のスタックを `IHistoryItem` 型に変更し、`PushDockUpdate` と `Undo` の最小実装を行う。
    - [x] **Refactor:** 既存の `Push` メソッドも内部で `CanvasHistoryItem` を使うように変更する（既存テストが通ることを維持）。
    - [x] **Red:** `Undo` 時に「何がUndoされたか」を知るための戻り値 (`UndoResult`) をテストで要求する。 [c5b6627]
    - [x] **Green:** `Undo` / `Redo` のシグネチャを変更（または拡張）し、Undoされたアイテム情報を返すように実装する。
- [x] Task: Conductor - User Manual Verification 'Phase 1: Domain Model' (Protocol in workflow.md) [checkpoint: a4c0acb]

## Phase 2: Application Layer Integration (振る舞いの接続)
**目標:** ドメインの変更をアプリケーション層に波及させ、実際の機能として動作させる。

- [x] Task: `MainViewModel` と `DockPictureViewModel` の連携テストを作成する。 [a892823]
    - [x] `DockPictureViewModel` の特定（ID）と更新が正しく行われるかのテスト。
- [x] Task: `DockPictureViewModel` に一意な ID を実装する。 [8fe628a]
- [x] Task: `MainViewModel` の `Pull` 処理を修正する。 [3fd4cde]
    - [x] 差分抽出ロジックの実装（テスト必須）。
    - [x] `DrawingSession` への Push 処理追加。
- [x] Task: Undo/Redo 時のイベントハンドリング実装。 [16478c5]
    - [x] `DrawingSession` から返された `DockActiveHistoryItem` を元に、対象ドック画像を復元する処理。
- [x] Task: Conductor - User Manual Verification 'Phase 2: Integration' (Protocol in workflow.md) [checkpoint: 19bfac5]

## Phase 3: Bug Fix and Finalization
**目標:** 手動検証で見つかったドックエリアのリドゥ不可問題を修正し、トラックを完了させる。

- [~] Task: ドックエリアに対するリドゥ（Redo）機能の不具合修正。
    - [ ] `MainViewModel.OnRedone` または `DrawingSession.Redo` のロジック確認と修正。
    - [ ] 統合テストに Redo の検証ケースを追加。
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Bug Fix' (Protocol in workflow.md)
