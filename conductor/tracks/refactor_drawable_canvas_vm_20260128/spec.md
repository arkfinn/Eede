# Track Specification: DrawableCanvasViewModel Refactoring

## 1. Overview
`DrawableCanvasViewModel` は、現在 UI 状態の管理、マウス入力の処理、ドメインモデル（`CanvasInteractionSession`）の操作、および座標変換のすべてを担う「神クラス」となっています。
本トラックでは、レガシーコード改善手法を用いて、既存の挙動を壊すことなく、これらの責務を適切に分離し、ドメイン駆動設計（DDD）に基づいた疎結合な構造へ再構築します。

## 2. Goals & Objectives
- **挙動の固定:** 仕様化テスト（Characterization Tests）を導入し、リファクタリングによる退行（デグレード）を 100% 防ぐ。
- **責務の分離:**
  - ViewModel: View とのバインディング（プロパティ）に専念させる。
  - InteractionCoordinator (新規): マウスイベントのハンドリングと状態遷移の制御を担当する。
  - 座標変換: `DisplayCoordinate` と `CanvasCoordinate` の変換ロジックを VO または専用サービスへ完全に移行する。
- **テスト可能性の向上:** Avalonia に依存しない形で描画ロジックをテスト可能にする。

## 3. Functional Requirements
リファクタリング後も、以下の機能が現状通り動作すること：
- 自由曲線、図形描画（直線、矩形、円）
- Shift キーによる描画制約（正方形、水平・垂直線など）
- 範囲選択、移動、および右クリックによるキャンセル
- コピー、切り取り、貼り付け操作
- プレビュー表示（移動中や描画中のピクセル表示）
- グリッド設定およびアニメーション編集モードの切り替え

## 4. Technical Strategy (Master's Approach)
1.  **Golden Master / Characterization Tests:**
    `Eede.Presentation.Tests` 内で、ViewModel に対して一連のマウスイベントを注入し、最終的な `MyBitmap` のピクセルデータや `SelectingArea` の状態をスナップショットとして保存・比較するテストを構築する。
2.  **InteractionCoordinator の抽出:**
    ViewModel 内の `ExecuteDrawBeginAction` 等のロジックを `InteractionCoordinator` へ移動。ViewModel はこのコーディネーターにイベントを通知し、戻り値として得られる「表示用状態」を自身のプロパティに反映するだけにする。
3.  **Dependency Injection:**
    抽出したコーディネーターを DI コンテナに登録し、ViewModel に注入する。

## 5. Acceptance Criteria
- [ ] すべての仕様化テストが、リファクタリング前後でパスすること。
- [ ] `DrawableCanvasViewModel` のコード行数が大幅に削減されていること。
- [ ] `CanvasInteractionSession` の操作ロジックが ViewModel から排除されていること。
- [ ] 手動確認において、描画・選択・移動の操作感に変化がないこと。
