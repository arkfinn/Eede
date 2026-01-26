using Eede.Application.Drawings;
using Eede.Application.Services;
using Eede.Application.Animations;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.Common.Adapters;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using ReactiveUI;
using ReactiveUI.Testing;
using System.Windows.Input;
using Avalonia.Headless.NUnit;

namespace Eede.Presentation.Tests.ViewModels.DataEntry;

public class DrawableCanvasViewModelTests
{
    private Mock<IClipboardService> _mockClipboard;
    private Mock<IAddFrameProvider> _mockAddFrameProvider;
    private GlobalState _globalState;

    [SetUp]
    public void Setup()
    {
        _mockClipboard = new Mock<IClipboardService>();
        _mockAddFrameProvider = new Mock<IAddFrameProvider>();
        _globalState = new GlobalState();
    }

    [AvaloniaTest]
    public void Characterization_DrawingAction()
    {
        // 仕様化テスト: 現在の挙動を固定する
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var mockSessionProvider = new Mock<IDrawingSessionProvider>();
            var viewModel = new DrawableCanvasViewModel(_globalState, _mockAddFrameProvider.Object, _mockClipboard.Object, new AvaloniaBitmapAdapter(), new DrawActionUseCase(), mockSessionProvider.Object);
            viewModel.SetPicture(Picture.CreateEmpty(new PictureSize(32, 32)));
            
            // 赤色で(10, 10)に描画
            var penColor = new ArgbColor(255, 255, 0, 0); // Red
            viewModel.PenColor = penColor;
            viewModel.PenSize = 1;
            viewModel.Magnification = new Magnification(1);
            scheduler.AdvanceBy(1);

            viewModel.DrawBeginCommand.Execute(new Position(10, 10)).Subscribe();
            scheduler.AdvanceBy(1);
            viewModel.DrawingCommand.Execute(new Position(10, 10)).Subscribe();
            scheduler.AdvanceBy(1);
            viewModel.DrawEndCommand.Execute(new Position(10, 10)).Subscribe();
            scheduler.AdvanceBy(1);

            var picture = viewModel.PictureBuffer.Previous;
            var pixel = picture.PickColor(new Position(10, 10));
            
            // 現在の挙動では、PenSize=1の場合、指定した座標が描画されるはず
            Assert.Multiple(() =>
            {
                Assert.That(pixel.Alpha, Is.EqualTo(255), "Alpha should be 255");
                Assert.That(pixel.Red, Is.EqualTo(255), "Red should be 255");
                Assert.That(pixel.Green, Is.EqualTo(0), "Green should be 0");
                Assert.That(pixel.Blue, Is.EqualTo(0), "Blue should be 0");
            });
        });
    }

    [AvaloniaTest]
    public void Characterization_MagnificationUpdate()
    {
        // 仕様化テスト: 倍率変更時に内部状態が更新されるか
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var mockSessionProvider = new Mock<IDrawingSessionProvider>();
            var viewModel = new DrawableCanvasViewModel(_globalState, _mockAddFrameProvider.Object, _mockClipboard.Object, new AvaloniaBitmapAdapter(), new DrawActionUseCase(), mockSessionProvider.Object);
            viewModel.SetPicture(Picture.CreateEmpty(new PictureSize(32, 32)));
            
            var initialMag = viewModel.Magnification.Value;
            Assert.That(initialMag, Is.EqualTo(4));

            viewModel.Magnification = new Magnification(8);
            
            Assert.That(viewModel.Magnification.Value, Is.EqualTo(8));
        });
    }

    [AvaloniaTest]
    public void MoveSelection_ShouldInvokeDrewWithOriginalArea()
    {
        // 選択範囲を移動した際、Drewイベントの引数が正しく「移動前」と「移動後」の範囲になっているかテスト
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var mockSessionProvider = new Mock<IDrawingSessionProvider>();
            var viewModel = new DrawableCanvasViewModel(_globalState, _mockAddFrameProvider.Object, _mockClipboard.Object, new AvaloniaBitmapAdapter(), new DrawActionUseCase(), mockSessionProvider.Object);
            var imageSize = new PictureSize(32, 32);
            viewModel.SetPicture(Picture.CreateEmpty(imageSize));
            viewModel.Magnification = new Magnification(1);

            // 1. 範囲選択モードにする
            var selector = new RegionSelector();
            viewModel.DrawStyle = selector;
            viewModel.SetupRegionSelector(selector);

            // 2. 選択範囲を作成 (10,10) - (20,20)
            // Command経由でRegionSelectorを駆動する
            viewModel.DrawBeginCommand.Execute(new Position(10, 10)).Subscribe();
            // ドラッグ中のイベントはテストでは省略可（RegionSelectorの実装によるが、DrawEndで確定するはず）
            viewModel.DrawEndCommand.Execute(new Position(20, 20)).Subscribe();

            Assert.That(viewModel.SelectingArea, Is.Not.Null);
            var originalArea = viewModel.SelectingArea.Value;
            // 期待値: (10, 10) から (20, 20) なので Width=10, Height=10
            Assert.That(originalArea.X, Is.EqualTo(10));
            Assert.That(originalArea.Y, Is.EqualTo(10));

            // 3. 移動開始 (Mouse Down)
            // SelectingArea内(15,15)をクリックして移動モード(DraggingState)へ
            var moveStartPos = new Position(15, 15);
            viewModel.DrawBeginCommand.Execute(moveStartPos).Subscribe();

            // 4. 移動中 (Mouse Move) -> (25, 25) へ +10, +10 移動
            var moveEndPos = new Position(25, 25);
            viewModel.DrawingCommand.Execute(moveEndPos).Subscribe();

            // 5. 移動終了 (Mouse Up) -> Drewイベント発火確認
            PictureArea? capturedPreviousArea = null;
            PictureArea? capturedNextArea = null;
            viewModel.Drew += (prevPic, nextPic, prevArea, nextArea) =>
            {
                capturedPreviousArea = prevArea;
                capturedNextArea = nextArea;
            };

            viewModel.DrawEndCommand.Execute(moveEndPos).Subscribe();

            Assert.Multiple(() =>
            {
                Assert.That(capturedPreviousArea, Is.Not.Null, "Previous area should not be null");
                Assert.That(capturedPreviousArea.Value, Is.EqualTo(originalArea), "Previous area should be the original area BEFORE move");
                
                // 移動量は (25-15, 25-15) = (+10, +10)
                var expectedNextArea = new PictureArea(new Position(originalArea.X + 10, originalArea.Y + 10), originalArea.Size);
                Assert.That(capturedNextArea.Value, Is.EqualTo(expectedNextArea), "Next area should be the moved area");
            });
        });
    }

    [AvaloniaTest]
    public void Undo_ShouldRestoreSelectionState()
    {
        // バグ再現テスト
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            
            var mockSessionProvider = new Mock<IDrawingSessionProvider>();
            
            // CurrentSessionプロパティの挙動を定義する変数を容易
            DrawingSession currentSession = null;
            mockSessionProvider.Setup(x => x.CurrentSession).Returns(() => currentSession);

            var viewModel = new DrawableCanvasViewModel(_globalState, _mockAddFrameProvider.Object, _mockClipboard.Object, new AvaloniaBitmapAdapter(), new DrawActionUseCase(), mockSessionProvider.Object);
            viewModel.Magnification = new Magnification(1);
            var imageSize = new PictureSize(32, 32);
            var initialPicture = Picture.CreateEmpty(imageSize);
            viewModel.SetPicture(initialPicture);

            // 1. 初期状態: 選択範囲を作成 (10, 10) - (20, 20)
            var selector = new RegionSelector();
            viewModel.DrawStyle = selector;
            viewModel.SetupRegionSelector(selector);
            viewModel.DrawBeginCommand.Execute(new Position(10, 10)).Subscribe();
            viewModel.DrawEndCommand.Execute(new Position(20, 20)).Subscribe();

            var originalArea = viewModel.SelectingArea.Value; 
            var originalSession = new DrawingSession(initialPicture, originalArea);
            
            // セッション更新をシミュレート
            currentSession = originalSession;
            mockSessionProvider.Raise(x => x.SessionChanged += null, originalSession);
            
            Assert.That(viewModel.SelectingArea, Is.EqualTo(originalArea));

            // 2. 移動操作 (15,15) -> (25,25)
            viewModel.DrawBeginCommand.Execute(new Position(15, 15)).Subscribe();
            viewModel.DrawingCommand.Execute(new Position(25, 25)).Subscribe();
            viewModel.DrawEndCommand.Execute(new Position(25, 25)).Subscribe();

            var movedArea = viewModel.SelectingArea.Value;
            Assert.That(movedArea.X, Is.EqualTo(20));

            // 3. Undo実行 (元のセッションが通知される)
            currentSession = originalSession;
            mockSessionProvider.Raise(x => x.SessionChanged += null, originalSession);

            // 期待値: 選択範囲が元の位置 (10, 10) に戻っていること
            Assert.That(viewModel.SelectingArea.Value, Is.EqualTo(originalArea), 
                $"Undo should restore SelectingArea to original position ({originalArea.X},{originalArea.Y}) but was ({viewModel.SelectingArea.Value.X},{viewModel.SelectingArea.Value.Y})");
        });
    }
}
