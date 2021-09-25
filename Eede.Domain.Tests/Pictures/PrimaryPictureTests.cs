using NUnit.Framework;

namespace Eede.Domain.Pictures.Tests
{
    [TestFixture]
    public class PrimaryPictureTests
    {
        [Test]
        public void 引数imageがnullでnewはできない()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                var p = new Picture(null);
            });
        }
    }
}