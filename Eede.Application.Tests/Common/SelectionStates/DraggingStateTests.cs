using Eede.Application.Common.SelectionStates;
using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;
using System.Windows.Input;

namespace Eede.Application.Tests.Common.SelectionStates;

[TestFixture]
public class DraggingStateTests
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
    public void PointerMovedUpdatesPreviewPosition()
    {
        var originalArea = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
        var selection = new Selection(originalArea);
        var picture = Picture.CreateEmpty(originalArea.Size);
        var content = new SelectionContent(picture, selection);
        var originalPicture = Picture.CreateEmpty(new PictureSize(100, 100));
        
        var state = new DraggingState(content, new Position(10, 10), originalPicture);
        var cursorArea = HalfBoxArea.Create(new Position(10, 10), new PictureSize(32, 32));
        
        // 移動
        state.HandlePointerMoved(cursorArea, true, new Position(15, 12), new PictureSize(100, 100));
        
        var previewInfo = state.GetSelectionPreviewInfo();
        
        Assert.Multiple(() =>
        {
            // deltaX = 15 - 10 = 5, deltaY = 12 - 10 = 2
            // newX = 10 + 5 = 15, newY = 10 + 2 = 12
            Assert.That(previewInfo.Position.X, Is.EqualTo(15));
            Assert.That(previewInfo.Position.Y, Is.EqualTo(12));
        });
    }

    [Test]
    public void HandlePointerLeftButtonReleasedUsesFinalMousePosition()
    {
        var originalArea = new PictureArea(new Position(0, 0), new PictureSize(10, 10));
        var content = new SelectionContent(Picture.CreateEmpty(originalArea.Size), new Selection(originalArea));
        var originalPicture = Picture.CreateEmpty(new PictureSize(100, 100));
        var state = new DraggingState(content, new Position(0, 0), originalPicture);

        var updateCommand = new MockCommand();
        // MouseUp 時の座標を指定
        var finalPos = new Position(25, 35);
        var nextState = state.HandlePointerLeftButtonReleased(null, finalPos, null, updateCommand);

        var selected = nextState as SelectedState;
        Assert.That(selected.Selection.Area.Position, Is.EqualTo(finalPos), "Final position should match the mouse position at release.");
    }

    [Test]
    public void RightButtonPressedCancelsDrag()
    {
        var originalArea = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
        var selection = new Selection(originalArea);
        var picture = Picture.CreateEmpty(originalArea.Size);
        var content = new SelectionContent(picture, selection);
        var originalPicture = Picture.CreateEmpty(new PictureSize(100, 100));
        
        var state = new DraggingState(content, new Position(10, 10), originalPicture);
        var updateCommand = new MockCommand();
        
        var (nextState, _) = state.HandlePointerRightButtonPressed(null, new Position(0, 0), new PictureSize(32, 32), updateCommand);
        
        Assert.Multiple(() =>
        {
            Assert.That(nextState, Is.TypeOf<SelectedState>(), "Should transition back to SelectedState");
            Assert.That(updateCommand.Executed, Is.True, "Should execute update command to restore original pixels");
            Assert.That(updateCommand.Parameter, Is.SameAs(originalPicture), "Should pass original picture");
        });
    }

    [Test]
    public void LeftButtonReleasedCommitsMoveWithCorrectPosition()
    {
        var originalArea = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
        var selection = new Selection(originalArea);
        var selectionPicture = Picture.CreateEmpty(originalArea.Size);
        var content = new SelectionContent(selectionPicture, selection);
        var originalPicture = Picture.CreateEmpty(new PictureSize(100, 100));
        
        var state = new DraggingState(content, new Position(10, 10), originalPicture);
        
        // 移動
        state.HandlePointerMoved(HalfBoxArea.Create(new Position(10, 10), new PictureSize(32, 32)), true, new Position(30, 40), new PictureSize(100, 100));
        
        var pushCommand = new MockCommand();
        var updateCommand = new MockCommand();
        
        var nextState = state.HandlePointerLeftButtonReleased(null, new Position(30, 40), pushCommand, updateCommand);
        
        Assert.Multiple(() =>
        {
            Assert.That(nextState, Is.TypeOf<SelectedState>());
            var selected = nextState as SelectedState;
            // deltaX = 30 - 10 = 20, deltaY = 40 - 10 = 30
            // newX = 10 + 20 = 30, newY = 10 + 30 = 40
            Assert.That(selected.Selection.Area.X, Is.EqualTo(30));
            Assert.That(selected.Selection.Area.Y, Is.EqualTo(40));
            Assert.That(updateCommand.Executed, Is.True);
        });
    }
}
