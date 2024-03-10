using Eede.Domain.Positions;
using NUnit.Framework;

namespace Eede.Domain.Pictures
{
    [TestFixture]
    public class PictureAreaTests
    {
        [TestCaseSource(nameof(UpdatePositionCases))]
        public void UpdatePositionTest(PictureArea area, Position to, PictureSize limit, PictureArea expected)
        {
            PictureArea result = area.UpdatePosition(to, limit);
            Assert.That((result.X, result.Y, result.Width, result.Height), Is.EqualTo((expected.X, expected.Y, expected.Width, expected.Height)));
        }

        private static readonly object[] UpdatePositionCases =
        {
            new object[] { new PictureArea(new Position(10, 20), new PictureSize(50, 60)),
                new Position(100, 200), new PictureSize(300, 400),
                new PictureArea(new Position(10, 20), new PictureSize(90, 180)) },
            new object[] { new PictureArea(new Position(10, 20), new PictureSize(50, 60)),
                new Position(0, 10), new PictureSize(300, 400),
                new PictureArea(new Position(0, 10), new PictureSize(10, 10)) },
        };
    }
}