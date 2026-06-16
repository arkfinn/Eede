using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing
{
    [TestFixture]
    public class CoordinateHistoryTests
    {
        [Test]
        public void Update_ShouldMoveNowToLastAndInputToNow()
        {
            CoordinateHistory h = new(new CanvasCoordinate(1, 2));
            Assert.That(h.Start, Is.EqualTo(new CanvasCoordinate(1, 2)));
            Assert.That(h.Last, Is.EqualTo(new CanvasCoordinate(1, 2)));
            Assert.That(h.Now, Is.EqualTo(new CanvasCoordinate(1, 2)));

            CoordinateHistory h2 = h.Update(new CanvasCoordinate(3, 4));
            Assert.That(h2.Start, Is.EqualTo(new CanvasCoordinate(1, 2)));
            Assert.That(h2.Last, Is.EqualTo(new CanvasCoordinate(1, 2)));
            Assert.That(h2.Now, Is.EqualTo(new CanvasCoordinate(3, 4)));

            CoordinateHistory h3 = h2.Update(new CanvasCoordinate(5, 6));
            Assert.That(h3.Start, Is.EqualTo(new CanvasCoordinate(1, 2)));
            Assert.That(h3.Last, Is.EqualTo(new CanvasCoordinate(3, 4)));
            Assert.That(h3.Now, Is.EqualTo(new CanvasCoordinate(5, 6)));

            CoordinateHistory h4 = h3.Update(new CanvasCoordinate(7, 8));
            Assert.That(h4.Start, Is.EqualTo(new CanvasCoordinate(1, 2)));
            Assert.That(h4.Last, Is.EqualTo(new CanvasCoordinate(5, 6)));
            Assert.That(h4.Now, Is.EqualTo(new CanvasCoordinate(7, 8)));
        }

        [Test]
        public void ToPositionHistory_ShouldConvertCoordinatesToPositions()
        {
            CoordinateHistory h = new CoordinateHistory(new CanvasCoordinate(1, 2))
                .Update(new CanvasCoordinate(3, 4))
                .Update(new CanvasCoordinate(5, 6));

            PositionHistory ph = h.ToPositionHistory();

            Assert.That(ph.Start, Is.EqualTo(new Position(1, 2)));
            Assert.That(ph.Last, Is.EqualTo(new Position(3, 4)));
            Assert.That(ph.Now, Is.EqualTo(new Position(5, 6)));
        }
    }
}