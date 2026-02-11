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
            Assert.That(path.ToString(), Is.EqualTo(@"Files\test\test.png"));
        }

        [TestCase(false, @"Files\test\test.png")]
        [TestCase(true, "")]
        public void IsEmptyTest(bool expected, string path)
        {
            FilePath p = new(path);
            Assert.That(p.IsEmpty(), Is.EqualTo(expected));
        }
    }
}