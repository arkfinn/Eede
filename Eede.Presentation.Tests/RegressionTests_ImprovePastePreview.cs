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
            var picture = Picture.CreateEmpty(new PictureSize(32, 32)).Blend(new DirectImageBlender(), Picture.Create(new PictureSize(1, 1), new byte[] { 0, 0, 255, 255 }), new Position(0, 0));
            _sessionProvider.Update(new DrawingSession(picture));

            // 1. (0,0)-(1,1) を選択
            _viewModel.DrawStyle = new RegionSelector();
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(1, 1)).Subscribe();

            // 2. Skipped moving selection directly as it is disabled.

            // 3. コピー実行
            _clipboardMock.Setup(x => x.CopyAsync(It.IsAny<Picture>())).Returns(Task.CompletedTask);
            await _viewModel.CopyCommand.Execute();

            // 4. ペースト実行 (コピーしたものが (0,0) にペーストされるはず)
            _clipboardMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(Picture.Create(new PictureSize(1, 1), new byte[] { 0, 0, 255, 255 }));
            await _viewModel.PasteCommand.Execute();
            
            // 検証：ペースト位置が (0,0) になっていること (Selection position)
            Assert.That(_viewModel.PreviewPosition, Is.EqualTo(new Position(0, 0)), "Pasted item should be at (0,0)");

            // 5. ペーストしたものを (10,10) へ移動
            _viewModel.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
            _viewModel.DrawingCommand.Execute(new Position(10, 10)).Subscribe();
            _viewModel.DrawEndCommand.Execute(new Position(10, 10)).Subscribe();

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
    }
}