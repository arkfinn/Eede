using Eede.Application.Common.SelectionStates;
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
    private class MockCommand : ICommand
    {
        public bool Executed { get; private set; }
        public object Parameter { get; private set; }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            Executed = true;
            Parameter = parameter;
        }
    }

    [Test]
    public void LeftClickShouldExecuteAddFrameCommand()
    {
        var addFrameCommand = new MockCommand();
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var state = new AnimationEditingState(addFrameCommand, grid, new PictureSize(64, 64));

        var cursorArea = HalfBoxArea.Create(new Position(16, 16), new PictureSize(16, 16));
        state.HandlePointerLeftButtonPressed(cursorArea, new Position(16, 16), null, null, null);

        Assert.That(addFrameCommand.Executed, Is.True);
        // 64 / 16 = 4 columns. (16, 16) is col 1, row 1. Index = 1 * 4 + 1 = 5.
        Assert.That(addFrameCommand.Parameter, Is.EqualTo(5));
    }
}
