using Eede.Application.Common.SelectionStates;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Application.Tests.Common.SelectionStates;

[TestFixture]
public class RegionSelectingStateTests
{
    [Test]
    public void RightButtonReleasedReturnsResizedArea()
    {
        var startPos = new Position(10, 10);
        var gridSize = new PictureSize(16, 16);
        var startArea = HalfBoxArea.Create(startPos, gridSize);
        var state = new RegionSelectingState(startArea, startArea);
        
        // ドラッグして広げる (10,10 から 40,40 へ)
        var magnifiedSize = new PictureSize(100, 100);
        state.HandlePointerMoved(startArea, true, new Position(40, 40), magnifiedSize);
        
        // 確定
        var (nextState, finalArea) = state.HandlePointerRightButtonReleased(startArea, null);
        
        Assert.Multiple(() =>
        {
            Assert.That(nextState, Is.TypeOf<SelectedState>());
            // 16x16グリッドなので、40,40付近まで広がっていることを確認
            Assert.That(finalArea.BoxSize.Width, Is.GreaterThan(gridSize.Width));
            Assert.That(finalArea.BoxSize.Height, Is.GreaterThan(gridSize.Height));
            
            var selected = nextState as SelectedState;
            Assert.That(selected.Selection.Area.Width, Is.EqualTo(finalArea.BoxSize.Width));
        });
    }
}
