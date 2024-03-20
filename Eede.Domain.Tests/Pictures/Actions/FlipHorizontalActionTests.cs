using NUnit.Framework;

namespace Eede.Domain.Pictures.Actions;

[TestFixture]
internal class FlipHorizontalActionTests
{
    [TestCaseSource(nameof(ExecuteCases))]
    public void TestExecute(byte[] beforeData, byte[] exceptedData)
    {
        var before = Picture.Create(new PictureSize(4, 4), beforeData);
        var excepted = Picture.Create(new PictureSize(4, 4), exceptedData);
        var action = new FlipHorizontalAction(before);
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


