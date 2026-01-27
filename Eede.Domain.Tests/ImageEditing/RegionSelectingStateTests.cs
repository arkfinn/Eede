using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing;

[TestFixture]
public class RegionSelectingStateTests
{
    private readonly PictureSize _minSize = new(32, 32);

    [Test]
    public void GetSelectingArea_Should_Return_Valid_Area_Even_If_Drag_Is_Small()
    {
        var start = new Position(10, 10);
        var now = new Position(13, 13);
        var state = new RegionSelectingState(start, now, _minSize);

        var area = state.GetSelectingArea();

        Assert.That(area, Is.Not.Null, "遊びの判定を削除したため、わずかなドラッグでも範囲選択が有効になるべきです。");
        Assert.That(area.Value.Width, Is.EqualTo(32)); // 最小サイズが適用される
        Assert.That(area.Value.Height, Is.EqualTo(32));
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
    public void GetSelectingArea_Should_Return_Large_Area_For_Large_Drag()
    {
        var start = new Position(0, 0);
        var now = new Position(100, 100);
        var state = new RegionSelectingState(start, now, _minSize);

        var area = state.GetSelectingArea();

        Assert.That(area, Is.Not.Null);
        // GridSize = 16
        // Left = Snap(0, 16) = 0
        // Top = Snap(0, 16) = 0
        // Right = Snap(0 + 100 + 16 - 1, 16) = Snap(115, 16) = 112
        // Bottom = Snap(0 + 100 + 16 - 1, 16) = Snap(115, 16) = 112
        // Width = 112, Height = 112
        Assert.That(area.Value.Width, Is.EqualTo(112));
        Assert.That(area.Value.Height, Is.EqualTo(112));
    }
}