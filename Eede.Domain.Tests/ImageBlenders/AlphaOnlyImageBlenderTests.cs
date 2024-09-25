using Eede.Domain.ImageBlenders;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageBlenders
{
    [TestFixture]
    public class AlphaOnlyImageBlenderTests
    {
        [Test]
        public void TestBlend()
        {
            Picture src = ReadPicture(@"ImageBlenders\test\blend.png");
            AlphaOnlyImageBlender blender = new();
            Picture dst = ReadPicture(@"ImageBlenders\test\base.png");

            Picture result = dst.Blend(blender, src, new Position(0, 0));

            // result.ToImage().Save(@"ImageBlenders\test\alpha_only_blend.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"ImageBlenders\test\alpha_only_blend.png");
            Assert.That(result.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return PictureHelper.ReadBitmap(path);
        }
    }
}
