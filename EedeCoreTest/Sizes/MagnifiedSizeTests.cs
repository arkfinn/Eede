using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eede.Sizes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Eede.Sizes.Tests
{
    [TestClass()]
    public class MagnifiedSizeTests
    {
        [TestMethod()]
        public void MagnifiedSizeTest()
        {
            var size = new MagnifiedSize(new Size(8, 4), new Magnification(8));
            Assert.AreEqual(64, size.Width);
            Assert.AreEqual(32, size.Height);
            Assert.AreEqual(new Size(64, 32), size.ToSize());
        }

        [TestMethod()]
        public void 引数nullによる作成を許容しない()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var size = new MagnifiedSize(new Size(0, 0), null);
            });
        }
    }
}