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
    public class Reproduction_Issue20260208_v2
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
        public async Task Should_ShowHandlesImmediately_AfterPaste()
        {
            // 倍率を32に設定
            _viewModel.Magnification = new Magnification(32);

            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);

            // 貼り付け実行。この直後に SyncWithSession が呼ばれる。
            await _viewModel.PasteCommand.Execute().ToTask();

            // 検証1: マウスを動かす前に、ViewModel のプロパティが更新されているか
            Assert.That(_viewModel.IsShowHandles, Is.True, "Handles must be visible logically right after paste");
            Assert.That(_viewModel.SelectingArea, Is.Not.Null, "SelectingArea must be set right after paste");
            Assert.That(_viewModel.IsRegionSelecting, Is.True, "IsRegionSelecting must be true to show Region overlay");
        }

        [AvaloniaTest]
        public async Task Should_DetectClick_AtSubPixelPositions()
        {
            // 倍率を32に設定
            _viewModel.Magnification = new Magnification(32);

            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);
            await _viewModel.PasteCommand.Execute().ToTask();

            // 位置 (0,0) にある 10x10 の選択範囲に対して、
            // 論理ピクセル (5,5) の範囲内（表示座標 160〜191）の様々な位置をクリックしてみる

            // 1. ピクセルの中央 (160 + 16, 160 + 16) = (176, 176)
            await _viewModel.DrawBeginCommand.Execute(new Position(176, 176)).ToTask();
            Assert.That(_coordinator.ActiveSelectionCursor, Is.EqualTo(SelectionCursor.Move), "Center of pixel should be Move");
            await _viewModel.DrawEndCommand.Execute(new Position(176, 176)).ToTask();

            // 2. ピクセルの左上角ギリギリ (160, 160)
            await _viewModel.DrawBeginCommand.Execute(new Position(160, 160)).ToTask();
            Assert.That(_coordinator.ActiveSelectionCursor, Is.EqualTo(SelectionCursor.Move), "Top-left of pixel should be Move");
            await _viewModel.DrawEndCommand.Execute(new Position(160, 160)).ToTask();

            // 3. ピクセルの右下角ギリギリ (191, 191)
            await _viewModel.DrawBeginCommand.Execute(new Position(191, 191)).ToTask();
            Assert.That(_coordinator.ActiveSelectionCursor, Is.EqualTo(SelectionCursor.Move), "Bottom-right of pixel should be Move");
            await _viewModel.DrawEndCommand.Execute(new Position(191, 191)).ToTask();

            // 4. 隣のピクセルとの境界 (192, 192) -> これは論理ピクセル (6,6) になるはず
            await _viewModel.DrawBeginCommand.Execute(new Position(192, 192)).ToTask();
            Assert.That(_coordinator.ActiveSelectionCursor, Is.EqualTo(SelectionCursor.Move), "Boundary pixel should still be Move");
            await _viewModel.DrawEndCommand.Execute(new Position(192, 192)).ToTask();
        }
    }
}
