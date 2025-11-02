# フォルダ構造の再分類提案

## 1. 目的

2025年11月1日に策定された新しいネームスペース規則とDDDの原則に基づき、`Eede.Domain`, `Eede.Application`, `Eede.Presentation` プロジェクトの既存のフォルダ構造を、よりビジネスの関心事を反映した、保守性の高い構造に再分類することを提案します。

## 2. 基本方針

- **境界づけられたコンテキストを最優先**: 技術的な役割（例: `Pictures`, `Systems`）による分類から、ビジネス上の関心事（例: `ImageEditing`）による分類へと移行します。
- **レイヤー間の構造的一貫性**: Domain, Application, Presentation層で、可能な限りコンテキストに基づいた一貫性のあるフォルダ構造を目指します。
- **依存関係の原則の遵守**: 依存関係は常に外側のレイヤー（UI, Infrastructure）から内側のレイヤー（Application, Domain）へ向かうようにします。内側のレイヤーが外側のレイヤーに依存することは許可しません。

---

## 3. 結論：推奨するフォルダ構造

以下の、**コンテキストベースの柔軟な混合スタイル**を推奨します。

### 3.1. `Eede.Domain` プロジェクト

| 既存フォルダ      | 提案する移動先                         | 理由                                                                                             |
| :---------------- | :------------------------------------- | :----------------------------------------------------------------------------------------------- |
| `Drawing/`        | `ImageEditing/`                        | 描画は画像編集の中核機能。                                                                       |
| `DrawStyles/`     | `ImageEditing/`                        | 同上。                                                                                           |
| `ImageBlenders/`  | `ImageEditing/`                        | 同上。                                                                                           |
| `ImageTransfers/` | `ImageEditing/`                        | 同上。                                                                                           |
| `Pictures/`       | `ImageEditing/`                        | `Picture`値オブジェクトは画像編集の核。                                                          |
| `Positions/`      | `ImageEditing/` および `SharedKernel/` | `Position`は共有するが、`HalfBoxArea`などは画像編集に特化。                                      |
| `Sizes/`          | `ImageEditing/` および `SharedKernel/` | `PictureSize`は共有するが、`MagnifiedSize`などは画像編集に特化。                                 |
| `Scales/`         | `ImageEditing/`                        | 拡大率は主に画像編集ビューで利用される。                                                         |
| `Systems/`        | `ImageEditing/`                        | `UndoSystem`は`ImageCanvas`の状態の一部。                                                        |
| `Colors/`         | `Palettes/` および `ImageEditing/`     | `Palette`関連は`Palettes`へ、`BackgroundColor`は`ImageEditing`へ。                               |
| `Files/`          | (廃止または`Infrastructure`へ)         | ドメイン層はファイル形式を知るべきでない。`FilePath`のような値オブジェクトのみ`SharedKernel`へ。 |

#### **提案後の `Eede.Domain` フォルダ構造**

```
Eede.Domain/
├── ImageEditing/
│   ├── ImageCanvas.cs          // Aggregate Root
│   ├── IImageCanvasRepository.cs
│   ├── Picture.cs              // Core Value Object
│   ├── BackgroundColor.cs      // Context-specific Value Object
│   ├── PictureArea.cs
│   ├── HalfBoxArea.cs
│   ├── UndoSystem.cs
│   ├── PositionHistory.cs
│   ├── Magnification.cs
│   ├── DrawStyles/             // Sub-domain: Drawing Tools
│   │   ├── IDrawStyle.cs
│   │   └── ...
│   ├── ImageBlenders/          // Sub-domain: Blending Logics
│   │   ├── IImageBlender.cs
│   │   └── ...
│   └── ImageTransfers/         // Sub-domain: Transformation Logics
│       ├── IImageTransfer.cs
│       └── ...
├── Palettes/
│   ├── Palette.cs              // Aggregate Root
│   ├── IPaletteRepository.cs
│   ├── ArgbColor.cs            // Value Object
│   └── HsvColor.cs             // Value Object
└── SharedKernel/
    ├── Position.cs
    └── PictureSize.cs
```

