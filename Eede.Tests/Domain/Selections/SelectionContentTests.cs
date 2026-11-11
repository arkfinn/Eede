using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.Selections
{
    [TestFixture]
    public class SelectionContentTests
    {
        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var picture = Picture.CreateEmpty(new PictureSize(10, 10));
            var selection = new Selection(new PictureArea(new Position(1, 2), new PictureSize(3, 4)));
            var content = new SelectionContent(picture, selection);

            Assert.Multiple(() =>
            {
                Assert.That(content.Image, Is.SameAs(picture));
                Assert.That(content.OriginalSelection, Is.SameAs(selection));
            });
        }
    }
}
