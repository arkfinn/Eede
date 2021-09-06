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
    public class PositionTests
    {
        [TestMethod()]
        public void PositionTest()
        {
            var p = new Position(1, 2);
            Assert.AreEqual(1, p.X);
            Assert.AreEqual(2, p.Y);
        }

        [TestMethod()]
        public void PositionCreateFromPoint()
        {
            var p = new Position(new Point(1, 2));
            Assert.AreEqual(1, p.X);
            Assert.AreEqual(2, p.Y);
        }

        [TestMethod()]
        public void GetHashCodeTest()
        {
            Assert.AreEqual(new Position(1, 2), new Position(1, 2));
        }

        [TestMethod()]
        public void ToPointTest()
        {
            Assert.AreEqual(new Position(1, 2).GetHashCode(), new Position(1, 2).GetHashCode());
        }

        [TestMethod()]
        public void ToPointTest1()
        {
            Assert.AreEqual(new Point(1, 2), new Position(1, 2).ToPoint());
        }

        [DataTestMethod()]
        [DataRow(true, 0, 0)]
        [DataRow(false, 1, 0)]
        [DataRow(false, 0, 1)]
        [DataRow(false, -1, 0)]
        [DataRow(false, 0, -1)]
        public void IsInnerOfTest(bool expected, int x, int y)
        {
            Assert.AreEqual(expected, new Position(x, y).IsInnerOf(new Size(1, 1)));
        }
    }
}