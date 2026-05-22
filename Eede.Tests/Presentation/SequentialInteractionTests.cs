using Avalonia;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Services;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Application.Infrastructure;
using Eede.Presentation.Settings;
using Eede.Application.Animations;
using Eede.Domain.Animations;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using System;

namespace Eede.Presentation.Tests
{
    [TestFixture]
    public class SequentialInteractionTests
    {
        private DrawingSessionProvider _sessionProvider;
        private InteractionCoordinator _coordinator;
        private Mock<IClipboard> _clipboardMock;
        private ISelectionService _selectionService;
        private DrawableCanvasViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _sessionProvider = new DrawingSessionProvider();
            _sessionProvider.Update(new DrawingSession(Picture.CreateEmpty(new PictureSize(100, 100))));
            _coordinator = new InteractionCoordinator(_sessionProvider);
            _clipboardMock = new Mock<IClipboard>();
            _selectionService = new SelectionService(
                new CopySelectionUseCase(_clipboardMock.Object),
                new CutSelectionUseCase(_clipboardMock.Object),
                new PasteFromClipboardUseCase(_clipboardMock.Object, _sessionProvider));

            var globalState = new GlobalState();
            var addFrameProvider = new Mock<IAddFrameProvider>();
            var bitmapAdapter = new Mock<IBitmapAdapter<Bitmap>>();

            _viewModel = new DrawableCanvasViewModel(
                globalState,
                addFrameProvider.Object,
                _clipboardMock.Object,
                bitmapAdapter.Object,
                _sessionProvider,
                _selectionService,
                _coordinator);
        }

        [AvaloniaTest]
        public async Task Should_BeAbleToDragTwice_WithHighMagnification()
        {
            // 1. Paste 10x10 picture at (0,0) with magnification 32
            _viewModel.Magnification = new Magnification(32);
            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);
            await _viewModel.PasteCommand.Execute().ToTask();

            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(0, 0)));

            // 2. First Drag: from logical (5,5) to (15,15)
            // Display positions: (5*32, 5*32) = (160, 160) -> (15*32, 15*32) = (480, 480)
            await _viewModel.DrawBeginCommand.Execute(new Position(160, 160)).ToTask();
            await _viewModel.DrawingCommand.Execute(new Position(480, 480)).ToTask();
            await _viewModel.DrawEndCommand.Execute(new Position(480, 480)).ToTask();

            // After first drag, position should be (10, 10) in canvas coords
            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(10, 10)), "After first drag, position should be (10,10)");

            // 3. Mouse move before second drag
            await _viewModel.DrawingCommand.Execute(new Position(481, 481)).ToTask();

            // 4. Second Drag: from logical (15,15) to (20,20)
            // (15*32, 15*32) = (480, 480) -> (20*32, 20*32) = (640, 640)
            await _viewModel.DrawBeginCommand.Execute(new Position(480, 480)).ToTask();
            Assert.That(_coordinator.ActiveSelectionCursor, Is.EqualTo(SelectionCursor.Move), "Should be in Move cursor when clicking inside the selection for the second time");

            await _viewModel.DrawingCommand.Execute(new Position(640, 640)).ToTask();
            await _viewModel.DrawEndCommand.Execute(new Position(640, 640)).ToTask();

            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(15, 15)), "After second drag, position should be (15,15)");
        }
    }
}
