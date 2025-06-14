# DDDシステム分析改善計画：画像編集ドメインの再定義

## 計画の目的

この計画は、`Documents/01_Architecture/ddd-analysis-review.md`で指摘された課題に基づき、Eedeシステムのドメイン駆動設計（DDD）分析を改善することを目的とします。特に、「画像操作ドメイン」を「画像編集ドメイン」として再定義し、レイヤー管理や選択範囲操作など、より具体的な責任範囲を持たせることに焦点を当てます。最終的なゴールは、実装を修正し、`Documents/01_Architecture/ddd-analysis.md`を実装に沿って更新し、再レビューに合格することです。

## フェーズ1: ドメイン定義の再構築と実装

### 目標
コアドメインの再定義と、それに伴う主要なドメインオブジェクト、サービス、境界のコードレベルでの調整を行います。

### 具体的な実装計画 (Code Mode 担当)

以下のファイルに対して具体的な修正を行います。

*   **1. ドメインの再定義とレイヤー/選択範囲の導入**:
    *   **`Eede.Domain/Pictures/Picture.cs`**:
        *   `Picture`クラスの責任範囲を「画像編集ドメイン」の概念に合わせて調整。
            1.  **レイヤー管理の導入**:
                *   現在、`Picture`クラスが直接画像データ（ピクセル情報）を保持している可能性があります。これを変更し、`Picture`クラスは複数の`Layer`エンティティを管理する「集約ルート」としての役割を担います。
                *   具体的には、`Picture`クラス内に`Layer`オブジェクトのコレクション（例: `List<Layer>`）を追加し、レイヤーの追加、削除、順序変更、可視性制御などの操作を`Picture`クラスを通じて行えるようにします。各`Layer`が自身の画像データを持つことになります。

            2.  **選択範囲操作との連携**:
                *   `Picture`クラスは、画像全体に対する操作だけでなく、特定の「選択範囲（`Selection`）」に対する操作も調整の対象となります。
                *   `Selection`エンティティまたは値オブジェクトが導入された際、`Picture`クラスは、その選択範囲情報を用いて、特定のレイヤー内の画像データに対して切り抜き、移動、コピーなどの編集操作を適用するメソッドを提供する可能性があります。これにより、`Picture`が集約ルートとして、レイヤーと選択範囲を統合的に管理する形になります。

            要するに、`Picture`は単一の画像データ保持者から、複数のレイヤーと選択範囲を統括する「画像編集のコンテキストにおける中心的なエンティティ」へと役割をシフトします。
        *   `Layer`エンティティを管理するためのコレクション（例: `List<Layer>`）を導入。
        *   選択範囲（`Selection`）を管理するためのプロパティまたは関連メソッドの追加を検討。
    *   **新規ファイル: `Eede.Domain/Layers/Layer.cs`**:
        *   `Layer`エンティティを定義。画像データ、可視性、ブレンドモードなどのプロパティを持つ。
    *   **新規ファイル: `Eede.Domain/Selections/Selection.cs`**:
        *   `Selection`値オブジェクトまたはエンティティを定義。選択範囲の幾何学的情報（矩形、パスなど）を持つ。
    *   **`Eede.Domain`内の名前空間の再編成**:
        *   必要に応じて、`Eede.Domain.Pictures`を`Eede.Domain.ImageEditing`などに変更し、関連クラスを移動。

*   **2. ユビキタス言語の精査と役割の明確化**:
    *   **`Eede.Domain/Drawings/DrawingBuffer.cs`**:
        *   クラスの役割を「描画中のキャンバス」として再検討し、必要に応じて名称変更（例: `CanvasBuffer`）や責任範囲の調整。
    *   **`Eede.Domain/DrawStyles/RegionSelector.cs`の移動**:
        *   `RegionSelector`をドメイン層からUI層またはアプリケーション層へ移動。
        *   移動先として、`Eede.Presentation/Views/General/Region.axaml.cs`または`Eede.Application`内の適切な場所を検討。
        *   移動後、`Eede.Domain/DrawStyles/RegionSelector.cs`は削除。

*   **3. 集約とエンティティの関係性の調整**:
    *   **`Eede.Domain/DrawStyles/IDrawStyle.cs`および関連クラス (`PenStyle.cs`, `Drawer.cs`など)**:
        *   `IDrawStyle`がドメインサービスとして機能するよう、そのインターフェースと実装を調整。集約ルートとしての役割を削除。
    *   **`Eede.Domain/Sizes/MagnifiedSize.cs`の移動**:
        *   `MagnifiedSize`をドメイン層からUI層へ移動。
        *   移動先として、`Eede.Presentation/ViewModels/DataDisplay/DockPictureViewModel.cs`や`Eede.Presentation/Views/Navigation/MagnificationMenu.axaml.cs`など、UI表示に関連するクラスを検討。
        *   移動後、`Eede.Domain/Sizes/MagnifiedSize.cs`は削除。

*   **4. ドメインサービスの調整**:
    *   **`Eede.Domain/Colors/IPaletteFileReader.cs`**:
        *   インターフェースがドメイン層に適切に配置されていることを確認。
    *   **`Eede.Infrastructure/Colors/ActFileReader.cs`など**:
        *   `IPaletteFileReader`の実装がインフラストラクチャ層に適切に配置されていることを確認。

### 担当モード
Code Mode (実装修正)

## フェーズ2: ドキュメントの更新

### 目標
フェーズ1で実施された実装変更を`Documents/01_Architecture/ddd-analysis.md`に反映します。

### 詳細
`ddd-analysis.md`の以下のセクションを更新します。
*   **1. ドメインの特定と境界の定義**: 「画像編集ドメイン」の記述、レイヤー管理と選択範囲操作の追加、UI操作サブドメインの位置づけ変更。
*   **2. ユビキタス言語一覧**: 「描画中のキャンバス」「選択範囲」への用語変更、`DrawingBuffer`、`RegionSelector`の位置づけ明確化。
*   **3. 集約とエンティティの関係性**: `DrawStyle`の変更、`MagnifiedSize`の移動、`Layer`集約の追加。
*   **4. ドメインサービスの候補**: `IDrawStyle`の役割明確化、ファイル操作サービスの配置。
*   **5. コンテキストマップ**: 「画像編集コンテキスト」「アニメーションコンテキスト」の追加、既存のコンテキストマップの更新。
*   **6. バウンデッド・コンテキスト間の関係性**: 「Drawing ↔ Image Processing」の関係を「顧客/供給者」に変更。

### 担当モード
Architect Mode (ドキュメント修正)

## フェーズ3: 再レビューと完了

### 目標
更新された実装とドキュメントがレビュー要件を満たしていることを確認します。

### 詳細
*   ユーザーによる`ddd-analysis.md`および関連するコード変更のレビュー。
*   必要に応じて追加の修正を実施。

### 担当モード
User (レビュー), Debug/Code Mode (修正)

## 成果物
*   更新された`Eede`プロジェクトのコードベース。
*   更新された`Documents/01_Architecture/ddd-analysis.md`。
*   本計画書（`ddd-redefinition-plan.md`）。
