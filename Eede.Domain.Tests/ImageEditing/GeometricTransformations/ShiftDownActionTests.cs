using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.GeometricTransformations;

[TestFixture]
internal class ShiftDownActionTests
{
    [TestCaseSource(nameof(ExecuteCases))]
    public void TestExecute(byte[] beforeData, byte[] exceptedData)
    {
        Picture before = Picture.Create(new PictureSize(4, 4), beforeData);
        Picture excepted = Picture.Create(new PictureSize(4, 4), exceptedData);
        ShiftDownAction action = new(before);
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
                    0,1,2,3,1,2,3,4,1,2,3,4,1,2,3,4,
                    1,2,3,4,1,2,3,4,1,2,3,4,1,2,3,4,
                    1,2,3,4,0,1,2,3,0,1,2,3,0,1,2,3,
                    1,2,3,4,0,1,2,3,0,1,2,3,0,1,2,3,
                }
            }
    };
}
