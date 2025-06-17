# DDD によるシステム分析

## 1. ドメインの特定と境界の定義

Eedeシステムのコアドメインは**ドット絵の作成と編集**です。これは、ピクセル単位の厳密な操作、限られた色数での表現、そして拡大表示による詳細な編集が不可欠な領域です。

-   **コアドメイン**:
    -   **描画ドメイン**: ユーザー入力に基づくピクセル単位の画像生成・変更 (`DrawStyles`, `Drawings`)。特有の描画ツール（例: 鉛筆、直線、塗りつぶし）の振る舞いを定義します。
    -   **画像操作ドメイン**: `Picture` エンティティへのピクセルデータレベルでの変換・編集操作 (`Picture Actions`)。整合性を保ちつつ、全体的な変形や調整を行います。
    -   **カラー管理ドメイン**: 色の表現、選択、そして厳選されたパレットの管理 (`Colors`, `Palettes`)。限られた色数で最大限の表現を引き出すための基盤となります。

-   **サブドメイン**:
    1.  **画像変換・処理サブドメイン**: 特有の画像合成 (`ImageBlenders`)、色調・アルファチャンネル変換 (`ImageTransfers`)。品質を保つための拡大縮小アルゴリズムや減色処理などが含まれます。
    2.  **UI 操作サブドメイン**: 画面上のピクセル座標・領域 (`Positions`)、拡大表示率 (`Scales`)、サイズ (`Sizes`) の管理。ユーザーがピクセル単位で正確に操作できるよう支援します。
    3.  **システム管理サブドメイン**: 編集履歴を保証するアンドゥ/リドゥ履歴 (`UndoSystem`)、ファイルの読み書き (`FileManagement`)。

## 2. ユビキタス言語一覧

Eedeプロジェクトの主要なユビキタス言語とその定義です。ドット絵の文脈に特化しています。

### 描画関連

-   **ピクセル (Pixel)**: ドット絵を構成する最小単位の点。色情報を持つ。
-   **グリッド (Grid)**: ピクセルを配置するための仮想的な格子。編集において、ピクセル単位の正確な配置を助ける。
-   **DrawStyle**: ピクセル単位の描画の振る舞いを定義する抽象概念（例: 1ピクセル鉛筆、直線、矩形、塗りつぶし）。`Eede.Domain.DrawStyles` 名前空間のクラスにマッピングされ、表現力を高める。
-   **DrawingBuffer**: 描画操作の中間結果を保持するメモリ上のバッファ。ピクセル操作を効率的に抽象化し、描画パフォーマンスを向上させる。
-   **PenStyle**: 描画時のペンの太さ（ピクセル単位）、形状、色などの設定をカプセル化した値オブジェクト。描画精度に直結する。
-   **RegionSelector**: ユーザーが画像上の特定の矩形ピクセル領域を選択するためのツール。選択範囲はピクセル単位で定義される。

### 画像操作関連

-   **Picture**: 編集対象となるドット絵を表現するエンティティ。厳密なピクセルデータとサイズを保持し、不変性を保証する。変更操作は常に新しい `Picture` インスタンスを返すことで、アンドゥ/リドゥシステムとの整合性を保ち、ピクセルデータの破壊を防ぐ。
-   **PictureArea**: `Picture` 内の特定の矩形ピクセル領域を指す値オブジェクト。ピクセル単位での選択、コピー、貼り付けなどに利用される。
-   **PictureSize**: `Picture` の幅と高さをピクセル単位で表す値オブジェクト。
-   **PictureAction**: `Picture` エンティティに適用される単一の操作（例: `FlipHorizontalAction`, `RotateRightAction`）。元の `Picture` を変更せず、新しい `Picture` を生成するドメインサービス。ピクセルデータの整合性を保ちながら画像全体を操作する。

### カラー関連

-   **Palette**: ドット絵に不可欠な、限られた色数の `ArgbColor` を管理する集約。ユーザーが選択可能な色の集合を定義し、表現の制約と美学を維持する。
-   **パレットインデックス (Palette Index)**: パレット内の特定の色を指し示す数値。ピクセルデータが直接色情報を持つ代わりにパレットインデックスを持つことで、ファイルサイズ削減や色変更の容易さを実現する場合がある。
-   **ArgbColor**: アルファ、赤、緑、青の各成分を持つ色情報を表す値オブジェクト。色表現の基本となる。
-   **HsvColor**: 色相、彩度、明度で色情報を表す値オブジェクト。`ArgbColor` と相互変換可能で、色の選択や調整に利用される。

### 画像処理関連

