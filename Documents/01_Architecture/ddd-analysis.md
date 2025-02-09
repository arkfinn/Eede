# DDD によるシステム分析

## 1. ドメインの特定と境界の定義

### 主要ドメイン：デジタルイラスト/画像編集

- **コアドメイン**：
  - 描画機能（DrawStyles, Drawings）
  - 画像操作（Pictures, Picture Actions）
  - カラー管理（Colors）

### サブドメイン：

1. **画像変換・処理サブドメイン**

   - ImageBlenders：画像の合成処理
   - ImageTransfers：画像の転送・変換処理

2. **UI 操作サブドメイン**

   - Positions：座標管理
   - Scales：拡大縮小
   - Sizes：サイズ管理

3. **システム管理サブドメイン**
   - Systems：アンドゥ/リドゥ
   - Files：ファイル操作

## 2. ユビキタス言語一覧

### 描画関連

- **DrawStyle**：描画スタイル（線、曲線、塗りつぶしなど）
- **DrawingBuffer**：描画用バッファー
- **PenStyle**：ペンの設定
- **RegionSelector**：領域選択ツール

### 画像操作関連

- **Picture**：編集対象の画像
- **PictureArea**：画像の編集可能領域
- **PictureSize**：画像サイズ
- **PictureAction**：画像に対する操作（反転、回転など）

### カラー関連

- **Palette**：カラーパレット
- **ArgbColor**：ARGB 形式の色情報
- **HsvColor**：HSV 形式の色情報

### 画像処理関連

- **ImageBlender**：画像合成処理
- **ImageTransfer**：画像転送処理
- **AlphaTone**：アルファチャンネル処理

## 3. 集約とエンティティの関係性

### Picture 集約

- **ルート**：Picture
- **エンティティ**：
  - PictureArea
  - PictureSize
- **値オブジェクト**：
  - Position
  - MagnifiedSize

### DrawStyle 集約

- **ルート**：DrawStyle
- **エンティティ**：
  - Drawer
  - PenStyle
- **値オブジェクト**：
  - ArgbColor
  - Position

### Palette 集約

- **ルート**：Palette
- **値オブジェクト**：
  - ArgbColor
  - HsvColor

## 4. ドメインサービスの候補

1. **画像処理サービス**

   - `IImageBlender`: 画像合成処理
   - `IImageTransfer`: 画像転送処理
   - `IPaletteFileReader`: パレットファイル読み込み

2. **描画サービス**

   - `IDrawStyle`: 描画スタイル管理
   - `DrawingBuffer`: 描画バッファー操作

3. **ファイル操作サービス**
   - `ActFileReader`: ACT ファイル読み込み
   - `AlphaActFileReader`: アルファ付き ACT ファイル読み込み

## 5. コンテキストマップ

```
[UI Layer]
    |
    |--> [Drawing Context] --> [Image Processing Context]
    |        - DrawStyles         - ImageBlenders
    |        - Drawings          - ImageTransfers
    |
    |--> [Color Management Context]
    |        - Colors
    |        - Palettes
    |
    |--> [File Management Context]
             - Files
             - Readers/Writers
```

## 6. バウンデッド・コンテキスト間の関係性

### 上流・下流関係

1. **Drawing Context (上流)**

   - Image Processing Context に依存
   - Color Management Context に依存

2. **Image Processing Context (下流)**

   - Color Management Context に依存
   - File Management Context に依存

3. **Color Management Context (共有カーネル)**

   - 他のコンテキストから広く参照される
   - 色情報の標準化された表現を提供

4. **File Management Context (汎用サブドメイン)**
   - 他のコンテキストから独立
   - 標準的なインターフェースを提供

### 連携パターン

- Drawing ↔ Image Processing: **パートナーシップ**

  - 密接な協力関係
  - 相互の変更の影響を考慮

- Color Management ↔ 他コンテキスト: **共有カーネル**

  - 共通の色表現モデル
  - 標準化されたインターフェース

- File Management ↔ 他コンテキスト: **公開ホスト・サービス**
  - 明確なサービスインターフェース
  - 疎結合な関係
