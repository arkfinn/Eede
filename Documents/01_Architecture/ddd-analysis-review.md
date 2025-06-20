# DDD システム分析レビューと改善提案 (Eede)

## 概要

このドキュメントは、既存の `ddd-analysis.md` をレビューし、特にドット絵というコアドメインの特性に焦点を当て、ドメイン駆動開発（DDD）の観点から課題と改善提案をまとめたものです。

## ドット絵 (Pixel Art) の特性

ドット絵は、ピクセル単位の厳密な制御、限られた色数での表現、拡大表示での編集、アンチエイリアシングの回避、ニアレストネイバー法などの特定の拡大縮小アルゴリズムの使用、そしてアニメーション表現が特徴です。これらの特性は、システム設計において特に考慮されるべき点です。

## ドメイン駆動開発 (DDD) の基本

DDDは、ビジネスドメインの複雑さをモデル化し、ユビキタス言語、境界づけられたコンテキスト、集約、エンティティ、値オブジェクト、ドメインサービスといった概念を用いて、ドメインエキスパートと開発者が共通理解を深めながらソフトウェアを構築するアプローチです。

## レビュー結果と課題

### 1. ドメインの特定と境界の定義

*   **課題**:
    *   「画像操作ドメイン」の責任範囲が曖昧で、他のドメインと重複する可能性があります。
    *   「UI 操作サブドメイン」はドメイン層ではなく、UI/Presentation層またはApplication層の関心事であるべきです。
*   **提案**:
    *   「画像操作ドメイン」を「画像編集ドメイン」として再定義し、レイヤー管理や選択範囲操作など、より具体的な責任範囲を持たせることを検討します。
    *   「UI 操作サブドメイン」はUI/Presentation層の内部概念として扱います。

### 2. ユビキタス言語一覧

*   **課題**:
    *   「DrawingBuffer」や「RegionSelector」は実装やUIに密接な用語であり、ドメイン層のユビキタス言語としては再検討が必要です。
*   **提案**:
    *   ドメインエキスパートが理解しやすい「描画中のキャンバス」や「選択範囲 (Selection Area)」といった言葉に置き換えるか、UI/Application層の用語として明確に位置づけます。

### 3. 集約とエンティティの関係性

*   **課題**:
    *   `DrawStyle` が集約ルートとされていますが、これは描画の「振る舞い」を定義するものであり、データの一貫性を保証する集約の境界とは異なります。むしろドメインサービスに近い概念です。
    *   `MagnifiedSize` はUIの表示に関する概念であり、ドメイン層から削除すべきです。
*   **提案**:
    *   `DrawStyle` は集約ルートではなく、描画ロジックを実行するドメインサービスとして再定義します。`design-best-practices.md` の原則（ドメインサービスを極力排除）を考慮しつつ、複数のドメインオブジェクトにまたがる複雑な描画処理をカプセル化する役割として適切です。
    *   `MagnifiedSize` はUI層に移動します。
    *   レイヤー機能のサポートを検討する場合、`Layer` をエンティティとする集約の導入を検討します。

### 4. ドメインサービスの候補

*   **課題**:
    *   `IDrawStyle` の集約ルートとしての位置づけは再検討が必要です。
    *   ファイル操作サービス （`IPaletteFileReader`、`ActFileReader` など） の配置がDDDの原則と一部異なります。
*   **提案**:
    *   `IDrawStyle` は描画ロジックを実行するドメインサービスとして適切です。
    *   ファイル操作関連のインターフェースはドメイン層に置き、その実装はインフラストラクチャ層に置くべきです。

### 5. コンテキストマップ

*   **課題**:
    *   「画像操作ドメイン」がマップに明示されていません。
    *   `productContext.md` で言及されている「アニメーション確認ウィンドウ」に対応する「アニメーションコンテキスト」が欠落しています。
*   **提案**:
    *   「画像編集コンテキスト」を新規追加し、コンテキストマップに含めます。
    *   ドット絵アニメーションの複雑さを考慮し、「アニメーションコンテキスト」を独立したコンテキストとして追加することを強く推奨します。

### 6. バウンデッド・コンテキスト間の関係性

*   **課題**:
    *   「Drawing ↔ Image Processing」の関係が「パートナーシップ」とされていますが、`Drawing Context` が `Image Processing Context` を利用する上流・下流関係にあるため、「顧客/供給者 (Customer/Supplier)」関係の方がより適切です。
*   **提案**:
    *   関係性を「顧客/供給者」として再定義し、`Drawing Context` が `Image Processing Context` のサービスを利用する形を明確にします。

## 次のステップ

上記の課題と提案に基づき、`Documents/01_Architecture/ddd-analysis.md` を更新することを推奨します。特に、以下の点を優先的に修正・加筆します。

1.  **ドメインとコンテキストの再定義**: 「画像操作ドメイン」の明確化と「アニメーションコンテキスト」の追加。
2.  **ユビキタス言語の精査**: 実装寄りの用語の修正または位置づけの明確化。
3.  **集約とエンティティの役割の明確化**: `DrawStyle` の位置づけと `MagnifiedSize` の移動。
4.  **コンテキストマップの更新**: 新しいコンテキストの追加と既存の関係性の調整。

これらの変更は、ドット絵というコアドメインの特性をより深くDDDモデルに反映させ、システムの整合性と拡張性を向上させるために重要です。
