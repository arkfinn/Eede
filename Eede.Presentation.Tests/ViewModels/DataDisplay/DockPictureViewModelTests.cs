using Eede.Application.Animations;
using Eede.Application.Services;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Files;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.Common.Services;
using Moq;
using NUnit.Framework;
using Avalonia.Headless.NUnit;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using ReactiveUI;
using Avalonia.Media.Imaging;
using System.IO;

namespace Eede.Presentation.Tests.ViewModels.DataDisplay;

public class DockPictureViewModelTests
{
    private GlobalState _globalState;
    private Mock<IAnimationService> _mockAnimationService;
    private Mock<IFileSystem> _mockFileSystem;
    private AnimationViewModel _animationViewModel;

    [SetUp]
    public void Setup()
    {
        _globalState = new GlobalState();
        _mockAnimationService = new Mock<IAnimationService>();
        _mockAnimationService.Setup(s => s.Patterns).Returns(new System.Collections.Generic.List<AnimationPattern>());
        _mockFileSystem = new Mock<IFileSystem>();
        _animationViewModel = new AnimationViewModel(_mockAnimationService.Object, _mockFileSystem.Object);
    }

    [AvaloniaTest]
    public void Characterization_Initialize()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel);
            
            var mockFile = new Mock<IImageFile>();
            // 32x32の空のビットマップを作成
            var bitmap = new WriteableBitmap(new Avalonia.PixelSize(32, 32), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Premul);
            mockFile.Setup(f => f.Bitmap).Returns(bitmap);
            mockFile.Setup(f => f.Subject()).Returns("TestImage");

            viewModel.Initialize(mockFile.Object);

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.Subject, Is.EqualTo("TestImage"));
                Assert.That(viewModel.Edited, Is.False);
                Assert.That(viewModel.PictureBuffer.Size.Width, Is.EqualTo(32));
                Assert.That(viewModel.PictureBuffer.Size.Height, Is.EqualTo(32));
            });
        });
    }

    [AvaloniaTest]
    public void Characterization_PictureUpdate()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel);
            
            var initialPicture = Picture.CreateEmpty(new PictureSize(32, 32));
            var mockFile = new Mock<IImageFile>();
            var bitmap = new WriteableBitmap(new Avalonia.PixelSize(32, 32), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Premul);
            mockFile.Setup(f => f.Bitmap).Returns(bitmap);
            viewModel.Initialize(mockFile.Object);

            Assert.That(viewModel.Edited, Is.False);

            var updatedPicture = Picture.CreateEmpty(new PictureSize(32, 32));
            // 本来は何か描画されているはずだが、ここでは空で。
            viewModel.OnPictureUpdate.Execute(updatedPicture).Subscribe();
            scheduler.AdvanceBy(1);

            Assert.That(viewModel.Edited, Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel.PictureBuffer.Size, Is.EqualTo(updatedPicture.Size));
                Assert.That(viewModel.PictureBuffer.Length, Is.EqualTo(updatedPicture.Length));
            });
        });
    }
}
