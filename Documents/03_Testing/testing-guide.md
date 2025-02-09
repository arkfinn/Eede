# Eede テストガイド

## 1. テスト環境のセットアップ

### 1.1 必要なツール

- Visual Studio 2022 または Visual Studio Code
- .NET SDK 7.0 以上
- NUnit テストフレームワーク
- Moq モッキングフレームワーク

### 1.2 テストプロジェクトの構造

```
Eede/
├── Eede.Domain.Tests/        # ドメインロジックのテスト
├── Eede.Application.Tests/   # アプリケーション層のテスト
└── Eede.Infrastructure.Tests/# インフラストラクチャ層のテスト
```

## 2. テストの種類

### 2.1 単体テスト

- 個々のクラスやメソッドの動作を検証
- 外部依存をモックに置き換え
- 境界値や異常系のテストを含む

```csharp
[TestFixture]
public class PictureTests
{
    [Test]
    public void Resize_WithValidSize_ReturnResizedPicture()
    {
        // Arrange
        var picture = new Picture(100, 100);
        var newSize = new PictureSize(50, 50);

        // Act
        var resized = picture.Resize(newSize);

        // Assert
        Assert.That(resized.Size.Width, Is.EqualTo(50));
        Assert.That(resized.Size.Height, Is.EqualTo(50));
    }
}
```

### 2.2 統合テスト

- 複数のコンポーネント間の連携をテスト
- 実際の依存関係を使用
- エンドツーエンドのシナリオを検証

```csharp
[TestFixture]
public class ImageProcessingIntegrationTests
{
    private IPictureReader _reader;
    private IPictureWriter _writer;

    [SetUp]
    public void Setup()
    {
        _reader = new PictureReader();
        _writer = new PictureWriter();
    }

    [Test]
    public async Task ProcessAndSaveImage_ShouldSucceed()
    {
        // Arrange
        var path = "test-image.png";
        var processor = new ImageProcessor(_reader, _writer);

        // Act
        await processor.ProcessAndSaveAsync(path);

        // Assert
        Assert.That(File.Exists(path), Is.True);
    }
}
```

## 3. テスト作成のガイドライン

### 3.1 テストの命名規則

- メソッド名*シナリオ*期待される結果

```csharp
LoadImage_WithInvalidPath_ThrowsFileNotFoundException
ProcessImage_WithLargeFile_CompletesSuccessfully
```

### 3.2 AAA パターン

1. **Arrange**: テストの準備
2. **Act**: テスト対象の実行
3. **Assert**: 結果の検証

### 3.3 テストデータ

- テストフィクスチャーの活用
- 意味のあるテストデータの使用
- 共通のテストヘルパーの作成

```csharp
public static class PictureTestHelper
{
    public static Picture CreateSamplePicture(int width, int height)
    {
        return new Picture(width, height);
    }
}
```

## 4. モックの使用

### 4.1 モックの基本

```csharp
[Test]
public void SavePicture_CallsWriter()
{
    // Arrange
    var mockWriter = new Mock<IPictureWriter>();
    var picture = PictureTestHelper.CreateSamplePicture(100, 100);

    // Act
    var processor = new PictureProcessor(mockWriter.Object);
    processor.Save(picture, "test.png");

    // Assert
    mockWriter.Verify(w => w.Write(picture, "test.png"), Times.Once);
}
```

### 4.2 スタブの活用

```csharp
var stubReader = new Mock<IPictureReader>();
stubReader.Setup(r => r.Read("test.png"))
         .Returns(PictureTestHelper.CreateSamplePicture(100, 100));
```

## 5. テストの実行

### 5.1 Visual Studio でのテスト実行

1. テストエクスプローラーを開く
2. テストを選択して実行
3. 結果を確認

### 5.2 コマンドラインでのテスト実行

```bash
dotnet test Eede.sln              # すべてのテストを実行
dotnet test Eede.Domain.Tests     # 特定のプロジェクトのテストを実行
```

## 6. テストカバレッジ

### 6.1 カバレッジの目標

- ドメイン層: 90%以上
- アプリケーション層: 80%以上
- インフラストラクチャ層: 70%以上

### 6.2 カバレッジレポートの生成

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## 7. パフォーマンステスト

### 7.1 パフォーマンス基準

- 画像読み込み: 1 秒以内
- 基本的な編集操作: 100ms 以内
- 保存処理: 2 秒以内

### 7.2 パフォーマンステストの実装

```csharp
[Test]
public void ImageLoad_Performance_UnderOneSecond()
{
    // Arrange
    var stopwatch = new Stopwatch();
    var loader = new ImageLoader();

    // Act
    stopwatch.Start();
    var image = loader.Load("large-test-image.png");
    stopwatch.Stop();

    // Assert
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000));
}
```

## 8. テスト自動化

### 8.1 CI/CD パイプラインでのテスト

- プルリクエスト時に自動実行
- マージ前のテスト成功を必須化
- テストレポートの自動生成

### 8.2 定期的なテスト実行

- 夜間ビルドでの完全テスト実行
- パフォーマンステストの定期実行
- 結果の自動通知

## 9. テストメンテナンス

### 9.1 テストコードのレビュー

- テストの品質確認
- カバレッジの確認
- テストの可読性チェック

### 9.2 テストの更新

- 機能変更時のテスト更新
- 不要なテストの削除
- テストケースの最適化

## 10. よくある問題と解決策

### 10.1 フレークテスト（不安定なテスト）

- 非同期処理の適切な待機
- テスト環境の独立性確保
- タイムアウトの適切な設定

### 10.2 遅いテスト

- テストの並列実行
- テストデータの最適化
- モックの適切な使用

## 更新履歴

このガイドは、新しいテスト手法やベストプラクティスの導入に応じて定期的に更新されます。最新のテスト手法やツールについて、チーム内で継続的に議論と改善を行っていきます。
