using Eede.Domain.Files;
using NUnit.Framework;

namespace Eede.Domain.Pictures.Tests
{
    [TestFixture]
    public class PrimaryPictureTests
    {
        [Test]
        public void IsEmptyFileNameTest()
        {
            var p = new PrimaryPicture(new FilePath(""), new System.Drawing.Bitmap(1, 1));
            Assert.AreEqual(true, p.IsEmptyFileName());
        }
    }
}