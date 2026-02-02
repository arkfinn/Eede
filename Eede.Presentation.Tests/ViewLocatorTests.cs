using NUnit.Framework;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.Pages;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.Views.DataEntry;
using Avalonia.Controls;
using System;
using Avalonia.Headless.NUnit;

namespace Eede.Presentation.Tests;

[TestFixture]
public class ViewLocatorTests
{
    private ViewLocator _viewLocator;

    [SetUp]
    public void SetUp()
    {
        _viewLocator = new ViewLocator();
    }

    [AvaloniaTest]
    public void Match_ShouldReturnTrue_ForViewModelBase()
    {
        var viewModel = new StubViewModel();
        Assert.That(_viewLocator.Match(viewModel), Is.True);
    }

    [AvaloniaTest]
    public void Match_ShouldReturnFalse_ForNonViewModel()
    {
        Assert.That(_viewLocator.Match("Not a ViewModel"), Is.False);
    }

    [AvaloniaTest]
    public void Build_ShouldReturnMainView_ForMainViewModel()
    {
        // 型情報のみが必要なため、インスタンス化の代わりに型情報を模倣する
        var viewModel = (MainViewModel)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(MainViewModel));
        var result = _viewLocator.Build(viewModel);
        Assert.That(result, Is.InstanceOf<MainView>());
    }

    [AvaloniaTest]
    public void Build_ShouldReturnPaletteContainer_ForPaletteContainerViewModel()
    {
        var viewModel = new PaletteContainerViewModel();
        var result = _viewLocator.Build(viewModel);
        Assert.That(result, Is.InstanceOf<PaletteContainer>());
    }

    [AvaloniaTest]
    public void Build_ShouldReturnTextBlock_WhenViewNotFound()
    {
        var viewModel = new StubViewModel();
        var result = _viewLocator.Build(viewModel);
        Assert.That(result, Is.InstanceOf<TextBlock>());
        var textBlock = (TextBlock)result!;
        Assert.That(textBlock.Text, Does.Contain("Not Found"));
    }

    private class StubViewModel : Eede.Presentation.ViewModels.ViewModelBase { }
}
