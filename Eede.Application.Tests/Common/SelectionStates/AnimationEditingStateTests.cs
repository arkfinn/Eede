using Eede.Application.Common.SelectionStates;
using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;
using System.Windows.Input;

namespace Eede.Application.Tests.Common.SelectionStates;

[TestFixture]
public class AnimationEditingStateTests
{
    private class MockAddFrameProvider : IAddFrameProvider
    {
        public bool Executed { get; private set; }
        public int Parameter { get; private set; }

        public void AddFrame(int cellIndex)
        {
            Executed = true;
            Parameter = cellIndex;
        }
    }

    [Test]
    public void LeftClickShouldExecuteAddFrameCommand()
    {
        var provider = new MockAddFrameProvider();
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var state = new AnimationEditingState(provider, grid, new PictureSize(64, 64));

        var cursorArea = HalfBoxArea.Create(new Position(16, 16), new PictureSize(16, 16));
        state.HandlePointerLeftButtonPressed(cursorArea, new Position(16, 16), null, null, null);

        Assert.That(provider.Executed, Is.True);
        // 64 / 16 = 4 columns. (16, 16) is col 1, row 1. Index = 1 * 4 + 1 = 5.
        Assert.That(provider.Parameter, Is.EqualTo(5));
    }
}
