using Eede.Domain.Files;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;

namespace Eede.Infrastructure.Pictures.Tests
{
    [TestFixture()]
    public class PictureFileWriterTests
    {
        [Test()]
        public void Pngファイルを保存できる()
        {
            string saveFilepath = Environment.CurrentDirectory + "\\SamplePictures\\SavePngFile.png";
            File.Delete(saveFilepath);
            var filepath = new FilePath(Environment.CurrentDirectory + "\\SamplePictures\\PngFile.png");
            var reader = new PictureFileReader(filepath);
            var writer = new PictureFileWriter(new FilePath(saveFilepath));
            using (var picture = reader.Read())
            {
                writer.Write(picture);
            }
            using (var destination = new Bitmap(Image.FromFile(saveFilepath)))
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
                new PictureFileWriter(null);
            });
        }

        [Test]
        public void 引数Pathの中身が空でnewはできない()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new PictureFileWriter(new FilePath(""));
            });
        }

        [Test]
        public void 引数pictureがnullでWriteはできない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                string saveFilepath = Environment.CurrentDirectory + "\\SamplePictures\\SavePngFile.png";
                var writer = new PictureFileWriter(new FilePath(saveFilepath));
                writer.Write(null);
            });
        }
    }
}