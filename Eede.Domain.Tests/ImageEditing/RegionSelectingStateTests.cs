using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.ImageEditing;
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
        
            [Test]
            public void HandlePointerMoved_Should_Update_NowPosition()
            {
                var start = new Position(0, 0);
                var state = new RegionSelectingState(start, start, _minSize);
        
                // Initial area (32x32 due to min size and snap)
                var initialArea = state.GetSelectingArea();
                Assert.That(initialArea.Value.Width, Is.EqualTo(32));
        
                        // Move to (100, 100)
                        var cursorArea = HalfBoxArea.Create(start, _minSize);
                        state.HandlePointerMoved(cursorArea, true, new Position(100, 100), false, new PictureSize(200, 200));
                                var movedArea = state.GetSelectingArea();
                Assert.That(movedArea.Value.Width, Is.EqualTo(112)); // Updated!
            }
        
            [Test]
            public void HandlePointerMoved_Should_Support_Square_Selection_With_Shift()
            {
                var start = new Position(100, 100);
                var state = new RegionSelectingState(start, start, _minSize);
        
                        // Move to (150, 120) with Shift
                        // deltaX = 50, deltaY = 20. Max delta = 50.
                        // Target should be (150, 150)
                        var cursorArea = HalfBoxArea.Create(start, _minSize);
                        state.HandlePointerMoved(cursorArea, true, new Position(150, 120), true, new PictureSize(1000, 1000));
                                var area = state.GetSelectingArea();
                // Snap(100, 16) = 96
                // Snap(150 + 16 - 1, 16) = Snap(165, 16) = 160
                // Size = 160 - 96 = 64
                Assert.That(area.Value.Width, Is.EqualTo(64));
                Assert.That(area.Value.Height, Is.EqualTo(64));
            }
        }
        