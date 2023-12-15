using Eede.Domain.Files;
using Eede.Domain.ImageBlenders;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageBlenders
{
    [TestFixture]
    public class AlphaImageBlenderTests
    {
        [Test]
        public void TestBlend()
        {
            Picture src = ReadPicture(@"ImageBlenders\test\blend.png");
            AlphaImageBlender blender = new();
            Picture dst = ReadPicture(@"ImageBlenders\test\base.png");

            Picture result = dst.Blend(blender, src, new Position(0, 0));

            // result.ToImage().Save(@"ImageBlenders\test\alpha_blend.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"ImageBlenders\test\alpha_blend.png");
            Assert.That(result.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return new PictureFileReader(new FilePath(path)).Read();
        }
    }
}
