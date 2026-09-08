using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System.Collections.Generic;

namespace Eede.Domain.Tests.ImageEditing.SelectionStates;

[TestFixture]
public class AnimationEditingStateTests
{
    private class SpyAddFrameProvider : IAddFrameProvider
    {
        public List<int> AddedFrames { get; } = new();

        public void AddFrame(int index)
        {
            AddedFrames.Add(index);
        }
    }

    [Test]
    public void HandlePointerLeftButtonPressed_CallsAddFrame_WhenInsideGrid()
    {
        var spy = new SpyAddFrameProvider();
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var imageSize = new PictureSize(64, 64);
        var state = new AnimationEditingState(spy, grid, imageSize);

        // Position (0, 0) corresponds to cell index 0
        var cursorArea = HalfBoxArea.Create(new Position(0, 0), new PictureSize(16, 16));
        var nextState = state.HandlePointerLeftButtonPressed(cursorArea, new Position(0, 0), null, () => null!, null);

        Assert.That(nextState, Is.SameAs(state));
        Assert.That(spy.AddedFrames, Has.Count.EqualTo(1));
        Assert.That(spy.AddedFrames[0], Is.EqualTo(0));
    }

    [TestCase(-10, -10)]
    [TestCase(-10, 0)]
    [TestCase(0, -10)]
    [TestCase(100, 100)]
    public void HandlePointerLeftButtonPressed_DoesNotAddFrame_WhenOutsideGrid(int posX, int posY)
    {
        var spy = new SpyAddFrameProvider();
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var imageSize = new PictureSize(64, 64);
        var state = new AnimationEditingState(spy, grid, imageSize);

        var cursorArea = HalfBoxArea.Create(new Position(posX, posY), new PictureSize(16, 16));
        state.HandlePointerLeftButtonPressed(cursorArea, new Position(posX, posY), null, () => null!, null);

        Assert.That(spy.AddedFrames, Is.Empty);
    }

    [Test]
    public void GetCursor_ReturnsDefault()
    {
        var spy = new SpyAddFrameProvider();
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var imageSize = new PictureSize(64, 64);
        var state = new AnimationEditingState(spy, grid, imageSize);

        Assert.That(state.GetCursor(new Position(0, 0)), Is.EqualTo(SelectionCursor.Default));
    }

    [Test]
    public void GetSelectingArea_ReturnsNull()
    {
        var spy = new SpyAddFrameProvider();
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var imageSize = new PictureSize(64, 64);
        var state = new AnimationEditingState(spy, grid, imageSize);

        Assert.That(state.GetSelectingArea(), Is.Null);
    }
}