-   **ImageBlender**: 2つのドット絵を特定のアルゴリズムで合成するドメインサービス。ピクセル単位の合成処理を正確に行う。
-   **ImageTransfer**: ピクセルデータを変換するドメインサービス。**減色処理**、**パレット変換**、**特定の拡大アルゴリズム（例: Nearest Neighbor）**、色調補正など、品質を保つための処理を担う。
-   **AlphaTone**: アルファチャンネルの濃度を調整する操作。半透明表現に利用される。

## 3. 集約とエンティティの関係性

`Picture` はドット絵編集における最も重要なエンティティで、その**不変性**によりアンドゥ/リドゥの容易さやピクセルデータの整合性を強力に保証します。

### Picture 集約

-   **ルート**: `Picture` (Eede.Domain.Pictures.Picture)
    -   **値オブジェクト**:
        -   `PictureArea` (Eede.Domain.Pictures.PictureArea): `Picture` 内の選択領域や編集対象領域をピクセル単位で表現。
        -   `PictureSize` (Eede.Domain.Pictures.PictureSize): `Picture` の幅と高さをピクセル単位で表す。
        -   `Position` (Eede.Domain.Positions.Position): ピクセル座標を表す。描画や選択において、正確なピクセル位置を特定するために不可欠。
        -   `MagnifiedSize` (Eede.Domain.Sizes.MagnifiedSize): 拡大表示されたドット絵のサイズを表す。ピクセルが拡大されても、元のピクセルグリッドとの対応を維持する。

### DrawStyle 集約

ドット絵の描画の振る舞いをカプセル化し、ピクセル単位の描画操作のコンテキストを提供する。

-   **ルート**: `DrawStyle` （Eede.Domain.DrawStyles.IDrawStyle インターフェースとその実装）
    -   **エンティティ**:
        -   `Drawer` (Eede.Domain.Drawings.Drawer): 実際のピクセル描画ロジックを実行するエンティティ。
    -   **値オブジェクト**:
        -   `PenStyle` (Eede.Domain.DrawStyles.PenStyle): ピクセル単位のペンの設定を保持。
        -   `ArgbColor` (Eede.Domain.Colors.ArgbColor): 描画色。パレットから選択されることが多い。
        -   `Position` (Eede.Domain.Positions.Position): 描画開始/終了ピクセル座標。

### Palette 集約

ドット絵の色管理の中核であり、アプリケーション全体で共有される限られた色数の集合を管理する。表現において、パレットは単なる色の集合ではなく、作品の雰囲気を決定づける重要な要素となる。

-   **ルート**: `Palette` (Eede.Domain.Colors.Palette)
    -   **値オブジェクト**:
        -   `ArgbColor` (Eede.Domain.Colors.ArgbColor): パレット内の色。
        -   `HsvColor` (Eede.Domain.Colors.HsvColor): パレット内の色をHSV形式で表現。

## 4. ドメインサービスの候補

ドメインサービスは、複数のドメインオブジェクトにまたがる操作や、ドメインの重要なプロセスをカプセル化する。特にドット絵特有の処理を強調する。

1.  **画像処理サービス**
    -   `IImageBlender` (Eede.Domain.ImageBlenders.IImageBlender): 2つの `Picture` をピクセル単位で合成。透過処理やレイヤー合成など、レイヤー編集に利用。
    -   `IImageTransfer` (Eede.Domain.ImageTransfers.IImageTransfer): `Picture` のピクセルデータを変換。**減色処理**、**パレット変換**、**特定の拡大アルゴリズム（例: Nearest Neighbor）**、色調補正など、品質を保つための処理を担う。
    -   `IPaletteFileReader` (Eede.Application.Services.IPaletteFileReader): 外部ファイルからパレットデータを読み込む。コミュニティで共有されるパレット形式に対応。

2.  **描画サービス**
    -   `IDrawStyle` (Eede.Domain.DrawStyles.IDrawStyle): ユーザー入力と `Picture` を受け取り、ピクセル単位の描画ロジックを実行して新しい `Picture` を生成。アンチエイリアシングの有無など、描画品質に影響する設定も含む。
    -   `DrawingBuffer` (Eede.Domain.Drawings.DrawingBuffer): 描画操作中に一時的なピクセルデータを管理。高速なピクセル操作を可能にする。

3.  **ファイル操作サービス**
    -   `ActFileReader` (Eede.Infrastructure.Files.ActFileReader): 特定のパレットファイル形式（ACT）を読み込み、ドメインオブジェクトに変換。
    -   `AlphaActFileReader` (Eede.Infrastructure.Files.AlphaActFileReader): アルファチャンネル付きACTファイルを読み込む。
    -   `PictureFileWriter` （仮称）: ドット絵をPNG, GIFなどの画像ファイル形式で保存するサービス。特にGIFアニメーションなど、特有の保存形式に対応。