### 3.2. `Eede.Application` および `Eede.Presentation`

これらの上位レイヤーも、上記の`Eede.Domain`のコンテキスト構造（`ImageEditing`, `Palettes`）を反映したフォルダ構成に再編することを推奨します。

---

## 4. `Eede.Application` プロジェクトの再分類案

**現状の課題**: `UseCase`フォルダに様々な関心事のユースケースが混在し、一部は`Infrastructure`層の責務を含んでいます。

**提案**: Domain層のコンテキスト構造を反映させ、ユースケースをコンテキストごとに分類します。`Infrastructure`層の責務はリポジトリ経由で呼び出すように修正します。

| 既存クラス/フォルダ                              | 提案する移動先  | 理由                                                              |
| :----------------------------------------------- | :-------------- | :---------------------------------------------------------------- |
| `UseCase/Pictures/PictureEditingUseCase.cs`      | `ImageEditing/` | 画像編集のユースケース。                                          |
| `UseCase/Colors/LoadPaletteFileUseCase.cs`       | `Palettes/`     | パレット管理のユースケース。                                      |
| `UseCase/Colors/FindPaletteFileReaderUseCase.cs` | (廃止)          | この責務は`Infrastructure`層のリポジトリ実装がFactoryとして担う。 |
| `Drawings/`                                      | `ImageEditing/` | 描画関連のアプリケーションサービス。                              |
| `Pictures/`                                      | `ImageEditing/` | 画像関連のイベント引数など。                                      |
| `Colors/`                                        | `Palettes/`     | 色関連のイベント引数など。                                        |

#### **提案後の `Eede.Application` フォルダ構造**

```
Eede.Application/
├── ImageEditing/
│   ├── PictureEditingUseCase.cs
│   └── ...
└── Palettes/
    ├── LoadPaletteUseCase.cs
    ├── SavePaletteUseCase.cs
    └── ...
```

---

## 5. `Eede.Infrastructure` プロジェクトの再分類案

**現状の課題**: `Eede.Domain`プロジェクトに、ファイルI/Oなどインフラストラクチャ層が担うべき責務のコードが含まれています。

**提案**: ドメイン層で定義されたインターフェースに基づき、具体的な実装を`Infrastructure`層の各コンテキストフォルダに配置します。役割ベースの命名（例: `Repositories`）は避け、責務が明確な命名（例: `Persistence`）を採用します。

| `Eede.Domain`からの移動対象ファイル | 提案する移動先          | 理由                                                   |
| :---------------------------------- | :---------------------- | :----------------------------------------------------- |
| `Colors/ActFileReader.cs`           | `Palettes/Persistence/` | `IPaletteRepository`の具体的な実装（ファイル永続化）。 |
| `Colors/AlphaActFileReader.cs`      | `Palettes/Persistence/` | 同上。                                                 |
| `Colors/AlphaActFileWriter.cs`      | `Palettes/Persistence/` | 同上。                                                 |
| `Colors/IPaletteFileReader.cs`      | `Palettes/Persistence/` | 同上。                                                 |

#### **提案後の `Eede.Infrastructure` フォルダ構造（抜粋）**

```
Eede.Infrastructure/
└── Palettes/
    └── Persistence/
        ├── PaletteRepository.cs
        └── ActFileFormat/
            ├── IPaletteFileReader.cs
            ├── ActFileReader.cs
            ├── AlphaActFileReader.cs
            └── AlphaActFileWriter.cs
```

---

## 6. `Eede.Presentation` プロジェクトの再分類案

**現状の課題**: `ViewModels`や`Views`内が`Pages`, `DataDisplay`, `DataEntry`などで分類されており、UIの役割ベースではあるが、どの機能（ドメイン）に対応するかが分かりにくい。

**提案**: `ViewModels`と`Views`の内部を、ドメインコンテキストに対応した機能単位でグルーピングします。

