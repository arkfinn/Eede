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
        private PasteFromClipboardUseCase _pasteUseCase;
        private DrawableCanvasViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _sessionProvider = new DrawingSessionProvider();
            _sessionProvider.Update(new DrawingSession(Picture.CreateEmpty(new PictureSize(32, 32))));
            _coordinator = new InteractionCoordinator(_sessionProvider);
            _clipboardMock = new Mock<IClipboard>();
            _pasteUseCase = new PasteFromClipboardUseCase(_clipboardMock.Object);

            var globalState = new GlobalState();
            var addFrameProvider = new Mock<IAddFrameProvider>();
            var bitmapAdapter = new Mock<IBitmapAdapter<Bitmap>>();
            var copyUseCase = new CopySelectionUseCase(_clipboardMock.Object);
            var cutUseCase = new CutSelectionUseCase(_clipboardMock.Object);

            _viewModel = new DrawableCanvasViewModel(
                globalState,
                addFrameProvider.Object,
                _clipboardMock.Object,
                bitmapAdapter.Object,
                _sessionProvider,
                copyUseCase,
                cutUseCase,
                _pasteUseCase,
                _coordinator);
        }

        [AvaloniaTest]
        public async Task PasteCharacterization_CurrentBehavior()
        {
            // Arrange: 10x10 の赤い画像をクリップボードに用意
            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            // 本来は赤い色を塗るべきだが、仕様化テストとしては「渡されたインスタンス」で判定
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);

            // Act: ペースト実行
            await _viewModel.PasteCommand.Execute().ToTask();

            // Assert: 現在の挙動を確認
            // 1. DrawingSession の CurrentPreviewContent は null (現状の設計ミス/課題)
            Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent, Is.Null, "Current Session's preview should be null in CURRENT implementation");

            // 2. InteractionCoordinator 側はプレビュー情報を持っている
            Assert.That(_coordinator.PreviewPixels, Is.EqualTo(pastedPicture), "Coordinator should hold the pasted pixels as preview");
            Assert.That(_coordinator.PreviewPosition, Is.EqualTo(new Position(0, 0)));

            // 3. DrawingSession の CurrentPicture は合成されていない（元のまま）
            // CurrentPicture は PreviewContent が null なら Buffer.Fetch() を返すため
            Assert.That(_sessionProvider.CurrentSession.CurrentPicture, Is.EqualTo(_sessionProvider.CurrentSession.Buffer.Fetch()));
        }
    }
}
