using Eede.Domain.Pictures;
using Eede.Domain.Pictures.Actions;
using Eede.Domain.Sizes;
using NUnit.Framework;

namespace Eede.Domain.Tests.Pictures.Actions
{
    [TestFixture]
    internal class ShiftUpActionTests
    {
        [TestCaseSource(nameof(ExecuteCases))]
        public void TestExecute(byte[] beforeData, byte[] exceptedData)
        {
            Picture before = Picture.Create(new PictureSize(4, 4), beforeData);
            Picture excepted = Picture.Create(new PictureSize(4, 4), exceptedData);
            ShiftUpAction action = new(before);
            Picture after = action.Execute();

            Assert.That(after.CloneImage(), Is.EqualTo(excepted.CloneImage()));

        }

        private static readonly object[] ExecuteCases =
        {
            new object[]
            {
                new byte[] {
                    1,2,3,4,1,2,3,4,1,2,3,4,1,2,3,4,
                    1,2,3,4,0,1,2,3,0,1,2,3,0,1,2,3,
                    1,2,3,4,0,1,2,3,0,1,2,3,0,1,2,3,
                    0,1,2,3,1,2,3,4,1,2,3,4,1,2,3,4,
                },
                new byte[] {
                    1,2,3,4,0,1,2,3,0,1,2,3,0,1,2,3,
                    1,2,3,4,0,1,2,3,0,1,2,3,0,1,2,3,
                    0,1,2,3,1,2,3,4,1,2,3,4,1,2,3,4,
                    1,2,3,4,1,2,3,4,1,2,3,4,1,2,3,4,
                }
            },
            new object[]
            {
                new byte[] {
                    0,1,2,3,0,1,2,3,0,1,2,3,1,2,3,4,
                    0,1,2,3,0,1,2,3,0,1,2,3,1,2,3,4,
                    1,2,3,4,1,2,3,4,1,2,3,4,0,1,2,3,
                    1,2,3,4,1,2,3,4,1,2,3,4,1,2,3,4,
                },
                new byte[] {
                    0,1,2,3,0,1,2,3,0,1,2,3,1,2,3,4,
                    1,2,3,4,1,2,3,4,1,2,3,4,0,1,2,3,
                    1,2,3,4,1,2,3,4,1,2,3,4,1,2,3,4,
                    0,1,2,3,0,1,2,3,0,1,2,3,1,2,3,4,
                }
            },
            new object[]
            {
                new byte[] {
                     0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11,12,13,14,15,
                    16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,
                    32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,
                    48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,
                },
                new byte[] {
                    16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,
                    32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,
                    48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,
                     0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11,12,13,14,15,
                }
            }
        };
    }
}
