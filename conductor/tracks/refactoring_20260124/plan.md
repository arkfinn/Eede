# Implementation Plan: DDD-Driven Codebase Refactoring

## Phase 1: Assessment and Safety Lockdown [checkpoint: 7b09cab]
リファクタリングの安全性を確保するための準備と、ターゲットの精密な分析を行います。

- [x] Task: Gitステータスの確認（クリーンな状態であることの保証） 885218b
- [x] Task: 巨大クラスおよび設計上の問題があるクラスの特定 c506fd1
- [x] Task: **【安全性】ターゲットに対する仕様化テスト（Golden Master）の作成** 13ecae6
- [x] Task: Conductor - User Manual Verification 'Phase 1: Assessment and Safety Lockdown' (Protocol in workflow.md) 7b09cab

## Phase 2: Domain Model Reconstruction (DrawingSession)
ViewModelからドメイン知識を奪い、豊かなドメインモデルを構築します。

- [~] Task: **`DrawingSession` 集約の導入**
    - `PictureBuffer` と `UndoSystem` を統合し、描画操作と履歴管理の整合性を保証するエンティティを作成
- [ ] Task: **`DrawingTool` の値オブジェクト化**
    - `IDrawStyle` やペン設定を、ドメインルール（線の太さ、ブレンディング等）を持つドメインモデルへ再構成
- [ ] Task: **`IPictureRepository` の定義**
    - ファイルI/Oを技術詳細（Infrastructure）として分離し、ドメイン層に抽象を配置
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Domain Model Reconstruction' (Protocol in workflow.md)

## Phase 3: Decoupling Presentation Layer
ViewModelを「UI状態の保持」と「ドメインモデルへの委譲」のみに特化させます。

- [ ] Task: **`DrawingSessionViewModel` への分割**
    - `MainViewModel` から描画セッション固有の状態を分離
- [ ] Task: **ビットマップ生成ロジックの Infrastructure 層への移動**
    - `Picture` から `Bitmap` への変換を ViewModel から排除し、Infrastructure層の Adapter に隠蔽
- [ ] Task: **描画イベントからドメインコマンドへの変換層の実装**
    - `DrawableCanvasViewModel` の低レベルな座標計算ロジックを整理
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Decoupling Presentation Layer' (Protocol in workflow.md)

## Phase 4: Final Verification and Cleanup
最終的な品質確認と、得られた知見のドキュメント化を行います。

- [ ] Task: 全テストスイートの実行とカバレッジ（目標80%）の確認
- [ ] Task: 不要になった古い実装の完全削除
- [ ] Task: プロジェクト全体のビルドと手動動作確認
- [ ] Task: /learn プロトコルの実行と、設計原則（ADR等）の更新
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Verification and Consolidation' (Protocol in workflow.md)