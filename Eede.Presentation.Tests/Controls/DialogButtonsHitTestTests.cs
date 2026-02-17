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
using System;

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
            PrimaryCommand = mockCommand.Object
        };

        // ウィンドウに配置してサイズを確定させる
        var window = new Window { Content = target, Width = 400, Height = 400 };
        window.Show();

        // レイアウト確定待ち
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // ボタンを探す
        var primaryButton = target.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(b => b.Classes.Contains("primary"));

        Assert.That(primaryButton, Is.Not.Null, "Primary button not found in template.");

        // バインディングとコマンドの状態を確認
        Assert.That(primaryButton.Command, Is.EqualTo(mockCommand.Object), "Command should be bound to the button.");
        Assert.That(primaryButton.IsEnabled, Is.True, "Button should be enabled.");

        // ボタンの左上隅（2, 2）の座標をウィンドウ座標に変換
        var pointOnEdge = new Point(2, 2);
        
        // 直接イベントを発行するのではなく、HeadlessWindowExtensions の Mouse メソッド群を使用する
        // 座標は Window に対する相対座標が必要
        var windowPoint = primaryButton.TranslatePoint(pointOnEdge, window);
        Assert.That(windowPoint, Is.Not.Null, "Could not translate point to window coordinates.");

        // ヘッドレステストのシミュレーションを使用してクリック
        // Avalonia.Headless の拡張メソッドを使用
        window.MouseMove(windowPoint.Value);
        window.MouseDown(windowPoint.Value, MouseButton.Left);
        window.MouseUp(windowPoint.Value, MouseButton.Left);

        // UIスレッドのジョブを処理してコマンド実行を待つ
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // コマンドが実行されたか確認
        mockCommand.Verify(x => x.Execute(It.IsAny<object>()), Times.AtLeastOnce, "Command was not executed when clicking on button edge.");
    }
}
