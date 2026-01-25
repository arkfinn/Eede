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
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Animations;
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

        // 各ツールの生成をモック
        _mockDrawStyleFactory.Setup(f => f.Create(DrawStyleType.FreeCurve)).Returns(new FreeCurve());
        _mockDrawStyleFactory.Setup(f => f.Create(DrawStyleType.Line)).Returns(new Line());
        _mockDrawStyleFactory.Setup(f => f.Create(DrawStyleType.RegionSelect)).Returns(new RegionSelector());
    }

    private MainViewModel CreateViewModel()
    {
        return new MainViewModel(
            _globalState,
            _mockAnimationService.Object,
            _mockClipboardService.Object,
            _mockBitmapAdapter.Object,
            _mockPictureRepository.Object,
            _mockDrawStyleFactory.Object,
            _mockPictureEditingUseCase.Object,
            _drawingSessionProvider);
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
    public void External_Injection_Constructor_Test()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;

            // 各サブ ViewModel を個別に作成
            var drawableCanvasViewModel = new DrawableCanvasViewModel(
                _globalState, null, _mockClipboardService.Object, _mockBitmapAdapter.Object, new Eede.Application.Drawings.DrawActionUseCase());
            var animationViewModel = new AnimationViewModel(_mockAnimationService.Object, new Moq.Mock<IFileSystem>().Object);
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
                paletteContainerViewModel
            );

            // 同期機能が働いているか確認（アニメーションモード）
            Assert.That(viewModel.DrawableCanvasViewModel.IsAnimationMode, Is.False);
            animationViewModel.IsAnimationMode = true;
            scheduler.AdvanceBy(1);
            Assert.That(viewModel.DrawableCanvasViewModel.IsAnimationMode, Is.True);
        });
    }
}
