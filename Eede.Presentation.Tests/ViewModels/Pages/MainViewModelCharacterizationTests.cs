using Avalonia.Headless.NUnit;
using Eede.Application.Animations;
using Eede.Application.Services;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Pages;
using Moq;
using NUnit.Framework;
using ReactiveUI;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using System.Reactive;
using System;
using Eede.Domain.SharedKernel;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Application.UseCase.Pictures;
using Eede.Application.Drawings;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.Common.Services;

namespace Eede.Presentation.Tests.ViewModels.Pages;

[TestFixture]
public class MainViewModelCharacterizationTests
{
    private GlobalState _globalState;
    private Mock<IAnimationService> _mockAnimationService;
    private Mock<IClipboardService> _mockClipboardService;
    private Mock<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>> _mockBitmapAdapter;
    private Mock<IPictureRepository> _mockPictureRepository;
    private Mock<IDrawStyleFactory> _mockDrawStyleFactory;
    private Mock<IPictureEditingUseCase> _mockPictureEditingUseCase;
    private IDrawingSessionProvider _drawingSessionProvider;
    private Mock<IDrawActionUseCase> _mockDrawActionUseCase;

    [SetUp]
    public void Setup()
    {
        _globalState = new GlobalState();
        _mockAnimationService = new Mock<IAnimationService>();
        _mockAnimationService.Setup(s => s.Patterns).Returns(new System.Collections.Generic.List<Eede.Domain.Animations.AnimationPattern>());
        _mockClipboardService = new Mock<IClipboardService>();
        _mockBitmapAdapter = new Mock<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>>();
        _mockPictureRepository = new Mock<IPictureRepository>();
        _mockDrawStyleFactory = new Mock<IDrawStyleFactory>();
        _mockPictureEditingUseCase = new Mock<IPictureEditingUseCase>();
        _drawingSessionProvider = new DrawingSessionProvider();
        _mockDrawActionUseCase = new Mock<IDrawActionUseCase>();

        // 各ツールの生成をモック
        _mockDrawStyleFactory.Setup(f => f.Create(DrawStyleType.FreeCurve)).Returns(new FreeCurve());
        _mockDrawStyleFactory.Setup(f => f.Create(DrawStyleType.Line)).Returns(new Line());
        _mockDrawStyleFactory.Setup(f => f.Create(DrawStyleType.RegionSelect)).Returns(new RegionSelector());
    }

    private MainViewModel CreateViewModel()
    {
        var animationViewModel = new AnimationViewModel(_mockAnimationService.Object, new Mock<IFileSystem>().Object);
        var drawableCanvasViewModel = new DrawableCanvasViewModel(
            _globalState,
            animationViewModel,
            _mockClipboardService.Object,
            _mockBitmapAdapter.Object,
            _mockDrawActionUseCase.Object,
            _drawingSessionProvider,
            new CopySelectionUseCase(_mockClipboardService.Object),
            new CutSelectionUseCase(_mockClipboardService.Object),
            new PasteFromClipboardUseCase(_mockClipboardService.Object));
        var drawingSessionViewModel = new DrawingSessionViewModel(_drawingSessionProvider);
        var paletteContainerViewModel = new PaletteContainerViewModel();

        return new MainViewModel(
            _globalState,
            _mockClipboardService.Object,
            _mockBitmapAdapter.Object,
            _mockPictureRepository.Object,
            _mockDrawStyleFactory.Object,
            _mockPictureEditingUseCase.Object,
            _drawingSessionProvider,
            drawableCanvasViewModel,
            animationViewModel,
            drawingSessionViewModel,
            paletteContainerViewModel,
            null!,
            null!,
            null!);
    }

    [AvaloniaTest]
    public void DrawStyle_Sync_Test()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = CreateViewModel();

            // 初期状態は FreeCurve
            Assert.That(viewModel.DrawStyle, Is.EqualTo(DrawStyleType.FreeCurve));
            Assert.That(viewModel.DrawableCanvasViewModel.DrawStyle, Is.InstanceOf<FreeCurve>());

            // Line に変更
            viewModel.DrawStyle = DrawStyleType.Line;
            scheduler.AdvanceBy(1);
            Assert.That(viewModel.DrawableCanvasViewModel.DrawStyle, Is.InstanceOf<Line>());

