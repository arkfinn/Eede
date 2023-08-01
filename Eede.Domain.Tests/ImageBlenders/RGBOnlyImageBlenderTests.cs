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
    public class RGBOnlyImageBlenderTests
    {
        [Test]
        public void TestBlend()
        {
            var srcBmp = new Bitmap(@"ImageBlenders\test\blend.png");
            var src = new Picture(srcBmp);
            var dst = new Bitmap(@"ImageBlenders\test\base.png");

            var blender = new RGBOnlyImageBlender();
            var picture = new Picture(dst);

            var result = picture.Blend(blender, src, new Positions.Position(0, 0));

            // result.ToImage().Save(@"ImageBlenders\test\rgb_blend.png", ImageFormat.Png);
            var expected = new Bitmap(@"ImageBlenders\test\rgb_blend.png");
            Assert.IsTrue(ImageComparer.Equals(result.ToImage(), expected));
        }
    }
}
