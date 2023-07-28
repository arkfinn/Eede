using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.Tests.ImageBlenders
{
    [TestFixture]
    public class AlphaOnlyImageBlenderTests
    {
        [Test]
        public void TestBlend()
        {
            var src = new Bitmap(@"ImageBlenders\test\blend.png");
            var dst = new Bitmap(@"ImageBlenders\test\base.png");

            var blender = new AlphaOnlyImageBlender();
            var picture = new Picture(dst);

            var result = picture.Blend(blender, src, new Positions.Position(0, 0));

            // result.ToImage().Save(@"ImageBlenders\test\alpha_only_blend.png", ImageFormat.Png);
            var expected = new Bitmap(@"ImageBlenders\test\alpha_only_blend.png");
            Assert.IsTrue(ImageComparer.Equals(result.ToImage(), expected));
        }
    }
}
