using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.SharedKernel
{
    [TestFixture]
    public class PictureAreaTests
    {
        [TestCaseSource(nameof(FromPositionCases))]
        public void FromPositionTest(Position from, Position to, PictureSize limit, PictureArea expected)
        {
            PictureArea result = PictureArea.FromPosition(from, to, limit);
            Assert.That((result.X, result.Y, result.Width, result.Height), Is.EqualTo((expected.X, expected.Y, expected.Width, expected.Height)));
        }

        private static readonly object[] FromPositionCases =
        {
            new object[] { new Position(10, 20), new Position(100, 200), new PictureSize(300, 400),
                new PictureArea(new Position(10, 20), new PictureSize(90, 180)) },
            new object[] { new Position(10, 20), new Position(0, 10), new PictureSize(300, 400),
                new PictureArea(new Position(0, 10), new PictureSize(10, 10)) },
             new object[] { new Position(0, 0), new Position(500, 500), new PictureSize(300, 400),
                new PictureArea(new Position(0, 0), new PictureSize(300, 400)) },
            new object[] { new Position(1, 1), new Position(500, 500), new PictureSize(300, 400),
                new PictureArea(new Position(1, 1), new PictureSize(299, 399)) },
            // Reproduction case for ArgumentException
            new object[] { new Position(110, 0), new Position(120, 10), new PictureSize(100, 100),
                new PictureArea(new Position(100, 0), new PictureSize(0, 10)) },
        };

        [TestCaseSource(nameof(CombineCases))]
        public void CombineTest(PictureArea area1, PictureArea area2, PictureArea expected)
        {
            PictureArea result = area1.Combine(area2);
            Assert.That((result.X, result.Y, result.Width, result.Height), Is.EqualTo((expected.X, expected.Y, expected.Width, expected.Height)));
        }

        private static readonly object[] CombineCases =
        {
            // Non-overlapping
            new object[] {
                new PictureArea(new Position(0, 0), new PictureSize(10, 10)),
                new PictureArea(new Position(20, 20), new PictureSize(10, 10)),
                new PictureArea(new Position(0, 0), new PictureSize(30, 30))
            },
            // Partially overlapping
            new object[] {
                new PictureArea(new Position(10, 10), new PictureSize(20, 20)),
                new PictureArea(new Position(20, 20), new PictureSize(20, 20)),
                new PictureArea(new Position(10, 10), new PictureSize(30, 30))
            },
            // Nested areas
            new object[] {
                new PictureArea(new Position(10, 10), new PictureSize(50, 50)),
                new PictureArea(new Position(20, 20), new PictureSize(10, 10)),
                new PictureArea(new Position(10, 10), new PictureSize(50, 50))
            },
            // Empty area 1
            new object[] {
                new PictureArea(new Position(10, 10), new PictureSize(0, 0)),
                new PictureArea(new Position(20, 20), new PictureSize(20, 20)),
                new PictureArea(new Position(20, 20), new PictureSize(20, 20))
            },
            // Empty area 2
            new object[] {
                new PictureArea(new Position(10, 10), new PictureSize(20, 20)),
                new PictureArea(new Position(20, 20), new PictureSize(0, 0)),
                new PictureArea(new Position(10, 10), new PictureSize(20, 20))
            },
            // Both empty
            new object[] {
                new PictureArea(new Position(10, 10), new PictureSize(0, 0)),
                new PictureArea(new Position(20, 20), new PictureSize(0, 0)),
                new PictureArea(new Position(20, 20), new PictureSize(0, 0))
            }
        };
    }
}