# Eede コーディング規約

## 1. 基本方針

- クリーンで読みやすいコードを維持する
- 一貫性のある命名規則を使用する
- コードの品質と保守性を重視する

## 2. ファイル構成

### 2.1 ファイル名

- パスカルケース（PascalCase）を使用
- ファイル名はクラス名と一致させる
- 例：`DrawableArea.cs`, `PictureFile.cs`

### 2.2 ファイル構造

```csharp
// ライセンスヘッダー（必要な場合）

using System;
// 他のusing文（アルファベット順）

namespace Eede.Domain
{
    // インターフェース、クラスの実装
}
```

## 3. 命名規則

### 3.1 クラス・インターフェース

- パスカルケースを使用
- 名詞または名詞句を使用
- インターフェースは'I'プレフィックスを使用

```csharp
public interface IPictureReader
public class PictureFile
```

### 3.2 メソッド

- パスカルケースを使用
- 動詞または動詞句を使用

```csharp
public void DrawLine()
public Picture LoadImage()
```

### 3.3 変数・パラメーター

- キャメルケース（camelCase）を使用
- 意味のある名前を使用
- 単一文字の変数は避ける（ループカウンターを除く）

```csharp
private readonly string filePath;
public void ProcessImage(int width, int height)
```

### 3.4 定数

- 大文字のスネークケース（SNAKE_CASE）を使用

```csharp
private const int MAX_IMAGE_SIZE = 4096;
```

## 4. コードフォーマット

### 4.1 インデント

- スペース 4 文字を使用
- タブ文字は使用しない

### 4.2 波括弧

- 開き括弧は新しい行に配置
- 閉じ括弧も新しい行に配置

```csharp
public class Example
{
    public void Method()
    {
        // 実装
    }
}
```

### 4.3 行の長さ

- 120 文字を超えないようにする
- 長い行は適切に改行

## 5. コメント

### 5.1 コメントスタイル

- パブリック API には必ず XML ドキュメントコメントを付ける
- 複雑なロジックには説明コメントを追加

```csharp
/// <summary>
/// 画像をロードし、指定されたサイズにリサイズします。
/// </summary>
/// <param name="path">画像ファイルのパス</param>
/// <returns>リサイズされた画像</returns>
public Picture LoadAndResize(string path)
```

### 5.2 TODO コメント

- 一時的な解決策や将来の改善点には TODO コメントを使用

```csharp
// TODO: パフォーマンスの最適化が必要
```

## 6. 例外処理

### 6.1 例外の使用

- ビジネスロジックの例外は専用の例外クラスを作成
- catch ブロックは具体的な例外を指定

```csharp
try
{
    // 処理
}
catch (IOException ex)
{
    // 具体的な例外処理
}
```

### 6.2 エラー処理

- null チェックは積極的に行う
- 戻り値の型として null を返すことは避ける
- 可能な場合は Null オブジェクトパターンを使用

## 7. LINQ の使用

- 読みやすさを重視
- 複雑なクエリは複数行に分割
- メソッド構文を優先して使用

```csharp
var result = collection
    .Where(x => x.IsValid)
    .Select(x => x.Value)
    .ToList();
```

## 8. ユニットテスト

### 8.1 テストの命名

- ［テスト対象のメソッド］_［シナリオ］_［期待される結果］

```csharp
public void LoadImage_WithInvalidPath_ThrowsFileNotFoundException()
```

### 8.2 テストの構造

- Arrange-Act-Assert パターンを使用

```csharp
// Arrange
var path = "invalid/path";
var loader = new ImageLoader();

// Act
var action = () => loader.LoadImage(path);

// Assert
action.Should().Throw<FileNotFoundException>();
```

## 9. バージョン管理

### 9.1 コミットメッセージ

- 簡潔で明確な説明
- 現在形で記述
- プレフィックスを使用（feat:, fix:, docs:, etc）

### 9.2 ブランチ戦略

- feature/
- bugfix/
- release/
- hotfix/

## 10. レビュー基準

- すべてのパブリック API にドキュメントコメントがあるか
- テストが適切に書かれているか
- コーディング規約にしたがっているか
- パフォーマンスへの影響は考慮されているか
- セキュリティ上の問題はないか

このドキュメントは定期的にレビューされ、必要に応じて更新されます。
