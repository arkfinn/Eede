using Eede.Application.Common.SelectionStates;
using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;
using System.Windows.Input;

namespace Eede.Application.Tests.Common.SelectionStates;

[TestFixture]
public class SelectedStateTests
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
    public void LeftButtonPressedInsideSelectionTransitionsToDraggingState()
    {
        var area = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
        var selection = new Selection(area);
        var state = new SelectedState(selection);
        
        var cursorArea = HalfBoxArea.Create(new Position(0, 0), new PictureSize(32, 32));
        var mousePosition = new Position(15, 15); // 範囲内
        var picture = Picture.CreateEmpty(new PictureSize(100, 100));
        var pullCommand = new MockCommand();
        var updateCommand = new MockCommand();
        
        var nextState = state.HandlePointerLeftButtonPressed(cursorArea, mousePosition, pullCommand, () => picture, updateCommand);
        
        Assert.Multiple(() =>
        {
            Assert.That(nextState, Is.TypeOf<DraggingState>());
            Assert.That(pullCommand.Executed, Is.False, "Should NOT execute pull command when dragging starts");
            Assert.That(updateCommand.Executed, Is.True, "Should execute update command to clear original pixels");
        });
    }

    [Test]
    public void LeftButtonPressedOutsideSelectionTransitionsToNormalCursorState()
    {
        var area = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
        var selection = new Selection(area);
        var state = new SelectedState(selection);
        
        var cursorArea = HalfBoxArea.Create(new Position(0, 0), new PictureSize(32, 32));
        var mousePosition = new Position(5, 5); // 範囲外
        var pullCommand = new MockCommand();
        
        var nextState = state.HandlePointerLeftButtonPressed(cursorArea, mousePosition, pullCommand, null, null);
        
        Assert.Multiple(() =>
        {
            Assert.That(nextState, Is.TypeOf<NormalCursorState>(), "Should transition back to NormalCursorState when clicking outside");
            Assert.That(pullCommand.Executed, Is.True, "Should execute existing pull command when clicking outside");
        });
    }
}
