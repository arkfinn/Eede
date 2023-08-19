using Eede.Domain.Files;
using Eede.Domain.Positions;
using NUnit.Framework;
using System;

namespace Eede.Infrastructure.Pictures.Tests
{
    [TestFixture()]
    public class PictureFileReaderTests
    {
        [Test]
        public void Pngファイルを開くことができる()
        {
            var filepath = new FilePath(@"SamplePictures\PngFile.png");
            var destination = new PictureFileReader(filepath).Read();
            var col = destination.PickColor(new Position(1, 1));
            Assert.That(new[] { col.Red, col.Green, col.Blue }, Is.EqualTo(new[] { 255, 0, 0 }));
        }

        [Test]
        public void アルファチャンネル無しのPngファイルを開くことができる()
        {
            var filepath = new FilePath(@"SamplePictures\PngFile16.png");
            var destination = new PictureFileReader(filepath).Read();
            var col = destination.PickColor(new Position(1, 1));
            Assert.That(new[] { col.Red, col.Green, col.Blue }, Is.EqualTo(new[] { 0, 0, 0 }));
        }

        [Test]
        public void インデックス256色のPngファイルを開くことができる()
        {
            var filepath = new FilePath(@"SamplePictures\PngFile8.png");
            var destination = new PictureFileReader(filepath).Read();
            var col = destination.PickColor(new Position(1, 1));
            Assert.That(new[] { col.Red, col.Green, col.Blue }, Is.EqualTo(new[] { 0, 0, 0 }));
        }

        [Test]
        public void Bmpファイルを開くことができる()
        {
            var filepath = new FilePath(@"SamplePictures\BmpFile.bmp");
            var destination = new PictureFileReader(filepath).Read();
            var col = destination.PickColor(new Position(1, 1));
            Assert.That(new[] { col.Red, col.Green, col.Blue }, Is.EqualTo(new[] { 255, 0, 0 }));
        }

        [Test]
        public void 引数nullでnewはできない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new PictureFileReader(null);
            });
        }

        [Test]
        public void 引数Pathの中身が空でnewはできない()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new PictureFileReader(new FilePath(""));
            });
        }
    }
}