using Eede.Domain.Files;
using NUnit.Framework;

namespace Eede.Domain.Tests.Files
{
    [TestFixture]
    public class FilePathTests
    {
        [Test]
        public void FilePathTest()
        {
            FilePath path = new(@"Files\test\test.png");
            Assert.AreEqual(@"Files\test\test.png", path.Path);
        }

        [TestCase(false, @"Files\test\test.png")]
        [TestCase(true, "")]
        public void IsEmptyTest(bool expected, string path)
        {
            FilePath p = new(path);
            Assert.AreEqual(expected, p.IsEmpty());
        }
    }
}