# Specification: `canvas-service-elimination`

## Overview
手続き的な `CanvasService` を排除し、座標計算の知識を適切な Value Object（ドメインモデル）に集約します。また、ViewModel が持っていた描画操作の「手順」を Application 層の UseCase に抽出し、真のドメイン駆動設計（DDD）とオニオンアーキテクチャへの準拠を実現します。

## 変更の背景
前回のフェーズで導入した `CanvasService` は、ドメイン知識を「手続き」として吸い出してしまい、ドメインモデルがデータ保持のみに陥る（ドメインモデル貧血症）原因となっていました。これを「型」と「ドメインオブジェクト」主体の設計に再構築します。

## 機能要件
### 1. 座標用 Value Object の導入
- **`DisplayCoordinate`**: 画面上の位置（表示座標）を表す。UI（ViewModel）からの入力を最初に受ける。
- **`CanvasCoordinate`**: キャンバス上の位置（ピクセル座標）を表す。ドメインモデルが扱う。
- **変換ロジック**: `DisplayCoordinate.ToCanvas(Magnification m)` により、型を変換しながら座標計算を行う。

### 2. Magnification (倍率) の強化
- `Magnification` 自身がスケーリング（拡大・縮小）の計算ルールを持つ。

### 3. Application UseCase の実装
- **`DrawActionUseCase`**: 描画の開始、移動、終了などの手順を管理する。
- ViewModel からの入力を `DisplayCoordinate` 等のドメインオブジェクトとして直接受け取り、ドメインモデル（`DrawingSession` 等）へ委譲する。

### 4. ドメインモデルの改善
- `DrawingSession` や `IDrawStyle` などのドメイン層のインターフェース/クラスが、生の `int` ではなく `CanvasCoordinate` を要求するように変更する。

## 非機能要件
- **型安全性**: 表示座標とキャンバス座標を型レベルで区別し、誤った座標の使用をコンパイルエラーで防ぐ。
- **テスト可能性**: 座標変換や描画手順のテストにおいて、UI やプラットフォームサービスへの依存を不要にする。

## 受入基準 (Acceptance Criteria)
- [ ] `CanvasService` クラスおよびインターフェースが完全に削除されている。
- [ ] 表示座標からキャンバス座標への変換ロジックが `DisplayCoordinate` に実装され、単体テストがパスする。
- [ ] 描画操作（ペン、直線等）が、新しい UseCase を通じて以前と同様に（またはより正確に）動作する。
- [ ] ViewModel から低レベルな座標計算ロジックが完全に排除されている。

## 対象外 (Out of Scope)
- 描画エンジン自体のアルゴリズム変更。
- 新しい描画ツールの追加。
