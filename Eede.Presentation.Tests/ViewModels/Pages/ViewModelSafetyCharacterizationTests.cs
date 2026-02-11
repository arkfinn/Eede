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
using Eede.Domain.ImageEditing.Transformation;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Pages;
using Moq;
using NUnit.Framework;
using System;
using Avalonia.Headless.NUnit;

namespace Eede.Presentation.Tests.ViewModels.Pages;

[TestFixture]
public class ViewModelSafetyCharacterizationTests
{
    private MainViewModel _vm;

    [SetUp]
    public void SetUp()
    {
        var stateMock = new Mock<GlobalState>();
        var clipboardServiceMock = new Mock<IClipboard>();
        var bitmapAdapterMock = new Mock<IBitmapAdapter<Bitmap>>();
        var pictureRepositoryMock = new Mock<IPictureRepository>();
        var drawStyleFactoryMock = new Mock<IDrawStyleFactory>();
        var transformImageUseCaseMock = new Mock<ITransformImageUseCase>();
        var transferImageToCanvasUseCaseMock = new Mock<ITransferImageToCanvasUseCase>();
        var transferImageFromCanvasUseCaseMock = new Mock<ITransferImageFromCanvasUseCase>();
        var drawingSessionProviderMock = new Mock<IDrawingSessionProvider>();
        drawingSessionProviderMock.Setup(x => x.CurrentSession).Returns(new DrawingSession(Picture.CreateEmpty(new PictureSize(1, 1))));

        var copyUseCase = new CopySelectionUseCase(clipboardServiceMock.Object);
        var cutUseCase = new CutSelectionUseCase(clipboardServiceMock.Object);
        var pasteUseCase = new PasteFromClipboardUseCase(clipboardServiceMock.Object, drawingSessionProviderMock.Object);
        var selectionService = new SelectionService(copyUseCase, cutUseCase, pasteUseCase);

        var drawableCanvasViewModelMock = new Mock<DrawableCanvasViewModel>(
            stateMock.Object,
            Mock.Of<IAddFrameProvider>(),
            clipboardServiceMock.Object,
            bitmapAdapterMock.Object,
            drawingSessionProviderMock.Object,
            selectionService,
            Mock.Of<IInteractionCoordinator>()
        );
        drawableCanvasViewModelMock.SetupAllProperties();
        drawableCanvasViewModelMock.Object.Magnification = new Magnification(4.0f);

        var patternsProvider = new AnimationPatternsProvider();
        var patternService = new AnimationPatternService(
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider));
        var animationViewModelMock = new Mock<AnimationViewModel>(
            patternsProvider,
            patternService,
            Mock.Of<IFileSystem>(),
            bitmapAdapterMock.Object);

        var drawingSessionViewModelMock = new Mock<DrawingSessionViewModel>(drawingSessionProviderMock.Object);
        var paletteContainerViewModelMock = new Mock<PaletteContainerViewModel>();

        var pictureIOService = new PictureIOService(
            new SavePictureUseCase(pictureRepositoryMock.Object),
            new LoadPictureUseCase(pictureRepositoryMock.Object));

        Func<DockPictureViewModel> dockPictureFactory = () => new DockPictureViewModel(stateMock.Object, animationViewModelMock.Object, bitmapAdapterMock.Object, pictureIOService);
        Func<NewPictureWindowViewModel> newPictureWindowFactory = () => new Mock<NewPictureWindowViewModel>().Object;

        _vm = new MainViewModel(
            stateMock.Object,
            clipboardServiceMock.Object,
            bitmapAdapterMock.Object,
            pictureRepositoryMock.Object,
            drawStyleFactoryMock.Object,
            transformImageUseCaseMock.Object,
            new Mock<IScalingImageUseCase>().Object,
            transferImageToCanvasUseCaseMock.Object,
            transferImageFromCanvasUseCaseMock.Object,
            drawingSessionProviderMock.Object,
            drawableCanvasViewModelMock.Object,
            animationViewModelMock.Object,
            drawingSessionViewModelMock.Object,
            paletteContainerViewModelMock.Object,
            pictureIOService,
            dockPictureFactory,
            newPictureWindowFactory
        );
    }

    [AvaloniaTest]
    public void VerifyInitialStates()
    {
        Assert.Multiple(() =>
        {
            // Non-Null properties
            Assert.That(_vm.DrawableCanvasViewModel, Is.Not.Null);
            Assert.That(_vm.AnimationViewModel, Is.Not.Null);
            Assert.That(_vm.DrawingSessionViewModel, Is.Not.Null);
            Assert.That(_vm.Pictures, Is.Not.Null);

            // Verified Non-Null default values
            Assert.That(_vm.ImageBlender, Is.Not.Null, "ImageBlender should have a default blender");
            Assert.That(_vm.ImageTransfer, Is.Not.Null, "ImageTransfer should have a default transfer");
            Assert.That(_vm.PullBlender, Is.Not.Null, "PullBlender should have a default blender");

            // Intentionally Null properties (should become T? in Nullable enable)
            Assert.That(_vm.FileStorage, Is.Null, "FileStorage is Null by default");
            Assert.That(_vm.ActiveDockable, Is.Null, "ActiveDockable is Null by default");
            Assert.That(_vm.AnimationCursor, Is.Null, "AnimationCursor is Null by default");

            // Important value states
            Assert.That(_vm.Magnification.Value, Is.EqualTo(4.0f));
            Assert.That(_vm.DrawStyle, Is.EqualTo(DrawStyleType.FreeCurve));
        });
    }
}
