# Implementation Plan: Canvas Service Elimination

手続き的な `CanvasService` を排除し、座標計算の知識を Value Object に、描画手順を UseCase に再配置することで、ドメイン駆動設計を深化させます。

## Phase 1: Domain Model Refinement
座標変換を型安全に行うための Value Object を導入し、ドメインロジックを強化します。

- [x] Task: **座標 Value Object の実装**
    - [x] `DisplayCoordinate` (VO) の作成: UI上の位置を保持
    - [x] `CanvasCoordinate` (VO) の作成: ドメイン（ピクセル）上の位置を保持
    - [x] `DisplayCoordinate.ToCanvas(Magnification)` の実装と単体テスト
- [x] Task: **`Magnification` の強化**
    - [x] 倍率に応じたスケーリング計算を `Magnification` 内に集約
- [x] Task: **ドメインインターフェースの更新**
    - [x] `DrawingSession`, `IDrawStyle`, `DrawingTool` 等の引数を `CanvasCoordinate` に変更
- [x] Task: Conductor - User Manual Verification 'Phase 1: Domain Model Refinement' (Protocol in workflow.md)

## Phase 2: Application UseCase Implementation
ViewModel からドメインロジックの手順を抽出し、UseCase として定義します。

- [x] Task: **描画ユースケースの作成**
    - [x] `DrawActionUseCase` の実装: 描画開始、移動、終了の調整
    - [x] ユースケースの単体テスト（ドメインモデルとの連携を確認）
- [x] Task: **Undo/Redo 手順の整理**
    - [x] ユースケース内での描画履歴の確定（Push）タイミングの管理
- [x] Task: Conductor - User Manual Verification 'Phase 2: Application UseCase Implementation' (Protocol in workflow.md)

## Phase 3: Presentation Layer Refactoring
ViewModel から `CanvasService` への依存を排除し、新しい UseCase と座標 VO を使用するように書き換えます。

- [x] Task: **`DrawableCanvasViewModel` のリファクタリング**
    - [x] `ICanvasService` の注入と呼び出しを削除
    - [x] マウスイベントから `DisplayCoordinate` を生成し、UseCase へ渡すように変更
- [x] Task: **`CanvasService` の完全排除**
    - [x] 不要になった `CanvasService.cs`, `ICanvasService.cs` および関連テストの削除
- [x] Task: **統合テストと動作確認**
    - [x] 既存の ViewModel テストを新しい設計に合わせて修正・実行
- [x] Task: Conductor - User Manual Verification 'Phase 3: Presentation Layer Refactoring' (Protocol in workflow.md)

## Phase 4: Final Consolidation
リファクタリングの結果を検証し、ドキュメントを更新します。

- [x] Task: **全テストスイートの実行**
    - [x] カバレッジの維持と全 144+ テストの成功を確認
- [x] Task: **アーキテクチャドキュメントの更新**
    - [x] 新しい座標 VO の役割について設計指針に追記
- [x] Task: Conductor - User Manual Verification 'Phase 4: Final Consolidation' (Protocol in workflow.md)