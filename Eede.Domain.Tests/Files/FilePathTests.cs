using NUnit.Framework;

namespace Eede.Domain.Files.Tests
{
    [TestFixture]
    public class FilePathTests
    {
        [Test]
        public void FilePathTest()
        {
            var path = new FilePath(@"Files\test\test.png");
            Assert.AreEqual(@"Files\test\test.png", path.Path);
        }

        [TestCase(false, @"Files\test\test.png")]
        [TestCase(true, "")]
        public void IsEmptyTest(bool expected, string path)
        {
            var p = new FilePath(path);
            Assert.AreEqual(expected, p.IsEmpty());
        }
    }
}