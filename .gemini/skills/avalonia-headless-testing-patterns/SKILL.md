---
name: avalonia-headless-testing-patterns
description: Avalonia のヘッドレステスト環境セットアップ、非UI環境でのビットマップピクセルアクセス、および ReactiveUI コンポーネントのテスト戦略。
---

# Skill: Avalonia Headless Testing Patterns

## Description
Avalonia UI コンポーネントや ViewModel を、GUIのない環境（CI/CDやヘッドレス環境）でテストするためのパターン。プラットフォームサービスの初期化、ビットマップデータへの安全なアクセス、およびリアクティブパイプラインの同期方法に焦点を当てます。

## Key Patterns / Strategies

### 1. ヘッドレス環境のセットアップ
`Cursor` やプラットフォームサービスに依存するコンポーネントをテストする場合、`Avalonia.Headless.NUnit` 等を使用し、`TestAppBuilder` による初期化を登録する必要があります。

```csharp
// TestAppBuilder.cs
using Avalonia;
using Avalonia.Headless;
[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace MyProject.Tests;

public static class TestAppBuilder {
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
```
テストメソッドには `[Test]` の代わりに `[AvaloniaTest]` を使用します。

### 2. WriteableBitmap への安全なアクセス
ヘッドレス環境の `WriteableBitmap` 実装では `CopyPixels` が `NotSupportedException` を投げる場合があります。互換性を保つために `Lock()` と `Marshal.Copy` を組み合わせたフォールバックを使用します。

```csharp
public static void CopyToBuffer(Bitmap bitmap, byte[] targetBuffer) {
    if (bitmap is WriteableBitmap wb) {
        using (var lockBuffer = wb.Lock()) {
            System.Runtime.InteropServices.Marshal.Copy(lockBuffer.Address, targetBuffer, 0, targetBuffer.Length);
        }
    } else {
        // 標準 Bitmap の場合のフォールバック（要 GCHandle）
        // GCHandle pinnedArray = GCHandle.Alloc(targetBuffer, GCHandleType.Pinned);
        // bitmap.CopyPixels(rect, pinnedArray.AddrOfPinnedObject(), ...);
    }
}
```

### 3. ReactiveUI パイプラインのフラッシュ
`TestScheduler` を使用して `WhenAnyValue` や `BindTo` をテストする場合、プロパティ変更後に `scheduler.AdvanceBy(1)` を呼び出さないと、リアクティブな更新が反映されません。

```csharp
new TestScheduler().With(scheduler => {
    var viewModel = new MyViewModel();
    viewModel.Property = newValue;
    
    // リアクティブパイプラインを実行
    scheduler.AdvanceBy(1);
    
    Assert.That(viewModel.DerivedStatus, Is.EqualTo(ExpectedStatus));
});
```

### 4. 配列を持つ record 型の比較
C# の `record` 型は、配列フィールド（`byte[]` 等）に対して参照比較を行います。ピクセルデータを含むドメインモデルの比較では、インスタンス全体の `EqualTo` ではなく、サイズや長さ、あるいは内容のハッシュを比較します。

```csharp
Assert.Multiple(() => {
    Assert.That(actual.Size, Is.EqualTo(expected.Size));
    Assert.That(actual.ImageData.Length, Is.EqualTo(expected.ImageData.Length));
});
```
