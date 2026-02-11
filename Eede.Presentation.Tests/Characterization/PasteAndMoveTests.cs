using Avalonia.Headless.NUnit;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Services;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Application.Infrastructure;
using Eede.Presentation.Settings;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using System.Reactive.Threading.Tasks;
using Eede.Domain.Animations;
using Avalonia;

namespace Eede.Presentation.Tests.Characterization
{
    [TestFixture]
    public class PasteAndMoveTests
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
            _sessionProvider.Update(new DrawingSession(Picture.CreateEmpty(new PictureSize(32, 32))));
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

            _viewModel.Magnification = new Magnification(1);
        }

        [AvaloniaTest]
        public async Task ShouldBeAbleToMoveTwiceAfterPaste()
        {
            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);
            await _viewModel.PasteCommand.Execute().ToTask();

            Assert.That(_viewModel.IsShowHandles, Is.True, "Handles should be visible after paste");
            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(0, 0)));

            // 1回目の移動
            await _viewModel.DrawBeginCommand.Execute(new Position(2, 2)).ToTask();
            await _viewModel.DrawingCommand.Execute(new Position(7, 7)).ToTask();
            await _viewModel.DrawEndCommand.Execute(new Position(7, 7)).ToTask();

            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(5, 5)), "Position should be (5,5) after first move");
            Assert.That(_viewModel.IsShowHandles, Is.True, "Handles should still be visible");

            // 2回目の移動
            await _viewModel.DrawBeginCommand.Execute(new Position(7, 7)).ToTask();
            await _viewModel.DrawingCommand.Execute(new Position(12, 12)).ToTask();
            await _viewModel.DrawEndCommand.Execute(new Position(12, 12)).ToTask();

            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(10, 10)), "Position should be (10,10) after second move");
            Assert.That(_viewModel.IsShowHandles, Is.True, "Handles should still be visible after second move");
        }

        [AvaloniaTest]
        public async Task UndoAfterCommit_ShouldReturnToBeforePaste()
        {
            var initialPicture = _sessionProvider.CurrentSession.Buffer.Fetch();
            _sessionProvider.Update(_sessionProvider.CurrentSession.Push(Picture.CreateEmpty(new PictureSize(32, 32))));
            var pictureBeforePaste = _sessionProvider.CurrentSession.Buffer.Fetch();

            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);
            await _viewModel.PasteCommand.Execute().ToTask();

            _viewModel.CommitSelection();
            Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent, Is.Null, "Preview should be null after commit");

            _sessionProvider.Update(_sessionProvider.CurrentSession.Undo().Session);

            Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent, Is.Null, "Expected: Preview should NOT be restored after undoing commit");
            // インスタンス比較ではなく、ピクセル内容やプロパティで比較する
            var restoredPicture = _sessionProvider.CurrentSession.Buffer.Fetch();
            Assert.That(restoredPicture.Size, Is.EqualTo(pictureBeforePaste.Size));
            // 本来ならピクセルデータの等価性チェックを入れるべきだが、まずはインスタンスが戻っているか、
            // もしくは期待した「ペースト前」の状態（空のキャンバス）になっているかを確認
            Assert.That(restoredPicture, Is.EqualTo(pictureBeforePaste), "Should return to the picture state before paste");
        }
    }
}