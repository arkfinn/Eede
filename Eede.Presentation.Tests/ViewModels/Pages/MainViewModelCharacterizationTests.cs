using Avalonia.Media.Imaging;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing.History;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Common.Services;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Pages;
using Moq;
using NUnit.Framework;
using System;

namespace Eede.Presentation.Tests.ViewModels.Pages;

[TestFixture]
public class MainViewModelCharacterizationTests
{
    private Mock<GlobalState> _stateMock;
    private Mock<IClipboard> _clipboardServiceMock;
    private Mock<IBitmapAdapter<Bitmap>> _bitmapAdapterMock;
    private Mock<IPictureRepository> _pictureRepositoryMock;
    private Mock<IDrawStyleFactory> _drawStyleFactoryMock;
    private Mock<ITransformImageUseCase> _transformImageUseCaseMock;
    private Mock<ITransferImageToCanvasUseCase> _transferImageToCanvasUseCaseMock;
    private Mock<ITransferImageFromCanvasUseCase> _transferImageFromCanvasUseCaseMock;
    private Mock<IDrawingSessionProvider> _drawingSessionProviderMock;
    private Mock<DrawableCanvasViewModel> _drawableCanvasViewModelMock;
    private Mock<AnimationViewModel> _animationViewModelMock;
    private Mock<DrawingSessionViewModel> _drawingSessionViewModelMock;
    private Mock<PaletteContainerViewModel> _paletteContainerViewModelMock;
    private Mock<SavePictureUseCase> _savePictureUseCaseMock;
    private Mock<LoadPictureUseCase> _loadPictureUseCaseMock;
    private Mock<IServiceProvider> _serviceProviderMock;

    [SetUp]
    public void Setup()
    {
        _stateMock = new Mock<GlobalState>();
        _clipboardServiceMock = new Mock<IClipboard>();
        _bitmapAdapterMock = new Mock<IBitmapAdapter<Bitmap>>();
        _pictureRepositoryMock = new Mock<IPictureRepository>();
        _drawStyleFactoryMock = new Mock<IDrawStyleFactory>();
        _transformImageUseCaseMock = new Mock<ITransformImageUseCase>();
        _transferImageToCanvasUseCaseMock = new Mock<ITransferImageToCanvasUseCase>();
        _transferImageFromCanvasUseCaseMock = new Mock<ITransferImageFromCanvasUseCase>();
        _drawingSessionProviderMock = new Mock<IDrawingSessionProvider>();
        _drawingSessionProviderMock.Setup(x => x.CurrentSession).Returns(new DrawingSession(Picture.CreateEmpty(new PictureSize(1, 1))));
        
        // ViewModels usually need real instances or careful mocking. 
        // For characterization tests, we often use nulls if the constructor allows, 
        // but here we'll assume basic mocks are enough to pass the constructor.
        _drawableCanvasViewModelMock = new Mock<DrawableCanvasViewModel>(
            Mock.Of<GlobalState>(),
            Mock.Of<IAddFrameProvider>(),
            Mock.Of<IClipboard>(),
            Mock.Of<IBitmapAdapter<Bitmap>>(),
            Mock.Of<IDrawingSessionProvider>(),
            new CopySelectionUseCase(Mock.Of<IClipboard>()),
            new CutSelectionUseCase(Mock.Of<IClipboard>()),
            new PasteFromClipboardUseCase(Mock.Of<IClipboard>()),
            Mock.Of<IInteractionCoordinator>()
        );
                var patternsProvider = new AnimationPatternsProvider();
                _animationViewModelMock = new Mock<AnimationViewModel>(
                    patternsProvider,
                    new AddAnimationPatternUseCase(patternsProvider),
                    new ReplaceAnimationPatternUseCase(patternsProvider),
                    new RemoveAnimationPatternUseCase(patternsProvider),
                    Mock.Of<IFileSystem>());
                _drawingSessionViewModelMock = new Mock<DrawingSessionViewModel>(_drawingSessionProviderMock.Object);
        
                _paletteContainerViewModelMock = new Mock<PaletteContainerViewModel>();
        
        
        _savePictureUseCaseMock = new Mock<SavePictureUseCase>(_pictureRepositoryMock.Object);
        _loadPictureUseCaseMock = new Mock<LoadPictureUseCase>(_pictureRepositoryMock.Object);
        _serviceProviderMock = new Mock<IServiceProvider>();
    }

    [Test]
    public void ConstructorTest()
    {
        var vm = new MainViewModel(
            _stateMock.Object,
            _clipboardServiceMock.Object,
            _bitmapAdapterMock.Object,
            _pictureRepositoryMock.Object,
            _drawStyleFactoryMock.Object,
            _transformImageUseCaseMock.Object,
            _transferImageToCanvasUseCaseMock.Object,
            _transferImageFromCanvasUseCaseMock.Object,
            _drawingSessionProviderMock.Object,
            _drawableCanvasViewModelMock.Object,
            _animationViewModelMock.Object,
            _drawingSessionViewModelMock.Object,
            _paletteContainerViewModelMock.Object,
            _savePictureUseCaseMock.Object,
            _loadPictureUseCaseMock.Object,
            _serviceProviderMock.Object
        );

        Assert.That(vm, Is.Not.Null);
    }
}
