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
            _mockPictureEditingUseCase.Object);
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
}
