---
name: avalonia-reliable-clipboard
description: Avalonia 11における堅牢なクリップボード画像連携（コピー・ペースト）の実装パターン
---

# Skill: avalonia-reliable-clipboard

## Description
Avalonia 11の `IClipboard` APIにおいて、特定のプラットフォームやデータ形式（特に "Bitmap" オブジェクトそのもの）の取得が `null` を返す不安定さを回避し、画像データのコピー・ペーストを確実に行うためのパターンです。

## Key Patterns / Strategies

### 1. データの冗長化 (Multi-format Copy)
`Bitmap` オブジェクトだけでなく、互換性の高い `PNG` 形式のバイナリデータ（byte配列）を同時にセットすることで、復元率を高めます。

```csharp
public async Task CopyAsync(Bitmap bitmap)
{
    var dataObject = new DataObject();
    // 1. 標準のBitmapオブジェクトとしてセット
    dataObject.Set("Bitmap", bitmap);

    // 2. 互換性の高いPNG形式（byte[]）としてセット
    using (var ms = new MemoryStream())
    {
        bitmap.Save(ms);
        dataObject.Set("PNG", ms.ToArray());
    }
    await clipboard.SetDataObjectAsync(dataObject);
}
```

### 2. 多相的なデータ取得 (Flexible Paste)
取得側では複数の形式を優先順位に従って試行し、返されたデータの型（`Bitmap`, `Stream`, `byte[]`）に応じて適切にキャスト・変換します。

```csharp
private async Task<Bitmap> TryGetFromFormat(IClipboard clipboard, string format)
{
    object data = await clipboard.GetDataAsync(format);
    return data switch
    {
        Bitmap b => b,
        Stream s => new Bitmap(s),
        byte[] bytes => new Bitmap(new MemoryStream(bytes)),
        _ => null
    };
}
```

### 3. グローバルショートカットの定義場所
`UserControl` (`MainView`) 内の `KeyBindings` はフォーカス状態により反応しないことがあるため、アプリ全体で共通のショートカット（Ctrl+C/V等）は `Window` (`MainWindow`) レベルで定義します。

### 4. ファイルアクセスの型安全
クリップボードから取得した `IStorageItem` は `IStorageFile` にキャストしてから `OpenReadAsync()` を呼び出します。

```csharp
var data = await clipboard.GetDataAsync(DataFormats.Files);
var items = data as IEnumerable<IStorageItem>;
var firstFile = items?.OfType<IStorageFile>().FirstOrDefault();
if (firstFile != null)
{
    using var stream = await firstFile.OpenReadAsync();
    // ...
}
```
