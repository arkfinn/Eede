using Eede.Domain.Files;
using Eede.Domain.Pictures;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Domain.ImageBlenders
{
    [TestFixture]
    public class DirectImageBlenderTests
    {
        [Test]
        public void TestBlend()
        {
            var src = ReadPicture(@"ImageBlenders\test\blend.png");
            var blender = new DirectImageBlender();
            var dst = ReadPicture(@"ImageBlenders\test\base.png");

            var result = dst.Blend(blender, src, new Positions.Position(0, 0));

            // result.ToImage().Save(@"ImageBlenders\test\direct_blend.png", ImageFormat.Png);
            var expected = ReadPicture(@"ImageBlenders\test\direct_blend.png");
            Assert.That(result.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return new PictureFileReader(new FilePath(path)).Read();
        }
    }
}