#### **提案後の `Eede.Presentation` フォルダ構造（抜粋）**

```
Eede.Presentation/
├── ViewModels/
│   ├── MainViewModel.cs      // アプリケーション全体のシェル
│   ├── ImageEditing/
│   │   ├── DockPictureViewModel.cs
│   │   └── DrawableCanvasViewModel.cs
│   └── Palettes/
│       └── PaletteContainerViewModel.cs
└── Views/
    ├── Pages/                // MainWindowなど
    │   └── MainWindow.axaml
    ├── ImageEditing/
    │   ├── PictureFrame.axaml
    │   └── DrawableCanvas.axaml
    └── Palettes/
        └── PaletteContainer.axaml
```

## 7. 結論

この再分類により、プロジェクト全体の構造がビジネスの関心事と一致し、以下のメリットが期待できます。

- **可読性の向上**: 新しい開発者がコードベースを理解しやすくなる。
- **保守性の向上**: 変更の影響範囲がコンテキスト内に閉じやすくなる。
- **拡張性の向上**: 新しいビジネス機能（コンテキスト）を追加しやすくなる。

## 8. 設計に関する議論の記録

### 8.1. 依存関係の原則（Dependency Rule）

本リファクタリングは、クリーンアーキテクチャの依存関係の原則を厳格に適用します。
- **依存の方向:** すべての依存関係は、外側のレイヤー（`Presentation`, `Infrastructure`）から内側のレイヤー（`Application`, `Domain`）に向かわなければなりません。
- **インターフェースへの依存:** `Application`層は、`Infrastructure`層の具体的なクラス（例: `ActFileReader`）に直接依存してはいけません。代わりに、`Domain`層で定義されたインターフェース（例: `IPaletteRepository`）に依存します。
- **依存性の注入(DI):** `Infrastructure`層で実装された具象クラスは、アプリケーションのエントリーポイント（`Presentation.Desktop`など）でDIコンテナを介して`Application`層に注入されます。

この原則により、ビジネスロジック（`Application`）と技術的詳細（`Infrastructure`）が分離され、システムの保守性とテスト容易性が向上します。

### 8.2. `ImageEditing`コンテキスト内のフラット構造について

`ImageEditing`コンテキストの直下には、集約ルートである`ImageCanvas`に加え、`Picture`や`UndoSystem`など複数のファイルが配置されます。一見するとファイル数が多く見えますが、これらはすべて`ImageCanvas`の責務と密接に関連する、核となるドメインオブジェクト群です。

これらを`ValueObjects`や`Services`といった役割ベースのサブフォルダに分割することは、`namespace-rule.md`の指針に反し、かえって`ImageCanvas`との関係性を不明瞭にしてしまいます。

したがって、現時点ではこのフラットな構造が、`ImageCanvas`を中心としたドメインの構造を最も素直に表現していると判断します。将来的にコンテキストが肥大化し、明確に分離可能なサブドメイン（例: フィルター、レイヤー）が出現した時点で、改めてサブフォルダの作成を検討します。

### 8.3. 最終的な設計判断の理由

最終的に推奨する混合スタイルは、以下の理由により、Eedeプロジェクトにとって最もバランスの取れたアプローチであると結論付けました。

1.  **ドメイン中心の表現**: `ImageEditing`というビジネスコンテキストで分類し、その中心である`ImageCanvas`を集約ルートとしてフォルダのトップレベルに配置することで、ドメインの構造をコード上で直感的に表現できます。
2.  **役割ベース分類の回避**: `Aggregates`や`ValueObjects`といった実装の詳細で分類することを避け、`namespace-rule.md`の指針を遵守します。
3.  **過剰な階層化の防止**: ネームスペースが不必要に深くなることを防ぎ、可読性と開発効率を維持します。

これにより、形式的なルールに固執するのではなく、DDDの原則とプロジェクトの指針に基づいた、実用的で保守性の高いフォルダ構造を実現します。
