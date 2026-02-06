using Avalonia.Headless.NUnit;
using Dock.Model.Core;
using Eede.Presentation;
using Eede.Presentation.Views.DataDisplay;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;

namespace Eede.Presentation.Tests.Views.DataDisplay;

[TestFixture]
public class PictureFrameTests
{
    private ServiceProvider _serviceProvider;
    private InjectableDockFactory _factory;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        _factory = new InjectableDockFactory();
        services.AddSingleton(_factory);
        _serviceProvider = services.BuildServiceProvider();

        // App.Services をテスト用に上書きする（リフレクション等を使わず、Appクラスの構造に合わせる）
        // App.Services は static なので、テストごとにクリアまたは設定が必要
        var property = typeof(App).GetProperty("Services", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        property?.SetValue(null, _serviceProvider);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
        var property = typeof(App).GetProperty("Services", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        property?.SetValue(null, null);
    }

    [AvaloniaTest]
    public void ActiveDockable_ShouldSyncWithFactoryEvent()
    {
        // Arrange
        var pictureFrame = new PictureFrame();
        var mockDockable = new Mock<IDockable>().Object;

        // Act
        // SetFocusedDockable を通じてイベントを発火させる
        _factory.SetFocusedDockable(null, mockDockable);

        // Assert
        Assert.That(pictureFrame.ActiveDockable, Is.SameAs(mockDockable));
    }

    [AvaloniaTest]
    public void ActiveDockable_ShouldNotLoopInfinitely()
    {
        // Arrange
        var pictureFrame = new PictureFrame();
        var mockDockable = new Mock<IDockable>().Object;
        int eventCount = 0;
        _factory.ActiveDockableChanged += (s, e) => eventCount++;

        // Act
        // イベントを発火させて、プロパティが更新されることを確認
        _factory.SetFocusedDockable(null, mockDockable);
        
        // すでに同じ値が入っている状態でもう一度発火させても、
        // ガードによってイベントはそれ以上連鎖しないはず
        _factory.SetFocusedDockable(null, mockDockable);

        // Assert
        // 最初の1回だけイベントがカウントされていることを確認 (InjectableDockFactoryのガード)
        Assert.That(eventCount, Is.EqualTo(1));
        Assert.That(pictureFrame.ActiveDockable, Is.SameAs(mockDockable));
    }
}
