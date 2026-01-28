# Implementation Plan: DrawableCanvasViewModel Refactoring

## Phase 1: Assess & Secure (Safety First)
- [x] Task: 既存の `DrawableCanvasViewModel` の依存関係と入出力を分析する [8730994]
- [x] Task: Avalonia Headless Testing 環境を用いて、仕様化テスト (Characterization Tests) を作成する [f2a3e9c]
- [x] Task: 全テストがパスすることを確認し、リファクタリング前の「ゴールデンマスター」とする [f2a3e9c]
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Assess & Secure' (Protocol in workflow.md)

## Phase 2: Extract Interaction Logic (Divide & Conquer)
- [x] Task: `IInteractionCoordinator` インターフェースと実装クラスを Presentation 層に作成する [031fba0]
- [x] Task: 座標変換ロジック（Display <-> Canvas）を Coordinator または専用の VO に移動する [031fba0]
- [x] Task: `DrawableCanvasViewModel` 内の `ExecuteDrawBegin/Drawing/End` ロジックを Coordinator へ段階的に移譲する [b6aecf7]
    - [ ] 各ステップごとに既存の仕様化テストを実行し、挙動が変わっていないことを確認する
- [x] Task: `CanvasInteractionSession` の直接操作を Coordinator 内に閉じ込める [b6aecf7]
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Extract Interaction Logic' (Protocol in workflow.md)

## Phase 3: Property & State Refactoring
- [x] Task: ViewModel の Reactive プロパティの更新を、Coordinator から返される「状態オブジェクト」に基づく更新に整理する [48bb992]
- [x] Task: カーソル形状の決定ロジックを Coordinator または Domain 層へ移動する [48bb992]
- [x] Task: DI コンテナに Coordinator を登録し、ViewModel への注入を有効化する [48bb992]
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Property & State Refactoring' (Protocol in workflow.md)

## Phase 4: Final Cleanup & Verification
- [x] Task: ViewModel 内の不要な using やプライベート変数を削除する [48bb992]
- [x] Task: すべての単体テスト（新規および既存）を実行し、100% パスを確認する [48bb992]
- [x] Task: 手動でアプリケーションを動かし、操作感に違和感がないか最終確認する [72c8dc1]
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Final Cleanup & Verification' (Protocol in workflow.md)
