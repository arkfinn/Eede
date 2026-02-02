using Eede.Application.Infrastructure;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Services;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Application.Pictures;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Settings;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Eede.Application.Animations;
using System.Reactive;

using Eede.Domain.Animations;
using System.Reactive.Threading.Tasks;

using Avalonia.Headless.NUnit;

namespace Eede.Presentation.Tests
{
    [TestFixture]
    public class PasteCharacterizationTests
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
        }

        [AvaloniaTest]
        public async Task PasteCharacterization_NewBehavior()
        {
            // Arrange: 10x10 の赤い画像をクリップボードに用意
            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);

            // Act: ペースト実行
            await _viewModel.PasteCommand.Execute().ToTask();

            // Assert: 新しい挙動を確認
            // 1. DrawingSession の CurrentPreviewContent が保持されていること
            Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent, Is.Not.Null, "Current Session should hold the preview");
            Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent.Pixels, Is.EqualTo(pastedPicture));
            Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent.Type, Is.EqualTo(SelectionPreviewType.Paste));

            // 2. InteractionCoordinator 側もプレビュー情報をセッションから取得している
            Assert.That(_coordinator.PreviewPixels, Is.EqualTo(pastedPicture), "Coordinator should hold the pasted pixels via session");
            Assert.That(_coordinator.PreviewPosition, Is.EqualTo(new Position(0, 0)));

            // 3. DrawingSession の CurrentPicture は合成されていること
            Assert.That(_sessionProvider.CurrentSession.CurrentPicture, Is.Not.EqualTo(_sessionProvider.CurrentSession.Buffer.Fetch()));
        }

        [AvaloniaTest]
        public async Task Paste_CommitByClickOutside_Test()
        {
            // Arrange: ペーストプレビュー状態
            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);
            await _viewModel.PasteCommand.Execute().ToTask();

            // Act: プレビュー範囲外をクリック
            // プレビューは (0,0) にあるので、(20,20) をクリック
            await _viewModel.DrawBeginCommand.Execute(new Position(20, 20)).ToTask();
            await _viewModel.DrawEndCommand.Execute(new Position(20, 20)).ToTask();

            // Assert: 確定されていること
            Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent, Is.Null, "Preview should be committed");
            // 履歴が1つ増えているはず（初期状態 + ペースト確定）
            Assert.That(_sessionProvider.CurrentSession.CanUndo(), Is.True);
        }

        [AvaloniaTest]
        public async Task Paste_CommitByToolChange_Test()
        {
            // Arrange: ペーストプレビュー状態 (RegionSelector)
            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);
            _viewModel.DrawStyle = new RegionSelector();
            await _viewModel.PasteCommand.Execute().ToTask();

            // Act: ツールをペンツール（FreeCurve）に切り替え
            _viewModel.DrawStyle = new FreeCurve();

            // Assert: 確定されていること
            Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent, Is.Null, "Preview should be committed on tool change");
        }

        [AvaloniaTest]
        public async Task Paste_Undo_Test()
        {
            // Arrange: 何かを描いて履歴がある状態からペーストプレビュー
            var initialPicture = _sessionProvider.CurrentSession.Buffer.Fetch();
            var secondPicture = Picture.CreateEmpty(new PictureSize(32, 32));
            _sessionProvider.Update(_sessionProvider.CurrentSession.Push(secondPicture));

            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);
            await _viewModel.PasteCommand.Execute().ToTask();

            // Act: Undo 実行
            _sessionProvider.Update(_sessionProvider.CurrentSession.Undo().Session);

            // Assert: ペースト前の状態に戻っていること
            Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent, Is.Null, "Preview should be cleared by Undo");
            Assert.That(_sessionProvider.CurrentSession.CurrentPicture, Is.EqualTo(initialPicture), "Should return to initial picture");
        }
    }
}
