using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eede.Domain.Pictures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eede.Domain.Files;

namespace Eede.Domain.Pictures.Tests
{
    [TestClass()]
    public class PrimaryPictureTests
    {
        [TestMethod()]
        public void IsEmptyFileNameTest()
        {
            var p = new PrimaryPicture(new FilePath(""), new System.Drawing.Bitmap(1, 1));
            Assert.AreEqual(true, p.IsEmptyFileName());
        }
    }
}