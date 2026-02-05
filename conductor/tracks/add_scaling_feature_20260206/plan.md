# 拡大縮小機能の実装計画 (TDD Edition)

## Phase 1: ドメイン計算ロジック (Output-based TDD) [checkpoint: 9b9ec21]
UIや外部依存を切り離し、純粋な計算ロジック（リサイズ計算とビットマップ変換）を構築します。

- [x] Task: `ResizeContext` 値オブジェクトのTDD (ced9543)
    - [x] **Red**: 比率固定時の連動計算や、px/%の相互変換、アンカーに基づくオフセット計算のテストを記述
    - [x] **Green**: `ResizeContext` を実装し、テストをパスさせる
    - [x] **Refactor**: 計算式の整理、VOとしての不変性の確保
- [x] Task: ニアレストネイバー変換ロジックのTDD (61d43f2)
    - [x] **Red**: 小さなビットマップ（2x2等）を200%, 50%にした際、特定のピクセルが期待通り配置されるかのテストを記述
    - [x] **Green**: `ImageProcessingHelper` 等に変換ロジックを実装し、テストをパスさせる
    - [x] **Refactor**: パフォーマンス改善とコードの明瞭化
- [x] Task: Conductor - User Manual Verification 'Phase 1' (Protocol in workflow.md) (9b9ec21)

## Phase 2: ユースケースと状態遷移 (State-based TDD)
`DrawingSession`（集約）の状態変化と、ユースケースを通じた統合をテストします。

- [ ] Task: `ScalingImageUseCase` のTDD
    - [ ] **Red**: 「範囲選択なし時」にキャンバスがリサイズされ、画像が指定アンカーに配置される状態変化のテストを記述
    - [ ] **Red**: 「範囲選択あり時」に `DrawingSession` がプレビュー状態に遷移し、リサイズ後の画像が保持されるテストを記述
    - [ ] **Green**: ユースケースを実装し、テストをパスさせる
    - [ ] **Refactor**: ドメインイベントや状態遷移ロジックの Repatriation（適切な場所への再配置）
- [ ] Task: Conductor - User Manual Verification 'Phase 2' (Protocol in workflow.md)

## Phase 3: プレゼンテーション層 (Testable UI)
ViewModelの振る舞いをテストし、最後に View を構築します。

- [ ] Task: `ScalingDialogViewModel` のTDD
    - [ ] **Red**: 入力値の変化に伴うプロパティの連動（幅を変えたら高さが変わる等）と、バリデーションのテストを記述
    - [ ] **Green**: ReactiveUIを用いた ViewModel の実装
    - [ ] **Refactor**: Rxクエリ（WhenAnyValue）の整理
- [ ] Task: UI 実装と最終統合
    - [ ] ツールバーへのボタン追加とダイアログの表示実装（View）
    - [ ] 実機での手動検証と、既存の「プレビュー状態」との操作感の統一
- [ ] Task: Conductor - User Manual Verification 'Phase 3' (Protocol in workflow.md)
