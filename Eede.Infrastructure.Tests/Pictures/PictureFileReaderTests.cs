using Eede.Domain.Files;
using Eede.Domain.Positions;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;
using System;

namespace Eede.Infrastructure.Tests.Pictures
{
    [TestFixture()]
    public class PictureFileReaderTests
    {
        [Test]
        public void Pngファイルを開くことができる()
        {
            FilePath filepath = new(@"SamplePictures\PngFile.png");
            Domain.Pictures.Picture destination = new PictureFileReader(filepath).Read();
            Domain.Colors.ArgbColor col = destination.PickColor(new Position(1, 1));
            Assert.That(new[] { col.Red, col.Green, col.Blue }, Is.EqualTo(new[] { 255, 0, 0 }));
        }

        [Test]
        public void アルファチャンネル無しのPngファイルを開くことができる()
        {
            FilePath filepath = new(@"SamplePictures\PngFile16.png");
            Domain.Pictures.Picture destination = new PictureFileReader(filepath).Read();
            Domain.Colors.ArgbColor col = destination.PickColor(new Position(1, 1));
            Assert.That(new[] { col.Red, col.Green, col.Blue }, Is.EqualTo(new[] { 0, 0, 0 }));
        }

        [Test]
        public void インデックス256色のPngファイルを開くことができる()
        {
            FilePath filepath = new(@"SamplePictures\PngFile8.png");
            Domain.Pictures.Picture destination = new PictureFileReader(filepath).Read();
            Domain.Colors.ArgbColor col = destination.PickColor(new Position(1, 1));
            Assert.That(new[] { col.Red, col.Green, col.Blue }, Is.EqualTo(new[] { 0, 0, 0 }));
        }

        [Test]
        public void Bmpファイルを開くことができる()
        {
            FilePath filepath = new(@"SamplePictures\BmpFile.bmp");
            Domain.Pictures.Picture destination = new PictureFileReader(filepath).Read();
            Domain.Colors.ArgbColor col = destination.PickColor(new Position(1, 1));
            Assert.That(new[] { col.Red, col.Green, col.Blue }, Is.EqualTo(new[] { 255, 0, 0 }));
        }

        [Test]
        public void 引数nullでnewはできない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new PictureFileReader(null);
            });
        }

        [Test]
        public void 引数Pathの中身が空でnewはできない()
        {
            _ = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = new PictureFileReader(new FilePath(""));
            });
        }
    }
}