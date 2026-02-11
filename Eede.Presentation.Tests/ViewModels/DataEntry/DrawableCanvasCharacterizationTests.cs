using Avalonia.Input;
using Avalonia.Media.Imaging;
using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Services;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Pages;
using Eede.Application.Pictures;
using Moq;
using NUnit.Framework;
using System.Reactive;

namespace Eede.Presentation.Tests.ViewModels.DataEntry;

[TestFixture]
public class DrawableCanvasCharacterizationTests
{
    private Mock<GlobalState> _globalStateMock;
    private Mock<IAddFrameProvider> _addFrameProviderMock;
    private Mock<IClipboard> _clipboardServiceMock;
    private Mock<IBitmapAdapter<Bitmap>> _bitmapAdapterMock;
    private Mock<IDrawingSessionProvider> _drawingSessionProviderMock;
    private Mock<IInteractionCoordinator> _coordinatorMock;
    private Mock<DrawingSessionViewModel> _drawingSessionViewModelMock;

    private ISelectionService _selectionService;

    [SetUp]
    public void Setup()
    {
        _globalStateMock = new Mock<GlobalState>();
        _addFrameProviderMock = new Mock<IAddFrameProvider>();
        _clipboardServiceMock = new Mock<IClipboard>();
        _bitmapAdapterMock = new Mock<IBitmapAdapter<Bitmap>>();
        _drawingSessionProviderMock = new Mock<IDrawingSessionProvider>();
        _coordinatorMock = new Mock<IInteractionCoordinator>();
        _drawingSessionViewModelMock = new Mock<DrawingSessionViewModel>();

        _selectionService = new SelectionService(
            new CopySelectionUseCase(_clipboardServiceMock.Object),
            new CutSelectionUseCase(_clipboardServiceMock.Object),
            new PasteFromClipboardUseCase(_clipboardServiceMock.Object, _drawingSessionProviderMock.Object));
    }

    [Test]
    public void ConstructorTest()
    {
        var vm = new DrawableCanvasViewModel(
            _globalStateMock.Object,
            _addFrameProviderMock.Object,
            _clipboardServiceMock.Object,
            _bitmapAdapterMock.Object,
            _drawingSessionProviderMock.Object,
            _selectionService,
            _coordinatorMock.Object);

        Assert.That(vm, Is.Not.Null);
    }
}
