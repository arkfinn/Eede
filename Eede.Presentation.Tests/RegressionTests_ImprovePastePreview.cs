using Avalonia.Headless.NUnit;
using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
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

using Eede.Presentation.ViewModels.Pages; // Add this

namespace Eede.Presentation.Tests
{
    [TestFixture]
    public class RegressionTests_ImprovePastePreview
    {
        private DrawingSessionProvider _sessionProvider;
        private InteractionCoordinator _coordinator;
        private Mock<IClipboard> _clipboardMock;
        private PasteFromClipboardUseCase _pasteUseCase;
        private DrawableCanvasViewModel _viewModel;
        private DrawingSessionViewModel _sessionViewModel; // Add this

        [SetUp]
        public void Setup()
        {
            _sessionProvider = new DrawingSessionProvider();
            _sessionProvider.Update(new DrawingSession(Picture.CreateEmpty(new PictureSize(32, 32))));
            _coordinator = new InteractionCoordinator(_sessionProvider);
            _clipboardMock = new Mock<IClipboard>();
            _pasteUseCase = new PasteFromClipboardUseCase(_clipboardMock.Object, _sessionProvider);

            var globalState = new GlobalState();
            var addFrameProvider = new Mock<IAddFrameProvider>();
            var bitmapAdapter = new Mock<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>>();
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
            
            _viewModel.Magnification = new Magnification(1);
            _sessionViewModel = new DrawingSessionViewModel(_sessionProvider); // Initialize
        }

        [AvaloniaTest]
        public async Task MoveSelection_ShouldCommitAtCorrectPosition_Regression()
        {
            // 1. (0,0) に赤い点がある画像を用意
            var red = new ArgbColor(255, 255, 0, 0);
            var picture = Picture.CreateEmpty(new PictureSize(32, 32)).Blend(new DirectImageBlender(), Picture.Create(new PictureSize(1, 1), new byte[] { 0, 0, 255, 255 }), new Position(0, 0));
            _sessionProvider.Update(new DrawingSession(picture));

            // 2. (0,0) を範囲選択 (1x1の範囲にするため (0,0)-(1,1) を指定)
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(1, 1)).Subscribe();

            // 3. (10,10) へドラッグ移動 (移動開始点を (0,0) とし、終了点を (10,10) とする)
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(10, 10)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(10, 10)).Subscribe();

            // 4. 範囲外をクリックして確定
            _viewModel.DrawBeginCommand.Execute(new Position(30, 30)).Subscribe();

            // Assert: (10,10) が赤くなっていること
            var finalResult = _sessionProvider.CurrentSession.Buffer.Fetch();
            Assert.That(finalResult.PickColor(new Position(10, 10)), Is.EqualTo(red), "The pixel should be committed at the dragged position (10,10)");
            Assert.That(finalResult.PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0), "The original position (0,0) should be transparent");
        }

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

        [AvaloniaTest]
        public async Task MoveSelection_Commit_ShouldBeUndoable()
        {
            // Setup: (0,0) に赤い点がある画像
            var red = new ArgbColor(255, 255, 0, 0);
            var picture = Picture.CreateEmpty(new PictureSize(32, 32)).Blend(new DirectImageBlender(), Picture.Create(new PictureSize(1, 1), new byte[] { 0, 0, 255, 255 }), new Position(0, 0));
            _sessionProvider.Update(new DrawingSession(picture));

            // Initial check: Undo should be disabled
            var canUndo = await _sessionViewModel.UndoCommand.CanExecute.FirstAsync();
            Assert.That(canUndo, Is.False, "Undo should be disabled initially");

            // 1. Select (0,0)-(1,1)
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(1, 1)).Subscribe();

            // 2. Move to (10,10)
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(10, 10)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(10, 10)).Subscribe();

            // 3. Commit by clicking outside
            _viewModel.DrawBeginCommand.Execute(new Position(30, 30)).Subscribe();

            // Assert: Undo should be enabled
            canUndo = await _sessionViewModel.UndoCommand.CanExecute.FirstAsync();
            Assert.That(canUndo, Is.True, "Undo should be enabled after commit");

            // 4. Execute Undo
            await _sessionViewModel.UndoCommand.Execute();

            // Assert: Picture restored
            var result = _sessionProvider.CurrentSession.Buffer.Fetch();
            Assert.That(result.PickColor(new Position(0, 0)), Is.EqualTo(red), "Pixel at (0,0) should be restored");
            Assert.That(result.PickColor(new Position(10, 10)).Alpha, Is.EqualTo(0), "Pixel at (10,10) should be cleared");
        }

        [AvaloniaTest]
        public async Task Move_Copy_Paste_Commit_ShouldWorkCorrectly()
        {
            // Setup: (0,0) に赤い点
            var red = new ArgbColor(255, 255, 0, 0);
            var picture = Picture.CreateEmpty(new PictureSize(32, 32)).Blend(new DirectImageBlender(), Picture.Create(new PictureSize(1, 1), new byte[] { 0, 0, 255, 255 }), new Position(0, 0));
            _sessionProvider.Update(new DrawingSession(picture));

            // 1. (0,0)-(1,1) を選択
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(1, 1)).Subscribe();

            // 2. (5,5) へ移動開始
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(5, 5)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(5, 5)).Subscribe();

            // 3. コピー実行 (移動が自動的に確定されるはず)
            _clipboardMock.Setup(x => x.CopyAsync(It.IsAny<Picture>())).Returns(Task.CompletedTask);
            await _viewModel.CopyCommand.Execute();

            // 4. ペースト実行 (コピーしたものが (5,5) にペーストされるはず)
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(Picture.Create(new PictureSize(1, 1), new byte[] { 0, 0, 255, 255 }));
            await _viewModel.PasteCommand.Execute();
            
            // 検証：ペースト位置が (5,5) になっていること
            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(5, 5)), "Pasted item should be at (5,5)");

            // 5. ペーストしたものを (10,10) へ移動
            _viewModel.DrawBeginCommand.Execute(new Position(5, 5)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(10, 10)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(10, 10)).Subscribe();

            // 6. 確定
            _viewModel.DrawBeginCommand.Execute(new Position(30, 30)).Subscribe();

            // 最終結果の検証
            var finalResult = _sessionProvider.CurrentSession.Buffer.Fetch();
            // 最初の移動で (5,5) に赤が移動し、ペースト後の移動で (10,10) にも赤があるはず
            Assert.That(finalResult.PickColor(new Position(5, 5)), Is.EqualTo(red), "First moved pixel should be at (5,5)");
            Assert.That(finalResult.PickColor(new Position(10, 10)), Is.EqualTo(red), "Pasted pixel should be at (10,10)");
            
            // 選択範囲の追随検証
            var currentArea = _sessionProvider.CurrentSession.CurrentSelectingArea;
            Assert.That(currentArea, Is.Not.Null);
            Assert.That(currentArea.Value.Position, Is.EqualTo(new Position(10, 10)), "Selection area should follow the last committed item");
        }
    }
}