            // RegionSelect に変更
            viewModel.DrawStyle = DrawStyleType.RegionSelect;
            scheduler.AdvanceBy(1);
            Assert.That(viewModel.DrawableCanvasViewModel.DrawStyle, Is.InstanceOf<RegionSelector>());

            // Factory が各タイプで呼ばれたことを検証
            _mockDrawStyleFactory.Verify(f => f.Create(DrawStyleType.Line), Times.Once);
            _mockDrawStyleFactory.Verify(f => f.Create(DrawStyleType.RegionSelect), Times.Once);
        });
    }

    [AvaloniaTest]
    public void ColorPicked_Sync_Test()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = CreateViewModel();
            var expectedColor = new ArgbColor(255, 128, 64, 32);

            // DrawableCanvasViewModel で色選択が発生
            viewModel.DrawableCanvasViewModel.OnColorPicked.Execute(expectedColor).Subscribe();
            scheduler.AdvanceBy(1);

            // MainViewModel の PenColor が同期される
            Assert.That(viewModel.PenColor, Is.EqualTo(expectedColor));
        });
    }

    [AvaloniaTest]
    public void Drew_Event_Reflects_To_UndoRedo_Test()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = CreateViewModel();
            var dummyPicture = Picture.CreateEmpty(new PictureSize(32, 32));

            // 初期状態では Undo 不可
            Assert.That(((System.Windows.Input.ICommand)viewModel.UndoCommand).CanExecute(null), Is.False);

            // 描画完了イベントを発生させる
            viewModel.DrawableCanvasViewModel.OnDrew.Execute(dummyPicture).Subscribe();
            scheduler.AdvanceBy(1);

            // Undo が実行可能になる
            Assert.That(((System.Windows.Input.ICommand)viewModel.UndoCommand).CanExecute(null), Is.True);
        });
    }

    [AvaloniaTest]
    public void AnimationMode_Sync_Test()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = CreateViewModel();

            // 初期状態は false
            Assert.That(viewModel.DrawableCanvasViewModel.IsAnimationMode, Is.False);

            // アニメーションモードを ON に
            viewModel.AnimationViewModel.IsAnimationMode = true;
            scheduler.AdvanceBy(1);

            // DrawableCanvasViewModel に伝播することを確認
            Assert.That(viewModel.DrawableCanvasViewModel.IsAnimationMode, Is.True);
        });
    }

    [AvaloniaTest]
    public void PaletteColor_Fetch_Sync_Test()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = CreateViewModel();
            var expectedColor = viewModel.PaletteContainerViewModel.Palette.Fetch(0);

            // パレットの0番目の色を取得する操作をシミュレート
            viewModel.PaletteContainerViewModel.FetchColorCommand.Execute(0).Subscribe();
            scheduler.AdvanceBy(1);

            // MainViewModel の PenColor に反映されることを確認
            Assert.That(viewModel.PenColor, Is.EqualTo(expectedColor));
        });
    }

    [AvaloniaTest]
    public void Undo_After_Move_Should_Restore_SelectingArea_Test()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = CreateViewModel();
            var initialArea = new PictureArea(new Position(0, 0), new PictureSize(10, 10));
            var nextArea = new PictureArea(new Position(10, 10), new PictureSize(10, 10));

            // 1. 初期状態を設定（範囲選択中とする）
            viewModel.DrawableCanvasViewModel.SelectingArea = initialArea;
            viewModel.DrawStyle = DrawStyleType.RegionSelect;
            // 選択範囲が決定したことを履歴に刻む（画像は変わっていないが、SelectingArea を確定させる）
            _drawingSessionProvider.Update(_drawingSessionProvider.CurrentSession.UpdateSelectingArea(initialArea));
            scheduler.AdvanceBy(1);

            // 2. 移動操作（Push）をシミュレート
            // 実際の移動ツールでは、移動後の座標で Push される
            viewModel.DrawableCanvasViewModel.SelectingArea = nextArea;
            var nextPicture = Picture.CreateEmpty(new PictureSize(32, 32));
            
            // 手動で Drew イベントを発火させ、移動前の座標 (initialArea) を渡す
            var method = viewModel.DrawableCanvasViewModel.GetType().GetField("Drew", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var eventDelegate = (MulticastDelegate)method.GetValue(viewModel.DrawableCanvasViewModel);
            eventDelegate.DynamicInvoke(null, nextPicture, (PictureArea?)initialArea, (PictureArea?)nextArea);
            
            scheduler.AdvanceBy(1);

            // この時点で、履歴の最新(Current)は nextArea、Undoスタックのトップは initialArea になっているはず

            // 3. アンドゥ実行
            viewModel.UndoCommand.Execute().Subscribe();
            scheduler.AdvanceBy(1);

            // 4. 検証: SelectingArea が初期状態に戻っているべき（現在は失敗するはず）
            Assert.That(viewModel.DrawableCanvasViewModel.SelectingArea, Is.EqualTo(initialArea));
        });
    }

    [AvaloniaTest]
    public void Dock_Selection_Should_Not_Affect_Canvas_Frame_When_Not_RegionSelect_Tool_Test()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = CreateViewModel();

            // 1. 作業エリア側のツールを「自由曲線（ペン）」に設定
            viewModel.DrawStyle = DrawStyleType.FreeCurve;
            Assert.That(viewModel.DrawableCanvasViewModel.IsRegionSelecting, Is.False);

            // 2. ドックエリアの ViewModel を作成
            var dockVM = new DockPictureViewModel(
                _globalState,
                viewModel.AnimationViewModel,
                _mockBitmapAdapter.Object,
                null!,
                null!
            );
            var dummyPicture = Picture.CreateEmpty(new PictureSize(100, 100));
            viewModel.DrawableCanvasViewModel.SetPicture(dummyPicture);
            _mockPictureEditingUseCase.Setup(u => u.PushToCanvas(It.IsAny<Picture>(), It.IsAny<Picture>(), It.IsAny<PictureArea>()))
                .Returns(new PictureEditingUseCase.EditResult(dummyPicture, dummyPicture, null));

            // MainViewModel の購読ロジックを登録（SetupDockPicture をシミュレート）
            var privateSetupMethod = viewModel.GetType().GetMethod("SetupDockPicture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            privateSetupMethod.Invoke(viewModel, new object[] { dockVM });

            // 3. ドック側で「画像転送（Push）」を発生させる
            var targetArea = new PictureArea(new Position(16, 16), new PictureSize(32, 32));
            var pushMethod = dockVM.GetType().GetMethod("ExecutePicturePush", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pushMethod.Invoke(dockVM, new object[] { targetArea });

            scheduler.AdvanceBy(1);

            // 4. 検証: 作業エリア側の IsRegionSelecting は false のままであるべき
            Assert.That(viewModel.DrawableCanvasViewModel.IsRegionSelecting, Is.False, "ドックの操作によって作業エリアの枠が勝手に表示されてはいけません。");
        });
    }

    [AvaloniaTest]
    public void External_Injection_Constructor_Test()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;

            // 各サブ ViewModel を個別に作成
            var animationViewModel = new AnimationViewModel(_mockAnimationService.Object, new Moq.Mock<IFileSystem>().Object);
            var drawableCanvasViewModel = new DrawableCanvasViewModel(
                _globalState,
                animationViewModel,
                _mockClipboardService.Object,
                _mockBitmapAdapter.Object,
                _mockDrawActionUseCase.Object,
                _drawingSessionProvider,
                new CopySelectionUseCase(_mockClipboardService.Object),
                new CutSelectionUseCase(_mockClipboardService.Object),
                new PasteFromClipboardUseCase(_mockClipboardService.Object));
            var drawingSessionViewModel = new DrawingSessionViewModel(_drawingSessionProvider);
            var paletteContainerViewModel = new PaletteContainerViewModel();

            // 新しいコンストラクタ（接合部）を使用して MainViewModel を生成
            var viewModel = new MainViewModel(
                _globalState,
                _mockClipboardService.Object,
                _mockBitmapAdapter.Object,
                _mockPictureRepository.Object,
                _mockDrawStyleFactory.Object,
                _mockPictureEditingUseCase.Object,
                _drawingSessionProvider,
                drawableCanvasViewModel,
                animationViewModel,
                drawingSessionViewModel,
                paletteContainerViewModel,
                null!,
                null!,
                null!
            );

            // 同期機能が働いているか確認（アニメーションモード）
            Assert.That(viewModel.DrawableCanvasViewModel.IsAnimationMode, Is.False);
            animationViewModel.IsAnimationMode = true;
            scheduler.AdvanceBy(1);
            Assert.That(viewModel.DrawableCanvasViewModel.IsAnimationMode, Is.True);
        });
    }
}