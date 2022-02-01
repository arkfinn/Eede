using Eede.Domain.Files;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Infrastructure.Pictures.Tests
{
    [TestFixture()]
    public class PictureFileReaderTests
    {
        [Test]
        public void Pngファイルを開くことができる()
        {
            var filepath = new FilePath(Environment.CurrentDirectory + "\\SamplePictures\\PngFile.png");
            using (var destination = new Bitmap(new PictureFileReader(filepath).Read().ToImage()))
            {
                var col = destination.GetPixel(1, 1);
                Assert.AreEqual(255, col.R);
                Assert.AreEqual(0, col.G);
                Assert.AreEqual(0, col.B);
            }
        }

        [Test]
        public void アルファチャンネル無しのPngファイルを開くことができる()
        {
            var filepath = new FilePath(Environment.CurrentDirectory + "\\SamplePictures\\PngFile16.png");
            using (var destination = new Bitmap(new PictureFileReader(filepath).Read().ToImage()))
            {
                var col = destination.GetPixel(1, 1);
                Assert.AreEqual(0, col.R);
                Assert.AreEqual(0, col.G);
                Assert.AreEqual(0, col.B);
            }
        }

        [Test]
        public void インデックス256色のPngファイルを開くことができる()
        {
            var filepath = new FilePath(Environment.CurrentDirectory + "\\SamplePictures\\PngFile8.png");
            using (var destination = new Bitmap(new PictureFileReader(filepath).Read().ToImage()))
            {
                var col = destination.GetPixel(1, 1);
                Assert.AreEqual(0, col.R);
                Assert.AreEqual(0, col.G);
                Assert.AreEqual(0, col.B);
            }
        }

        [Test]
        public void bmpファイルを開くことができる()
        {
            var filepath = new FilePath(Environment.CurrentDirectory + "\\SamplePictures\\BmpFile.bmp");
            using (var destination = new Bitmap(new PictureFileReader(filepath).Read().ToImage()))
            {
                var col = destination.GetPixel(1, 1);
                Assert.AreEqual(255, col.R);
                Assert.AreEqual(0, col.G);
                Assert.AreEqual(0, col.B);
            }
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