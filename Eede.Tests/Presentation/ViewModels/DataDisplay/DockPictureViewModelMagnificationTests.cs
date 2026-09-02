using Avalonia.Headless.NUnit;
using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Files;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using ReactiveUI;
using ReactiveUI.Testing;
using System;

namespace Eede.Presentation.Tests.ViewModels.DataDisplay;

public class DockPictureViewModelMagnificationTests
{
    private GlobalState _globalState;
    private AnimationViewModel _animationViewModel;
    private Mock<IPictureRepository> _mockPictureRepository;
    private IPictureFileIO _PictureFileIO;

    [SetUp]
    public void Setup()
    {
        _globalState = new GlobalState();
        var patternsProvider = new AnimationPatternsProvider();
        var patternEditor = new AnimationPatternEditor(
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider));
        _animationViewModel = new AnimationViewModel(
            patternsProvider,
            patternEditor,
            new Mock<IFileSystem>().Object,
            new AvaloniaBitmapAdapter()); _mockPictureRepository = new Mock<IPictureRepository>();
        var mockSettingsRepo = new Mock<ISettingsRepository>();
        mockSettingsRepo.Setup(x => x.LoadAsync()).ReturnsAsync(new AppSettings());
        _PictureFileIO = new PictureFileIO(
            new SavePictureUseCase(_mockPictureRepository.Object, mockSettingsRepo.Object),
            new LoadPictureUseCase(_mockPictureRepository.Object, mockSettingsRepo.Object));
    }

    [AvaloniaTest]
    public void DefaultMagnificationIs1()
    {
        var scheduler = new TestScheduler();
        var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _PictureFileIO);

        Assert.That(viewModel.Magnification.Value, Is.EqualTo(1f));
    }

    [AvaloniaTest]
    public void CanUpdateMagnification()
    {
        var scheduler = new TestScheduler();
        var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _PictureFileIO)
        {
            Magnification = new Magnification(2)
        };

        Assert.That(viewModel.Magnification.Value, Is.EqualTo(2f));
    }

    [AvaloniaTest]
    public void ZoomInFollowsSteps()
    {
        var scheduler = new TestScheduler();
        var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _PictureFileIO);
        // Default is 1
        viewModel.ZoomIn();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(2f));
        viewModel.ZoomIn();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(4f));
        viewModel.ZoomIn();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(6f));
        viewModel.ZoomIn();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(8f));
        viewModel.ZoomIn();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(12f));
        viewModel.ZoomIn();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(12f), "Should stay at max");
    }

    [AvaloniaTest]
    public void ZoomOutFollowsSteps()
    {
        var scheduler = new TestScheduler();
        var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _PictureFileIO);
        viewModel.Magnification = new Magnification(12);

        viewModel.ZoomOut();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(8f));
        viewModel.ZoomOut();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(6f));
        viewModel.ZoomOut();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(4f));
        viewModel.ZoomOut();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(2f));
        viewModel.ZoomOut();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(1f));
        viewModel.ZoomOut();
        Assert.That(viewModel.Magnification.Value, Is.EqualTo(1f), "Should stay at min");
    }

    [AvaloniaTest]
    public void ZoomInCommandIncreasesMagnification()
    {
        var scheduler = new TestScheduler();
        var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _PictureFileIO);

        viewModel.ZoomInCommand.Execute().Subscribe();
        scheduler.AdvanceBy(1);

        Assert.That(viewModel.Magnification.Value, Is.EqualTo(2f));
    }

    [AvaloniaTest]
    public void ZoomOutCommandDecreasesMagnification()
    {
        var scheduler = new TestScheduler();
        var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _PictureFileIO);
        viewModel.Magnification = new Magnification(4);

        viewModel.ZoomOutCommand.Execute().Subscribe();
        scheduler.AdvanceBy(1);

        Assert.That(viewModel.Magnification.Value, Is.EqualTo(2f));
    }

    [AvaloniaTest]
    public void DisplaySizeIsCorrect()
    {
        var scheduler = new TestScheduler();
        var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _PictureFileIO);
        viewModel.Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), new FilePath("test.png"));

        // 4倍時
        viewModel.Magnification = new Magnification(4);
        scheduler.AdvanceBy(1);
        Assert.Multiple(() =>
        {
            Assert.That(viewModel.DisplayWidth, Is.EqualTo(128));
            Assert.That(viewModel.DisplayHeight, Is.EqualTo(128));
        });

        // 画像サイズ変更時
        viewModel.OnPictureUpdate.Execute(Picture.CreateEmpty(new PictureSize(64, 48))).Subscribe();
        scheduler.AdvanceBy(1);
        Assert.Multiple(() =>
        {
            Assert.That(viewModel.DisplayWidth, Is.EqualTo(256), "DisplayWidth should update when picture size changes");
            Assert.That(viewModel.DisplayHeight, Is.EqualTo(192), "DisplayHeight should update when picture size changes");
        });
    }

    [AvaloniaTest]
    public void SetMagnificationCommandUpdatesMagnification()
    {
        var scheduler = new TestScheduler();
        var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _PictureFileIO);

        viewModel.SetMagnificationCommand.Execute(8f).Subscribe();
        scheduler.AdvanceBy(1);

        Assert.That(viewModel.Magnification.Value, Is.EqualTo(8f));
    }
}


