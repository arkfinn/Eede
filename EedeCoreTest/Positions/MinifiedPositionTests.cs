using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eede.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Positions.Tests
{
    [TestClass()]
    public class MinifiedPositionTests
    {
        [TestMethod()]
        public void MinifiedPositionTest()
        {
            var pos = new MinifiedPosition(new Position(64, 32), new Magnification(8));
            Assert.AreEqual(8, pos.X);
            Assert.AreEqual(4, pos.Y);
            Assert.AreEqual(new Position(8, 4), pos.ToPosition());
        }

        [TestMethod()]
        public void 引数positionはnullによる作成を許容しない()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var size = new MinifiedPosition(null, new Magnification(8));
            });
        }

        [TestMethod()]
        public void 引数magnificationはnullによる作成を許容しない()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var size = new MinifiedPosition(new Position(64, 32), null);
            });
        }
    }
}