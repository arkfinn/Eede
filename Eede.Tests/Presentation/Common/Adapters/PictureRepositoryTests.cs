#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Eede.Application.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Pictures;
using Eede.Presentation.Common.Adapters;
using Moq;
using NUnit.Framework;

namespace Eede.Tests.Presentation.Common.Adapters;

[TestFixture]
public class PictureRepositoryTests
{
    [AvaloniaTest]
    public async Task LoadAsync_WithCachedPng_DecodesUsingPictureCodec_PreservesExactColors()
    {
        // 1. Arrange: 赤と緑のピクセルを持つ Picture をエンコードした PNG バイト列を用意
        var codec = new SkiaSharpPictureCodec();
        var sampleBytes = new byte[2 * 1 * 4];
        // Pixel (0, 0): 赤 (B=0, G=0, R=255, A=255)
        sampleBytes[0] = 0;
        sampleBytes[1] = 0;
        sampleBytes[2] = 255;
        sampleBytes[3] = 255;
        // Pixel (1, 0): 緑 (B=0, G=255, R=0, A=255)
        sampleBytes[4] = 0;
        sampleBytes[5] = 255;
        sampleBytes[6] = 0;
        sampleBytes[7] = 255;
        var samplePicture = Picture.Create(new PictureSize(2, 1), sampleBytes);
        byte[] pngBytes = codec.EncodeToPng(samplePicture);

        var blobUri = new Uri("blob:http://localhost:5000/test_image.png");
        var storageFileMock = new Mock<IStorageFile>();
        storageFileMock.SetupGet(f => f.Path).Returns(blobUri);
        storageFileMock.SetupGet(f => f.Name).Returns("test_image.png");
        storageFileMock.Setup(f => f.OpenReadAsync()).ReturnsAsync(new MemoryStream(pngBytes));
        AvaloniaFileStorage.CacheFile(storageFileMock.Object);

        var bitmapAdapterMock = new Mock<IBitmapAdapter<Bitmap>>();
        var repo = new PictureRepository(bitmapAdapterMock.Object, codec);

        // 2. Act: PictureRepository からロード
        var picture = await repo.LoadAsync(new FilePath(blobUri.ToString()));

        // 3. Assert: SkiaSharpPictureCodec が使われて、赤と緑が正確に復元されること (RとBの反転がないこと)
        Assert.That(picture.Width, Is.EqualTo(2));
        Assert.That(picture.Height, Is.EqualTo(1));

        var pixel0 = picture.PickColor(new Position(0, 0));
        Assert.That(pixel0, Is.EqualTo(new ArgbColor(255, 255, 0, 0)), "ピクセル0 (赤) が色化け・反転せず復元されること");

        var pixel1 = picture.PickColor(new Position(1, 0));
        Assert.That(pixel1, Is.EqualTo(new ArgbColor(255, 0, 255, 0)), "ピクセル1 (緑) が色化け・反転せず復元されること");
    }
}
