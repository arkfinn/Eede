using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing;

[TestFixture]
public class RegionSelectingStateTests
{
    private readonly PictureSize _minSize = new(32, 32);

    [Test]
    public void GetSelectingArea_Should_Return_Null_When_Drag_Is_Too_Small()
    {
        var start = new Position(10, 10);
        var now = new Position(13, 13);
        var state = new RegionSelectingState(start, now, _minSize);

        var area = state.GetSelectingArea();

        Assert.That(area, Is.Null, "遊びの範囲内のドラッグは範囲選択とみなすべきではありません。");
    }

    [Test]
    public void GetSelectingArea_Should_Snap_To_Grid()
    {
        var start = new Position(10, 10);
        var now = new Position(50, 50);
        var state = new RegionSelectingState(start, now, _minSize);

        var area = state.GetSelectingArea();

        Assert.That(area, Is.Not.Null);
        // snap(10, 16) = 0
        // snap(10+40+16-1, 16) = 64
        Assert.That(area.Value.X, Is.EqualTo(0));
        Assert.That(area.Value.Y, Is.EqualTo(0));
        Assert.That(area.Value.Width, Is.EqualTo(64));
        Assert.That(area.Value.Height, Is.EqualTo(64));
    }

    [Test]
    public void GetSelectingArea_Should_Ensure_Minimum_Size_Even_After_Snap()
    {
        var start = new Position(10, 10);
        var now = new Position(15, 15);
        var state = new RegionSelectingState(start, now, _minSize);

        var area = state.GetSelectingArea();

        Assert.That(area, Is.Not.Null);
        Assert.That(area.Value.Width, Is.EqualTo(32));
        Assert.That(area.Value.Height, Is.EqualTo(32));
    }
}