using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.Selections;

[TestFixture]
public class SelectionTests
{
    [Test]
    public void ContainsTest()
    {
        var area = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
        var selection = new Selection(area);

        Assert.Multiple(() =>
        {
            Assert.That(selection.Contains(new Position(10, 10)), Is.True, "Top-left should be contained");
            Assert.That(selection.Contains(new Position(29, 29)), Is.True, "Inside should be contained");
            Assert.That(selection.Contains(new Position(30, 30)), Is.False, "Outside (bottom-right) should not be contained");
            Assert.That(selection.Contains(new Position(9, 10)), Is.False, "Left should not be contained");
        });
    }

    [Test]
    public void MoveTest()
    {
        var area = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
        var selection = new Selection(area);
        var moved = selection.Move(5, -3);

        Assert.Multiple(() =>
        {
            Assert.That(moved.Area.X, Is.EqualTo(15));
            Assert.That(moved.Area.Y, Is.EqualTo(7));
            Assert.That(moved.Area.Width, Is.EqualTo(20));
            Assert.That(moved.Area.Height, Is.EqualTo(20));
        });
    }
}
