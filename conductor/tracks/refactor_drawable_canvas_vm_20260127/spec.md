# Specification: DrawableCanvasViewModel の責務分離とドメイン知識の返却

## 1. Overview
`DrawableCanvasViewModel` は現在、UIの表示状態管理、マウスイベントによる座標計算、描画セッションの状態遷移、および編集コマンド（Copy/Cut/Paste）の実行といった多岐にわたる責務を抱えています。
本トラックでは、DDDの原則に基づき、これらの混在した責務を Domain 層（Value Object, Entity）および Application 層（UseCase）へ適切に再配置（Repatriation）し、ViewModel を薄く保つことで保守性とテスト容易性を向上させます。

## 2. Functional Requirements
### 2.1 空間の型による分離 (Spatial Type Separation)
- `DisplayCoordinate` (UI上の座標) と `CanvasCoordinate` (ピクセル座標) を Value Object として明確に定義する。
- Magnification に基づく座標変換ロジックをこれらの VO に閉じ込め、ViewModel 内での手動計算を排除する。

### 2.2 編集ユースケースの抽出
- `CopyCommand`, `CutCommand`, `PasteCommand` のロジックを以下の UseCase に分割・抽出する。
    - `CopySelectionUseCase`
    - `CutSelectionUseCase`
    - `PasteFromClipboardUseCase`
- 各 UseCase は 1クラス1メソッド (`Execute()`) の原則に従う。

### 2.3 相互作用ロジックのドメイン返却
- マウスイベント（Begin/Move/End）に伴う状態遷移ロジック（`_selectionState` の管理）を、Domain 層の新しい集約（例：`CanvasInteractionSession`）へ移動する。
- ViewModel はポインタ位置をドメイン空間に変換してセッションへ渡し、更新された画像データと新しい状態（カーソル形状など）を受け取るだけの役割とする。

### 2.4 描画結果の通知 (Undo/Redo 連携)
- リファクタリング後も、描画確定時の `Drew` イベントが正しく発火し、既存のアンドゥ・リドゥ機構が維持されることを保証する。

## 3. Non-Functional Requirements
- **アーキテクチャ遵守**: オニオンアーキテクチャの境界を守り、Domain 層が Presentation/Infrastructure 層に依存しないようにする。
- **テスタビリティ**: 抽出した UseCase および Domain モデルに対して、UIを介さずにロジックを検証できる単体テストを追加する。
- **結合度の低減**: ViewModel が `ISelectionState` などの具体的な状態クラスを直接操作するのをやめ、インターフェースまたは抽象化されたセッション経由で操作する。

## 4. Acceptance Criteria
- [ ] `DrawableCanvasViewModel` から直接的な座標計算ロジック（Magnification を使った乗除算）が消滅していること。
- [ ] コピー・切り取り・貼り付けが、抽出された UseCase を経由して従来通り動作すること。
- [ ] 自由線描画、範囲選択、選択範囲の移動といった一連の操作が、リファクタリング前と変わらず実行できること。
- [ ] 新規作成された UseCase および Domain ロジックのユニットテストが全てパスすること。

## 5. Out of Scope
- UIの外観デザインの変更。
- アンドゥ・リドゥエンジン（`History` 等）自体の内部実装の変更。
- 描画ツール（`IDrawStyle` 実装クラス）内部のアルゴリズム変更。
