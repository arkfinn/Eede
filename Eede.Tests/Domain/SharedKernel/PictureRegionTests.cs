using System.Collections.Generic;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.SharedKernel
{
    [TestFixture]
    public class PictureRegionTests
    {
        [Test]
        public void IsEmpty_WhenNoAreasOrAllEmpty_ReturnsTrue()
        {
            var emptyRegion = new PictureRegion(new List<PictureArea>());
            Assert.That(emptyRegion.IsEmpty, Is.True);

            var allEmptyRegion = new PictureRegion(new List<PictureArea>
            {
                new PictureArea(new Position(10, 10), new PictureSize(0, 0)),
                new PictureArea(new Position(20, 20), new PictureSize(0, 5)),
            });
            Assert.That(allEmptyRegion.IsEmpty, Is.True);
        }

        [Test]
        public void IsEmpty_WhenContainsNonEmptyArea_ReturnsFalse()
        {
            var region = new PictureRegion(new List<PictureArea>
            {
                new PictureArea(new Position(10, 10), new PictureSize(0, 0)),
                new PictureArea(new Position(20, 20), new PictureSize(5, 5)),
            });
            Assert.That(region.IsEmpty, Is.False);
        }

        [Test]
        public void GetBoundingBox_IgnoresEmptyAreas()
        {
            // 空エリア（100, 100, 0, 0）が continue されずに結合されると、(10, 10)から(100, 100)まで広がってしまう
            var region = new PictureRegion(new List<PictureArea>
            {
                new PictureArea(new Position(100, 100), new PictureSize(0, 0)),
                new PictureArea(new Position(10, 10), new PictureSize(10, 10)),
                new PictureArea(new Position(15, 15), new PictureSize(10, 10)),
                new PictureArea(new Position(200, 200), new PictureSize(0, 0)),
            });

            var bbox = region.GetBoundingBox();
            Assert.Multiple(() =>
            {
                Assert.That(bbox.X, Is.EqualTo(10));
                Assert.That(bbox.Y, Is.EqualTo(10));
                Assert.That(bbox.Width, Is.EqualTo(15)); // (10, 10) 〜 (25, 25)
                Assert.That(bbox.Height, Is.EqualTo(15));
            });
        }
    }
}
