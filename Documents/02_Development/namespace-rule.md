### **名前空間の命名ガイドライン**

#### **1. 構成**

名前空間は、`CompanyName.ProductName.Category`のような形式で構成するのが一般的です。

- **CompanyName（会社名または組織名）**: あなたが所属する組織の名前。例：`Microsoft`, `Google`
- **ProductName（製品名）**: プロジェクトやアプリケーション名。例：`Office`, `Maps`, `Azure`
- **Category（カテゴリ）**: **ビジネス上の関心事や境界づけられたコンテキスト**を表す部分。技術的なレイヤー（例: `Web`, `Data`）や、ビジネス上の活動（例: `ImageEditing`, `Payments`）が該当します。

**良い例：**

- `Contoso.Commerce.Payments`
- `AdventureWorks.Management.Reporting`

※Eede プロジェクトでは、`Eede.Layer.Category` のように構成しています。

#### **2. 大文字と小文字の使い分け**

- **パスカルケース（PascalCase）** を使用します。
- 各単語の先頭を大文字にします。

**良い例：**

- `System.Threading`
- `Microsoft.AspNetCore.Mvc`

#### **3. 複数形と単数形**

- **複数形は避ける**のが一般的なベストプラクティスです。
- 名前空間は、特定の機能やカテゴリをグループ化するものであり、コレクションを表すものではないためです。

**良い例：**

- `Contoso.Commerce.Services` (サービスの集まり)
- `Contoso.Commerce.Data` (データの集まり)

**悪い例：**

- `Contoso.Commerce.Services` -> `Contoso.Commerce.Service`
- `Contoso.Commerce.Data` -> `Contoso.Commerce.Datum`
- `Contoso.Commerce.Interfaces` -> `Contoso.Commerce.Interface`

上記の例では、**`Services`**、**`Data`**、**`Interfaces`** という複数形が使われていますが、これは**「サービス群」**、**「データアクセス群」**、**「インターフェース群」**というカテゴリを意味しているため、**良い例**となります。一方で、単数形にすべきか複数形にすべきかで悩んだ場合は、**単数形**を選択することが一般的です。

しかし、以下のようにクラス名と混同しやすい場合は、**複数形**を使用することがあります。

- `System.Collections`
- `System.Collections.Generic`

#### **4. 役割名（例：ValueObjects）を名前空間に使用しない**

これは、特に DDD（ドメイン駆動設計）のようなアーキテクチャで重要になります。
名前空間は、**「何のためのコードか？」**（例：認証機能、支払い機能）という**ドメインのコンテキスト**に基づいて命名するべきです。
**「どのような役割を持つコードか？」**（例：値オブジェクト、エンティティ、サービス）という実装の詳細や役割名で命名すると、以下のような問題が発生します。

- **ドメインの理解を妨げる**: `Contoso.Commerce.ValueObjects` という名前空間では、その値オブジェクトが「何」に関連するものなのかがわかりません。
- **名前空間が肥大化する**: プロジェクト全体で`ValueObjects`, `Services`, `Repositories`のような名前空間を作成すると、関連性の薄いクラスが一箇所に集まり、コードの探索性が低下します。
- **役割の変更に対応しにくい**: クラスの役割が変わった場合、名前空間も変更する必要が生じ、リファクタリングが複雑になります。

**良い例（コンテキストベースの命名）：**

- `Contoso.Commerce.Orders`
- `Contoso.Commerce.Products`
- `Contoso.Commerce.Payments`

これらの名前空間には、注文に関連する`Order`エンティティや`OrderId`値オブジェクト、`OrderService`などが含まれます。これにより、コードがドメインごとに整理され、見通しが良くなります。

**悪い例（役割ベースの命名）：**

- `Contoso.Commerce.ValueObjects`
- `Contoso.Commerce.Entities`
- `Contoso.Commerce.Services`


### **その他の注意点**

- **予約語**（`class`, `string`, `int`など）は使用しないでください。
- **アンダースコア（\_）** などの特殊文字は使用しないでください。
- 名前空間の階層が深くなりすぎないように注意しましょう。通常、3 ～ 4 階層程度が理想的です。
