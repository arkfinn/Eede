using Eede.Domain.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eede;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.Files.Tests
{
    [TestClass()]
    public class FilePathTests
    {
        [TestMethod()]
        public void FilePathTest()
        {
            var path = new FilePath(@"Files\test\test.png");
            Assert.AreEqual(@"Files\test\test.png", path.Path);
        }

        [DataTestMethod()]
        [DataRow(false, @"Files\test\test.png")]
        [DataRow(true, "")]
        public void IsEmptyTest(bool expected, string path)
        {
            var p = new FilePath(path);
            Assert.AreEqual(expected, p.IsEmpty());
        }
    }
}