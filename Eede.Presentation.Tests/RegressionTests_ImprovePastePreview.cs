using Avalonia.Headless.NUnit;
using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.DataEntry;
using Moq;
using NUnit.Framework;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using Eede.Presentation.ViewModels.Pages;

namespace Eede.Presentation.Tests
{
    [TestFixture]
    public class RegressionTests_ImprovePastePreview
    {
        private DrawingSessionProvider _sessionProvider;
        private InteractionCoordinator _coordinator;
        private Mock<IClipboard> _clipboardMock;
        private ISelectionService _selectionService;
        private DrawableCanvasViewModel _viewModel;
        private DrawingSessionViewModel _sessionViewModel;

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
            var bitmapAdapter = new Mock<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>>();

            _viewModel = new DrawableCanvasViewModel(
                globalState,
                addFrameProvider.Object,
                _clipboardMock.Object,
                bitmapAdapter.Object,
                _sessionProvider,
                _selectionService,
                _coordinator);
            
            _viewModel.Magnification = new Magnification(1);
            _sessionViewModel = new DrawingSessionViewModel(_sessionProvider);
        }

        // Removed MoveSelection_ShouldCommitAtCorrectPosition_Regression as direct drag of selection is disabled for RegionSelector.

        [AvaloniaTest]
        public async Task Paste_ShouldInitializeAtSelectingAreaPosition_Regression()
        {
            // 1. (5,5) に範囲選択を作る
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(5, 5)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(15, 15)).Subscribe();

