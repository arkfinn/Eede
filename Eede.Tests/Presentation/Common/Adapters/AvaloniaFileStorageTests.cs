using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Eede.Presentation.Common.Adapters;
using Moq;
using NUnit.Framework;

namespace Eede.Tests.Presentation.Common.Adapters
{
    [TestFixture]
    public class AvaloniaFileStorageTests
    {
        [Test]
        public async Task CacheFile_DoesNotThrow_WhenFileUriIsNotLocalFile()
        {
            var storageFileMock = new Mock<IStorageFile>();
            var blobUri = new Uri("blob:http://localhost:5000/550e8400-e29b-41d4-a716-446655440000");

            storageFileMock.Setup(x => x.Path).Returns(blobUri);
            storageFileMock.Setup(x => x.Name).Returns("dot_art.png");
            storageFileMock.Setup(x => x.OpenReadAsync()).ReturnsAsync(new MemoryStream([0x89, 0x50]));

            // LocalPath アクセスによる InvalidOperationException が発生しないこと
            Assert.DoesNotThrow(() => AvaloniaFileStorage.CacheFile(storageFileMock.Object));

            // URI 文字列でもファイル名でもキャッシュからストリームを取得できること
            await using var streamByUri = await AvaloniaFileStorage.TryOpenReadStreamStaticAsync(blobUri.ToString());
            Assert.That(streamByUri, Is.Not.Null);

            await using var streamByName = await AvaloniaFileStorage.TryOpenReadStreamStaticAsync("dot_art.png");
            Assert.That(streamByName, Is.Not.Null);
        }

        [Test]
        public async Task TryOpenStreamStaticAsync_WhenPathIsRooted_DoesNotFallbackToCachedBasenameFromAnotherDirectory()
        {
            var dirAPath = Path.Combine(Path.GetTempPath(), "dirA", "collision_test.png");
            var dirBPath = Path.Combine(Path.GetTempPath(), "dirB", "collision_test.png");

            var storageFileMock = new Mock<IStorageFile>();
            storageFileMock.Setup(x => x.Path).Returns(new Uri(dirAPath));
            storageFileMock.Setup(x => x.Name).Returns("collision_test.png");
            storageFileMock.Setup(x => x.OpenReadAsync()).ReturnsAsync(() => new MemoryStream([0x01, 0x02]));
            storageFileMock.Setup(x => x.OpenWriteAsync()).ReturnsAsync(() => new MemoryStream());

            AvaloniaFileStorage.CacheFile(storageFileMock.Object);

            // 同一のルートパスでは取得可能
            await using var readDirA = await AvaloniaFileStorage.TryOpenReadStreamStaticAsync(dirAPath);
            Assert.That(readDirA, Is.Not.Null);

            // 別ディレクトリのルートパスでは同名ファイルのキャッシュにフォールバックしないこと
            await using var readDirB = await AvaloniaFileStorage.TryOpenReadStreamStaticAsync(dirBPath);
            Assert.That(readDirB, Is.Null);

            await using var writeDirB = await AvaloniaFileStorage.TryOpenWriteStreamStaticAsync(dirBPath);
            Assert.That(writeDirB, Is.Null);
        }
    }
}