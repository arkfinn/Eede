using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eede;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Eede.Positions;

namespace Eede.Tests
{
    [TestClass()]
    public class ColorDropperTests
    {
        [TestMethod()]
        public void 引数nullによる作成を許容しない()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                new ColorDropper(null);
            });
        }

        [TestMethod()]
        public void DropTest()
        {
            var b = new Bitmap(10, 10);
            b.SetPixel(1, 2, Color.DarkSeaGreen);
            b.SetPixel(2, 3, Color.Beige);

            var d = new ColorDropper(b);

            Assert.AreEqual(Color.DarkSeaGreen.ToArgb(), d.Drop(new Position(1, 2)).ToArgb());
            Assert.AreEqual(Color.Beige.ToArgb(), d.Drop(new Position(2, 3)).ToArgb());
        }

        [TestMethod()]
        public void Dropの引数posはbitmapの範囲外を許容しない()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                var b = new Bitmap(10, 10);
                var d = new ColorDropper(b);
                d.Drop(new Position(10, 10));
            });
        }
    }
}