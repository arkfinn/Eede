using Eede.Domain.Files;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Eede.Infrastructure.Tests.Pictures
{
    [TestFixture()]
    public class PictureFileWriterTests
    {
        [Test()]
        public void Pngファイルを保存できる()
        {
            FilePath saveFilepath = Location("\\SamplePictures\\SavePngFile.png");
            File.Delete(saveFilepath.Path);
            FilePath filepath = Location("\\SamplePictures\\PngFile.png");
            PictureFileReader reader = new(filepath);
            PictureFileWriter writer = new(saveFilepath);
            Domain.Pictures.Picture picture = reader.Read();

            writer.Write(picture);

            using Bitmap destination = new(Image.FromFile(saveFilepath.Path));
            Color col = destination.GetPixel(1, 1);
            Assert.AreEqual(255, col.R);
            Assert.AreEqual(0, col.G);
            Assert.AreEqual(0, col.B);
        }

        private FilePath Location(string path)
        {
            return new FilePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + path);
        }

        [Test]
        public void 引数nullでnewはできない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new PictureFileWriter(null);
            });
        }

        [Test]
        public void 引数Pathの中身が空でnewはできない()
        {
            _ = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = new PictureFileWriter(new FilePath(""));
            });
        }

        [Test]
        public void 引数pictureがnullでWriteはできない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                string saveFilepath = Environment.CurrentDirectory + "\\SamplePictures\\SavePngFile.png";
                PictureFileWriter writer = new(new FilePath(saveFilepath));
                writer.Write(null);
            });
        }
    }
}