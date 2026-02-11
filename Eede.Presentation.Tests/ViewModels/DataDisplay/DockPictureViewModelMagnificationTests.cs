using Avalonia.Headless.NUnit;
using Eede.Application.Animations;
using Eede.Application.Infrastructure;
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
    private IPictureIOService _pictureIOService;

    [SetUp]
    public void Setup()
    {
        _globalState = new GlobalState();
        var patternsProvider = new AnimationPatternsProvider();
        var patternService = new AnimationPatternService(
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider));
        _animationViewModel = new AnimationViewModel(
            patternsProvider,
            patternService,
            new Mock<IFileSystem>().Object,
            new AvaloniaBitmapAdapter()); _mockPictureRepository = new Mock<IPictureRepository>();
        _pictureIOService = new PictureIOService(
            new SavePictureUseCase(_mockPictureRepository.Object),
            new LoadPictureUseCase(_mockPictureRepository.Object));
    }

    [AvaloniaTest]
    public void DefaultMagnificationIs1()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _pictureIOService);

            Assert.That(viewModel.Magnification.Value, Is.EqualTo(1f));
        });
    }

    [AvaloniaTest]
    public void CanUpdateMagnification()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _pictureIOService);

            viewModel.Magnification = new Magnification(2);

            Assert.That(viewModel.Magnification.Value, Is.EqualTo(2f));
        });
    }

    [AvaloniaTest]
    public void ZoomInFollowsSteps()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _pictureIOService);
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
        });
    }

    [AvaloniaTest]
    public void ZoomOutFollowsSteps()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _pictureIOService);
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
        });
    }

    [AvaloniaTest]
    public void ZoomInCommandIncreasesMagnification()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _pictureIOService);

            viewModel.ZoomInCommand.Execute().Subscribe();
            scheduler.AdvanceBy(1);

            Assert.That(viewModel.Magnification.Value, Is.EqualTo(2f));
        });
    }

    [AvaloniaTest]
    public void ZoomOutCommandDecreasesMagnification()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _pictureIOService);
            viewModel.Magnification = new Magnification(4);

            viewModel.ZoomOutCommand.Execute().Subscribe();
            scheduler.AdvanceBy(1);

            Assert.That(viewModel.Magnification.Value, Is.EqualTo(2f));
        });
    }

    [AvaloniaTest]
    public void DisplaySizeIsCorrect()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _pictureIOService);
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
        });
    }

    [AvaloniaTest]
    public void SetMagnificationCommandUpdatesMagnification()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _pictureIOService);

            viewModel.SetMagnificationCommand.Execute(8f).Subscribe();
            scheduler.AdvanceBy(1);

            Assert.That(viewModel.Magnification.Value, Is.EqualTo(8f));
        });
    }
}
