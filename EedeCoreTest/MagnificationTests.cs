using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eede;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eede.Positions;

namespace Eede.Tests
{
    [TestClass()]
    public class MagnificationTests
    {
        [TestMethod()]
        public void OutOfRange()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                new Magnification(0);
            });
        }

        [TestMethod()]
        public void MagnifyTest()
        {
            var m = new Magnification(4);
            Assert.AreEqual(12, m.Magnify(3));
        }

        [TestMethod()]
        public void EqualsTest()
        {
            Assert.AreEqual(new Magnification(4), new Magnification(4));
        }

        [TestMethod()]
        public void GetHashCodeTest()
        {
            Assert.AreEqual(new Magnification(4).GetHashCode(), new Magnification(4).GetHashCode());
        }
    }
}
