using Eede.Application.Common.SelectionStates;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;
using System.Windows.Input;

namespace Eede.Application.Tests.Common.SelectionStates;

[TestFixture]
public class NormalCursorStateTests
{
    private class MockCommand : ICommand
    {
        public bool Executed { get; private set; }
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => Executed = true;
    }

    [Test]
    public void LeftButtonPressedExecutesPullAction()
    {
        var area = HalfBoxArea.Create(new Position(0, 0), new PictureSize(32, 32));
        var state = new NormalCursorState(area);
        var pullCommand = new MockCommand();
        
        state.HandlePointerLeftButtonPressed(area, new Position(0, 0), pullCommand, () => null, null);
        
        Assert.That(pullCommand.Executed, Is.True, "NormalCursorState should execute pullAction on left click for data transfer.");
    }
}
