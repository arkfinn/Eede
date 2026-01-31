using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Files;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Application.UseCase.Animations;
using Eede.Presentation.Common.Services;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Common.Adapters;
using Moq;
using NUnit.Framework;
using Avalonia.Headless.NUnit;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using ReactiveUI;
using Avalonia.Media.Imaging;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Eede.Presentation.Tests.ViewModels.DataDisplay;

public class DockPictureViewModelTests
{
    private GlobalState _globalState;
    private AnimationViewModel _animationViewModel;
    private Mock<IPictureRepository> _mockPictureRepository;
    private SavePictureUseCase _savePictureUseCase;
    private LoadPictureUseCase _loadPictureUseCase;

    [SetUp]
    public void Setup()
    {
        _globalState = new GlobalState();
        var patternsProvider = new AnimationPatternsProvider();
        _animationViewModel = new AnimationViewModel(
            patternsProvider,
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider),
            new Mock<IFileSystem>().Object);
        _mockPictureRepository = new Mock<IPictureRepository>();
        _savePictureUseCase = new SavePictureUseCase(_mockPictureRepository.Object);
        _loadPictureUseCase = new LoadPictureUseCase(_mockPictureRepository.Object);
    }

    [AvaloniaTest]
    public void Characterization_Initialize()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _savePictureUseCase, _loadPictureUseCase);

            var size = new PictureSize(32, 32);
            var picture = Picture.CreateEmpty(size);
            var path = new FilePath("TestImage.png");

            viewModel.Initialize(picture, path);

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.Subject, Is.EqualTo("TestImage.png"));
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
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _savePictureUseCase, _loadPictureUseCase);

            var initialSize = new PictureSize(32, 32);
            viewModel.Initialize(Picture.CreateEmpty(initialSize), new FilePath("test.png"));

            Assert.That(viewModel.Edited, Is.False);

            var updatedPicture = Picture.CreateEmpty(new PictureSize(32, 32));
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

    [AvaloniaTest]
    public void Characterization_Save()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, new AvaloniaBitmapAdapter(), _savePictureUseCase, _loadPictureUseCase);

            var mockFile = new Mock<IImageFile>();
            var bitmap = new WriteableBitmap(new Avalonia.PixelSize(32, 32), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Premul);
            mockFile.Setup(f => f.Bitmap).Returns(bitmap);
            mockFile.Setup(f => f.Path).Returns(new FilePath("test.png"));
            mockFile.Setup(f => f.WithBitmap(It.IsAny<Bitmap>())).Returns(mockFile.Object);

            viewModel.Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), new FilePath("test.png"));
            viewModel.Edited = true;

            bool saveEventCalled = false;
            viewModel.PictureSave += async (sender, args) =>
            {
                saveEventCalled = true;
                await Task.CompletedTask;
            };

            viewModel.Save().Wait();

            Assert.That(saveEventCalled, Is.True);
            Assert.That(viewModel.Edited, Is.False, "Save successful should reset Edited flag");
        });
    }
}