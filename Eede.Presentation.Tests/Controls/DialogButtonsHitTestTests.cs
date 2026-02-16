using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Eede.Presentation.Controls;
using NUnit.Framework;
using System.Windows.Input;
using Moq;
using System.Linq;

namespace Eede.Presentation.Tests.Controls;

[TestFixture]
public class DialogButtonsHitTestTests
{
    [AvaloniaTest]
    public void ClickOnButtonEdge_ShouldExecuteCommand()
    {
        var mockCommand = new Mock<ICommand>();
        mockCommand.Setup(x => x.CanExecute(It.IsAny<object>())).Returns(true);

        var target = new DialogButtons
        {
            PrimaryText = "OK",
            PrimaryCommand = mockCommand.Object,
            Width = 200,
            Height = 100
        };

        // テスト用のウィンドウに配置してレイアウトを確定させる
        var window = new Window { Content = target };
        window.Show();

        // ボタンを探す
        var primaryButton = target.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(b => b.Classes.Contains("primary"));

        Assert.That(primaryButton, Is.Not.Null, "Primary button not found in template.");

        // ボタンの左上隅（文字から離れた場所）の座標を計算
        var pointOnEdge = new Point(2, 2); 
        
        // PointerPressed
        primaryButton.RaiseEvent(new PointerPressedEventArgs(
            primaryButton,
            new Pointer(0, PointerType.Mouse, true),
            primaryButton,
            pointOnEdge,
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));

        // PointerReleased
        primaryButton.RaiseEvent(new PointerReleasedEventArgs(
            primaryButton,
            new Pointer(0, PointerType.Mouse, true),
            primaryButton,
            pointOnEdge,
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None,
            MouseButton.Left));

        // コマンドが実行されたか確認
        mockCommand.Verify(x => x.Execute(It.IsAny<object>()), Times.AtLeastOnce, "Command was not executed when clicking on button edge.");
    }
}