            // 2. ペースト実行
            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);
            await _viewModel.PasteCommand.Execute().ToTask();

            // Assert: ペースト位置が (5,5) になっていること
            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(5, 5)), "Paste should start at the selection's top-left corner");
        }

        // Removed MoveSelection_Commit_ShouldBeUndoable as direct drag of selection is disabled for RegionSelector.

        [AvaloniaTest]
        public async Task Move_Copy_Paste_Commit_ShouldWorkCorrectly()
        {
            // Setup: (0,0) に赤い点
            var red = new ArgbColor(255, 255, 0, 0);
            var rectData = new byte[4 * 4 * 4];
            for (int i = 0; i < rectData.Length; i += 4) { rectData[i] = 0; rectData[i + 1] = 0; rectData[i + 2] = 255; rectData[i + 3] = 255; }

            var picture = Picture.CreateEmpty(new PictureSize(32, 32)).Blend(new DirectImageBlender(), Picture.Create(new PictureSize(4, 4), rectData), new Position(0, 0));
            _sessionProvider.Update(new DrawingSession(picture));

            // 1. (0,0)-(4,4) を選択
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(4, 4)).Subscribe();

            // 2. Skipped moving selection directly as it is disabled.

            // 3. コピー実行
            _clipboardMock.Setup(x => x.CopyAsync(It.IsAny<Picture>())).Returns(Task.CompletedTask);
            await _viewModel.CopyCommand.Execute();

            // 4. ペースト実行 (コピーしたものが (0,0) にペーストされるはず)
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(Picture.Create(new PictureSize(4, 4), rectData));
            await _viewModel.PasteCommand.Execute();
            
            // 検証：ペースト位置が (0,0) になっていること (Selection position)
            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(0, 0)), "Pasted item should be at (0,0)");

            // 5. ペーストしたものを (10,10) へ移動
            // Center (2,2) -> Move +10 -> (12,12)
            _viewModel.DrawBeginCommand.Execute(new Position(2, 2)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(12, 12)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(12, 12)).Subscribe();

            // 6. 確定
            _viewModel.DrawBeginCommand.Execute(new Position(30, 30)).Subscribe();

            // 最終結果の検証
            var finalResult = _sessionProvider.CurrentSession.Buffer.Fetch();
            // 元の赤は (0,0) に残っているはず
            Assert.That(finalResult.PickColor(new Position(0, 0)), Is.EqualTo(red), "Original pixel should be at (0,0)");
            // ペーストされた赤は (10,10) にあるはず
            Assert.That(finalResult.PickColor(new Position(10, 10)), Is.EqualTo(red), "Pasted pixel should be at (10,10)");
            
            // 選択範囲の追随検証
            var currentArea = _sessionProvider.CurrentSession.CurrentSelectingArea;
            Assert.That(currentArea, Is.Not.Null);
            Assert.That(currentArea.Value.Position, Is.EqualTo(new Position(10, 10)), "Selection area should follow the last committed item");
        }

        [AvaloniaTest]
        public async Task Cursor_ShouldChange_AfterSelection()
        {
            // 1. (5,5)-(15,15) を選択
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(5, 5)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(15, 15)).Subscribe();

            // 2. 中央 (10,10) にマウスを移動 (倍率1)
            _viewModel.DrawingCommand.Execute(new Position(10, 10)).Subscribe();

            // 検証：カーソルが Move になっていること
            Assert.That(_viewModel.ActiveSelectionCursor, Is.EqualTo(SelectionCursor.Move), "Cursor should be Move over the selected area");

            // 3. 右下ハンドル (15,15) にマウスを移動
            _viewModel.DrawingCommand.Execute(new Position(15, 15)).Subscribe();
            Assert.That(_viewModel.ActiveSelectionCursor, Is.EqualTo(SelectionCursor.SizeNWSE), "Cursor should be NWSE over the bottom-right handle");
        }

        [AvaloniaTest]
        public async Task Cursor_ShouldChange_WhenHoveringSelection()
        {
            // 1. (5,5)-(15,15) を選択
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(5, 5)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(15, 15)).Subscribe();

            // 一旦カーソルを外す
            _viewModel.DrawingCommand.Execute(new Position(30, 30)).Subscribe();
            Assert.That(_viewModel.ActiveSelectionCursor, Is.EqualTo(SelectionCursor.Default));

            // 2. ホバー移動 (ボタンを押さずに 10,10 へ)
            // ViewModelのDrawingCommandは内部的にcoordinator.PointerMovedを呼ぶだけなので、
            // ボタン押下状態(IsLeftButtonPressing)に関わらずカーソル更新が走るはず
            _viewModel.DrawingCommand.Execute(new Position(10, 10)).Subscribe();

            // 検証
            Assert.That(_viewModel.ActiveSelectionCursor, Is.EqualTo(SelectionCursor.Move), "Cursor should change to Move just by hovering");
        }

        [AvaloniaTest]
        public async Task Paste_And_Resize_ShouldWork()
        {
            // 1. (5,5) に範囲選択を作る
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(5, 5)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(15, 15)).Subscribe();

            // 2. ペースト実行 (10x10画像)
            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);
            await _viewModel.PasteCommand.Execute().ToTask();

            // 3. 右下ハンドル (15,15) を掴んで (20,20) へドラッグ
            // handleSize=8 なので、(15,15) は確実にハンドル内
            _viewModel.DrawBeginCommand.Execute(new Position(15, 15)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(20, 20)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(20, 20)).Subscribe();

            // 検証：サイズが 15x15 になっていること (5,5) から (20,20)
            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(5, 5)));
            var info = _sessionProvider.CurrentSession.CurrentPreviewContent;
            Assert.That(info.Pixels.Size, Is.EqualTo(new PictureSize(15, 15)), "Size should be resized to 15x15");
        }

        [AvaloniaTest]
        public async Task Paste_And_Move_ShouldWork()
        {
            // 1. (5,5) に範囲選択を作る
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(5, 5)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(15, 15)).Subscribe();

            // 2. ペースト実行 (10x10画像)
            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);
            await _viewModel.PasteCommand.Execute().ToTask();

            // 3. 画像の中央 (10,10) を掴んで (20,20) へ移動
            // (10,10) は (5,5)-(15,15) の中央
            _viewModel.DrawBeginCommand.Execute(new Position(10, 10)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(20, 20)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(20, 20)).Subscribe();

            // 検証：位置が (15,15) になっていること ((5,5) + (20-10, 20-10))
            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(15, 15)), "Position should be moved to (15,15)");
        }

        [AvaloniaTest]
        public async Task Paste_Resize_Then_Move_ShouldWork()
        {
            // 1. (5,5) に範囲選択を作る
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(5, 5)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(15, 15)).Subscribe();

            // 2. ペースト実行 (10x10画像)
            var pastedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(pastedPicture);
            await _viewModel.PasteCommand.Execute().ToTask();

            // 3. リサイズ: 右下 (15,15) を (20,20) へドラッグ -> 15x15 に
            _viewModel.DrawBeginCommand.Execute(new Position(15, 15)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(20, 20)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(20, 20)).Subscribe();

            Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent.Pixels.Size, Is.EqualTo(new PictureSize(15, 15)));

            // 4. 移動: リサイズ後の画像中央 (12,12) 辺りを掴んで (22,22) へ移動
            // (5,5)-(20,20) の中央は (12,12)
            _viewModel.DrawBeginCommand.Execute(new Position(12, 12)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(22, 22)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(22, 22)).Subscribe();

            // 検証：位置が (15,15) になっていること ((5,5) + (22-12, 22-12))
            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(15, 15)), "Position should be moved to (15,15) after resizing");
            Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent.Pixels.Size, Is.EqualTo(new PictureSize(15, 15)), "Size should be maintained");
        }

        [AvaloniaTest]
        public async Task Resize_Commit_ShouldWorkCorrectly()
        {
            // Setup: (0,0)-(4,4) に赤い矩形
            var red = new ArgbColor(255, 255, 0, 0);
            var rectData = new byte[4 * 4 * 4];
            for (int i = 0; i < rectData.Length; i += 4) { rectData[i] = 0; rectData[i + 1] = 0; rectData[i + 2] = 255; rectData[i + 3] = 255; }
            var picture = Picture.CreateEmpty(new PictureSize(32, 32)).Blend(new DirectImageBlender(), Picture.Create(new PictureSize(4, 4), rectData), new Position(0, 0));
            _sessionProvider.Update(new DrawingSession(picture));

            // 1. Select (0,0)-(4,4)
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(4, 4)).Subscribe();

            // 2. Resize: Drag TopLeft(0,0) to (2,2) -> Shrink to 2x2
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(2, 2)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(2, 2)).Subscribe();

            // 3. Commit
            _viewModel.DrawBeginCommand.Execute(new Position(30, 30)).Subscribe();

            // Assert
            var finalResult = _sessionProvider.CurrentSession.Buffer.Fetch();
            // (0,0) should be transparent (cleared) because it moved/shrank
            Assert.That(finalResult.PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0), "Original (0,0) should be cleared");
            // (2,2) should be Red (new TopLeft)
            Assert.That(finalResult.PickColor(new Position(2, 2)), Is.EqualTo(red), "New TopLeft (2,2) should be red");
        }

        [AvaloniaTest]
        public async Task Move_Then_Click_Outside_Commits()
        {
            // Setup
            var picture = Picture.CreateEmpty(new PictureSize(32, 32)).Blend(new DirectImageBlender(), Picture.Create(new PictureSize(4, 4), new byte[4 * 4 * 4]), new Position(0, 0));
            _sessionProvider.Update(new DrawingSession(picture));

            // Select
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(4, 4)).Subscribe();

            // Move
            _viewModel.DrawBeginCommand.Execute(new Position(2, 2)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(10, 10)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(10, 10)).Subscribe();

            // Assert: Preview State
            Assert.That(_viewModel.PreviewPixels, Is.Not.Null);

            // Click Outside (20, 20)
            _viewModel.DrawBeginCommand.Execute(new Position(20, 20)).Subscribe();

            // Assert: Committed (Preview null)
            Assert.That(_viewModel.PreviewPixels, Is.Null, "Preview should be null after clicking outside");
        }

        [AvaloniaTest]
        public async Task SwitchTool_ShouldCommitAndClearSelection()
        {
            // Setup
            var picture = Picture.CreateEmpty(new PictureSize(32, 32));
            _sessionProvider.Update(new DrawingSession(picture));

            // 1. Select
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(4, 4)).Subscribe();
            Assert.That(_viewModel.IsRegionSelecting, Is.True);

            // 2. Switch to FreeCurve
            _viewModel.DrawStyle = new FreeCurve();

            // Assert: SelectingArea should be null
            Assert.That(_viewModel.SelectingArea, Is.Null, "Selection should be cleared when switching to non-selection tool");
            Assert.That(_viewModel.IsRegionSelecting, Is.False);
        }

        [AvaloniaTest]
        public async Task SwitchTool_AfterMoveAndResize_ShouldCommitAndClearSelection()
        {
            // 1. セットアップ
            var picture = Picture.CreateEmpty(new PictureSize(32, 32));
            _sessionProvider.Update(new DrawingSession(picture));

            // 2. 範囲選択 (0,0)-(4,4)
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(4, 4)).Subscribe();

            // 3. 移動操作 (0,0)から(10,10)へ
            _viewModel.DrawBeginCommand.Execute(new Position(2, 2)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(12, 12)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(12, 12)).Subscribe();

            // 4. ツール切り替え (ペンツールへ)
            _viewModel.DrawStyle = new FreeCurve();

            // 5. 検証
            // 選択枠が表示されていないこと
            Assert.That(_viewModel.IsRegionSelecting, Is.False, "IsRegionSelecting should be false after tool switch");
            // 選択範囲データが空であること
            Assert.That(_viewModel.SelectingArea, Is.Null, "SelectingArea should be null after tool switch");
            // プレビュー表示が消えていること
            Assert.That(_viewModel.PreviewPixels, Is.Null, "PreviewPixels should be null after tool switch");
        }
    }
}