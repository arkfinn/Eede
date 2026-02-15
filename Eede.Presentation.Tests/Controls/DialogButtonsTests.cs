using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Eede.Presentation.Controls;
using NUnit.Framework;
using System.Windows.Input;
using Moq;

namespace Eede.Presentation.Tests.Controls;

[TestFixture]
public class DialogButtonsTests
{
    [AvaloniaTest]
    public void Properties_ShouldUpdateContent()
    {
        var target = new DialogButtons
        {
            PrimaryText = "Save",
            SecondaryText = "Discard",
            CancelText = "Ignore"
        };

        Assert.That(target.PrimaryText, Is.EqualTo("Save"));
        Assert.That(target.SecondaryText, Is.EqualTo("Discard"));
        Assert.That(target.CancelText, Is.EqualTo("Ignore"));
    }

    [AvaloniaTest]
    public void PrimaryCommand_ShouldBeBound()
    {
        var mockCommand = new Mock<ICommand>();
        var target = new DialogButtons
        {
            PrimaryCommand = mockCommand.Object
        };

        Assert.That(target.PrimaryCommand, Is.EqualTo(mockCommand.Object));
    }
}
