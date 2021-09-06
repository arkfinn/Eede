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
    public class PicturePushedEventArgsTests
    {
        [TestMethod()]
        public void PicturePushedEventArgsTest()
        {
            var b = new Bitmap(1, 1);
            var p = new PicturePushedEventArgs(b, new Position(2, 2));
            Assert.AreEqual(b, p.Bitmap);
            Assert.AreEqual(new Position(2, 2), p.Position);
        }

        [TestMethod()]
        public void 引数graphicsについてnullによる作成を許可しない()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var h = new PicturePushedEventArgs(null, new Position(2, 2));
            });
        }

        [TestMethod()]
        public void 引数positionについてnullによる作成を許可しない()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var h = new PicturePushedEventArgs(new Bitmap(1, 1), null);
            });
        }
    }
}