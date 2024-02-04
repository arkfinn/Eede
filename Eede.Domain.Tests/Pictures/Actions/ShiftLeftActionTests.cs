using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.Pictures.Actions;

[TestFixture]
internal class ShiftLeftActionTests
{
    [TestCaseSource(nameof(ExecuteCases))]
    public void TestExecute(byte[] beforeData, byte[] exceptedData)
    {
        var before = Picture.Create(new PictureSize(4, 4), beforeData);
        var excepted = Picture.Create(new PictureSize(4, 4), exceptedData);
        var action = new ShiftLeftAction(before);
        var after = action.Execute();

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
                    1,2,3,4,1,2,3,4,1,2,3,4,1,2,3,4,
                    0,1,2,3,0,1,2,3,0,1,2,3,1,2,3,4,
                    0,1,2,3,0,1,2,3,0,1,2,3,1,2,3,4,
                    1,2,3,4,1,2,3,4,1,2,3,4,0,1,2,3,
                }
            }
    };
}
