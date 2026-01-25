# Implementation Plan - DockPictureViewModel Refactoring (DDD & Legacy Safe)

## Phase 0: 現状挙動の保護 (Safety Net)
- [ ] Task: `DockPictureViewModel` の既存の振る舞いを固定する「仕様化テスト」を作成する
    - [ ] Sub-task: `Save`/`Load` 時の副作用（ファイル書き出し等）を特定し、モック可能な継ぎ目を見つける
    - [ ] Sub-task: ゴールデンマスター（期待される出力の固定）を作成し、リファクタリング中の安全を確保する

## Phase 1: ドメイン層の確立 (Domain Definition)
- [ ] Task: `Eede.Domain` 層に永続化インターフェース `IPictureRepository` を定義する
    - [ ] Note: アプリケーション層ではなくドメイン層に置くことで、DIP（依存性逆転）を完全にする
- [ ] Task: `Eede.Infrastructure` (または Presentation) の `BitmapFile` を `IPictureRepository` の実装として適合させる
    - [ ] Note: 必要であれば Adapter パターンを使用し、`Bitmap` (UI依存) と `Picture` (ドメイン) の変換をこの層で吸収する
- [ ] Task: Conductor - User Manual Verification 'Phase 1: ドメイン層の確立' (Protocol in workflow.md)

## Phase 2: ロジックの物理的移動 (Move & Delegate)
- [ ] Task: `Eede.Application` に `SavePictureUseCase` を作成し、ViewModel のロジックを**そのまま**移動する
    - [ ] Sub-task: 引数には UI 固有の型（Bitmap等）を使わず、Domain Entity (`Picture`) や DTO を使用するように署名を変更する
    - [ ] Sub-task: 内部ロジックは汚いままで良いので、まずは場所を移動させる
- [ ] Task: `Eede.Application` に `LoadPictureUseCase` を作成し、ロジックを移動する
- [ ] Task: ViewModel が新しいユースケースに処理を委譲するように変更する
- [ ] Task: Conductor - User Manual Verification 'Phase 2: ロジックの物理的移動' (Protocol in workflow.md)

## Phase 3: ドメイン知識の蒸留 (Distill Domain Logic)
- [ ] Task: 移動したユースケース内のロジックを分析し、**ドメインルール**を抽出する
    - [ ] Sub-task: 画像の整合性チェック、加工ルールなどの「ビジネスロジック」を `Picture` エンティティや ValueObject に移動する（ドメイン貧血症の解消）
    - [ ] Sub-task: ユースケースは「リポジトリの呼び出し」や「ドメインオブジェクトの操作」という進行役に徹するようにする
- [ ] Task: Conductor - User Manual Verification 'Phase 3: ドメイン知識の蒸留' (Protocol in workflow.md)

## Phase 4: コードのクリーンアップ (5-Line Rule)
- [ ] Task: 残ったユースケースと ViewModel のコードに対して「5行ルール」を適用する
    - [ ] Sub-task: `if` 文の排除（ポリモーフィズム化）、メソッド抽出を行い、可読性を最大化する
- [ ] Task: 全テストを実行し、機能と設計の両面で回帰がないことを確認する
- [ ] Task: Conductor - User Manual Verification 'Phase 4: コードのクリーンアップ' (Protocol in workflow.md)