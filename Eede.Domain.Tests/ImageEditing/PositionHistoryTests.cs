using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing
{
    [TestFixture]
    public class PositionHistoryTests
    {
        [Test]
        public void Updateすると入力値がNowに入り直前の値がLastに入る()
        {
            PositionHistory h = new(new Position(1, 2));
            Assert.That(h.Start, Is.EqualTo(new Position(1, 2)));
            Assert.That(h.Last, Is.EqualTo(new Position(1, 2)));
            Assert.That(h.Now, Is.EqualTo(new Position(1, 2)));

            PositionHistory h2 = h.Update(new Position(3, 4));
            Assert.That(h2.Start, Is.EqualTo(new Position(1, 2)));
            Assert.That(h2.Last, Is.EqualTo(new Position(1, 2)));
            Assert.That(h2.Now, Is.EqualTo(new Position(3, 4)));

            PositionHistory h3 = h2.Update(new Position(5, 6));
            Assert.That(h3.Start, Is.EqualTo(new Position(1, 2)));
            Assert.That(h3.Last, Is.EqualTo(new Position(3, 4)));
            Assert.That(h3.Now, Is.EqualTo(new Position(5, 6)));

            PositionHistory h4 = h3.Update(new Position(7, 8));
            Assert.That(h4.Start, Is.EqualTo(new Position(1, 2)));
            Assert.That(h4.Last, Is.EqualTo(new Position(5, 6)));
            Assert.That(h4.Now, Is.EqualTo(new Position(7, 8)));
        }
    }
}