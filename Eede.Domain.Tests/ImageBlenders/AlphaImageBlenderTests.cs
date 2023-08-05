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

namespace Eede.Domain.ImageBlenders
{
    [TestFixture]
    public class AlphaImageBlenderTests
    {
        [Test]
        public void TestBlend()
        {
            var srcBmp = new Bitmap(@"ImageBlenders\test\blend.png");
            var src = new Picture(srcBmp);
            var dst = new Bitmap(@"ImageBlenders\test\base.png");

            var blender = new AlphaImageBlender();
            var picture = new Picture(dst);

            var result = picture.Blend(blender, src, new Positions.Position(0, 0));

            // result.ToImage().Save(@"ImageBlenders\test\alpha_blend.png", ImageFormat.Png);
            var expected = new Bitmap(@"ImageBlenders\test\alpha_blend.png");
            Assert.That(ImageComparer.Equals(result.ToImage(), expected), Is.True);
        }
    }
}