4.  **アンドゥ/リドゥサービス**
    -   `UndoSystem` (Eede.Domain.Systems.UndoSystem): `Picture` の変更履歴を管理し、アンドゥ/リドゥ操作を提供。ピクセル単位の変更を正確に巻き戻すために不可欠。

## 5. コンテキストマップ

Eedeシステムの主要なバウンデッドコンテキストとその関係性。ドット絵の特性を考慮した役割分担。

```
[UI/Presentation Context]
   ^       ^
   |       | (Uses)
   |       |
[Application Context]
   ^       ^       ^
   |       |       | (Uses)
   |       |       |
[Drawing Context] --> [Image Processing Context]
   - DrawStyles         - ImageBlenders
   - Drawings           - ImageTransfers
   ^
   | (Uses)
   |
[Color Management Context]
   ^
   | (Uses)
   |
[File Management Context]
```

-   **UI/Presentation Context (Eede.Presentation)**: ユーザーインターフェースと操作処理。ピクセルグリッドの表示、拡大表示、パレット選択UIなど、編集に特化したUI要素を提供する。
-   **Application Context (Eede.Application)**: ユースケースを調整し、ドメイン層オブジェクトをオーケストレーション。新規作成、保存、編集フロー全体を管理する。
-   **Drawing Context (Eede.Domain.DrawStyles, Eede.Domain.Drawings)**: ピクセル単位の描画ロジックと描画スタイルの管理。特有の描画ツール（鉛筆、直線、塗りつぶしなど）の振る舞いを定義する。
-   **Image Processing Context (Eede.Domain.ImageBlenders, Eede.Domain.ImageTransfers)**: 画像合成や変換ロジック。特に、**Nearest Neighborなどの拡大アルゴリズム**、**減色処理**、**パレット変換**など、品質と表現を維持・向上させるための処理を担う。
-   **Color Management Context (Eede.Domain.Colors)**: 色の表現とパレットの管理。限られた色数で効果的な表現を行うためのパレットの作成、編集、適用を扱う。
-   **File Management Context (Eede.Infrastructure.Files)**: ファイルの読み書きと永続化。PNG, GIFなどの画像形式や、ACTなどのパレット形式に対応する。

## 6. バウンデッド・コンテキスト間の関係性

### 上流・下流関係

1.  **UI/Presentation Context (下流)**: Application Context に依存。UI操作をApplication層のコマンドにマッピングする。
2.  **Application Context (上流)**: Drawing, Image Processing, Color Management, File Management 各ドメインコンテキストに依存。編集ワークフローを調整する。
3.  **Drawing Context (上流)**: Image Processing Context および Color Management Context に依存。描画結果をImage Processing Contextで処理し、Color Management Contextから色情報を取得する。
4.  **Image Processing Context (下流)**: Color Management Context に依存。画像処理においてパレット情報や色変換ロジックを利用する。減色やパレット変換に不可欠。
5.  **Color Management Context (共有カーネル)**: `Eede.Domain.Colors` に定義される色関連概念は、他の多くのコンテキストから参照される共有カーネル。パレットはシステム全体で共有される。
6.  **File Management Context (汎用サブドメイン)**: 他のコンテキストから独立した汎用機能を提供。Application Context を介して利用され、入出力を行う。

### 連携パターン

-   **UI/Presentation ↔ Application**: **アンチコラプション層 (ACL)**
    -   Presentation層はApplication層が提供するDTOやコマンド/クエリを介して通信。UIイベントをドメインの操作に変換する。
-   **Application ↔ ドメインコンテキスト群**: **調整役 (Orchestration)**
    -   Application層は複数のドメインコンテキストにまたがるユースケースを調整。複雑な編集操作（例: 選択範囲のコピー＆ペースト）を複数のドメインサービスを組み合わせて実現する。
-   **Drawing ↔ Image Processing**: **パートナーシップ**
    -   密接な協力関係。描画されたピクセルデータはすぐに画像処理コンテキストで変換・合成されることが多いため、密なコミュニケーションが必要。特に描画結果の品質を保証する。
-   **Color Management ↔ 他コンテキスト**: **共有カーネル**
    -   `Eede.Domain.Colors` の共通色表現モデルを共有。パレットは、描画、画像処理、UIの各コンテキストで一貫して利用される。
-   **File Management ↔ Application**: **公開ホスト・サービス**
    -   File Management Context は明確なサービスインターフェースを公開し、Application Context がそれを介してファイル操作を行う。保存・読み込みを安全かつ効率的に行う。
