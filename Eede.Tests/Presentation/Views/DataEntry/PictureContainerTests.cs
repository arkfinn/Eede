using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Pictures;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.UseCase.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Settings;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.History;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Coordinators;
using Eede.Presentation.Theming;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.General;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.DataEntry;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.VisualTree;
using System.Linq;
using Avalonia.Media.Imaging;

namespace Eede.Presentation.Tests.Views.DataEntry;

[TestFixture]
public class PictureContainerTests
{
    private MainViewModel _mainViewModel;
    private DockPictureViewModel _dockViewModel;
    private Window _window;
    private PictureContainer _container;

    [SetUp]
    public void Setup()
    {
        // 1. Core Dependencies
        var bitmapAdapter = new AvaloniaBitmapAdapter();
        var globalState = new GlobalState();

        var mockDrawingSessionProvider = new Mock<IDrawingSessionProvider>();
        mockDrawingSessionProvider.Setup(x => x.CurrentSession).Returns(new DrawingSession(Picture.CreateEmpty(new PictureSize(1, 1))));
        // DrawingSessionViewModel setup (simplified)
        var drawingSessionVM = new DrawingSessionViewModel(mockDrawingSessionProvider.Object);

        var mockClipboard = new Mock<IClipboard>();
        var mockCoordinator = new Mock<IInteractionCoordinator>();
        var mockAddFrameProvider = new Mock<IAddFrameProvider>();

        var copyUseCase = new CopySelectionUseCase(mockClipboard.Object);
        var cutUseCase = new CutSelectionUseCase(mockClipboard.Object);
        var pasteUseCase = new PasteFromClipboardUseCase(mockClipboard.Object, mockDrawingSessionProvider.Object);
        var SelectionClipboard = new SelectionClipboard(copyUseCase, cutUseCase, pasteUseCase);

        var mockPictureRepo = new Mock<IPictureRepository>();
        var mockSettingsRepoForUseCase = new Mock<ISettingsRepository>();
        mockSettingsRepoForUseCase.Setup(x => x.LoadAsync()).ReturnsAsync(new AppSettings());
        var savePictureUseCase = new SavePictureUseCase(mockPictureRepo.Object, mockSettingsRepoForUseCase.Object);
        var loadPictureUseCase = new LoadPictureUseCase(mockPictureRepo.Object, mockSettingsRepoForUseCase.Object);
        var PictureFileIO = new PictureFileIO(savePictureUseCase, loadPictureUseCase);

        // 2. Sub ViewModels Dependencies
        var patternsProvider = new AnimationPatternsProvider();
        var patternEditor = new AnimationPatternEditor(
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider));
        var animationVM = new AnimationViewModel(
            patternsProvider,
            patternEditor,
            new Mock<IFileSystem>().Object,
            new AvaloniaBitmapAdapter());

        var transformUseCase = new Mock<ITransformImageUseCase>();
        var transferToCanvasUseCase = new Mock<ITransferImageToCanvasUseCase>();
        var transferFromCanvasUseCase = new Mock<ITransferImageFromCanvasUseCase>();

        // DrawableCanvasViewModel
        var drawableCanvasVM = new DrawableCanvasViewModel(
            globalState,
            mockAddFrameProvider.Object,
            mockClipboard.Object,
            bitmapAdapter,
            mockDrawingSessionProvider.Object,
            SelectionClipboard,
            mockCoordinator.Object
        );

        var paletteVM = new PaletteContainerViewModel(new Mock<Eede.Application.Infrastructure.IPaletteRepository>().Object, new Mock<Eede.Application.Infrastructure.IPaletteSessionRepository>().Object);
        var mockDrawStyleFactory = new Mock<IDrawStyleFactory>();
        var settingsRepo = new Mock<ISettingsRepository>().Object;
        var loadSettingsUseCase = new LoadSettingsUseCase(settingsRepo);
        var saveSettingsUseCase = new SaveSettingsUseCase(settingsRepo);

        // MainViewModel
        _mainViewModel = new MainViewModel(
            globalState,
            mockClipboard.Object,
            bitmapAdapter,
            mockPictureRepo.Object,
            mockDrawStyleFactory.Object,
            transformUseCase.Object,
            new Mock<IScalingImageUseCase>().Object,
            transferToCanvasUseCase.Object,
            transferFromCanvasUseCase.Object,
            mockDrawingSessionProvider.Object,
            drawableCanvasVM,
            animationVM,
            drawingSessionVM,
            paletteVM,
            PictureFileIO,
            new Mock<IThemeDetector>().Object,
            loadSettingsUseCase,
            saveSettingsUseCase,
            new WelcomeViewModel(mockSettingsRepoForUseCase.Object, new Mock<IExternalBrowserLauncher>().Object),
            () => new DockPictureViewModel(globalState, animationVM, bitmapAdapter, PictureFileIO),
            () => new NewPictureWindowViewModel()
        );

        // DockPictureViewModel
        _dockViewModel = new DockPictureViewModel(globalState, animationVM, bitmapAdapter, PictureFileIO);
        _dockViewModel.Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), new FilePath("test.png"));

        // Setup Window and Container
        _window = new Window();
        _container = new PictureContainer
        {
            DataContext = _dockViewModel
        };
        _window.Content = _container;
    }

    [AvaloniaTest]
    public void Should_UpdateInternalCursorSize_WhenViewModelCursorSizeChanges()
    {
        _window.Show();
        
        // 1. 最初はデフォルト（32x32）であることを確認（SetupDockPicture 等で上書きされる前の初期値）
        var field = typeof(PictureContainer).GetField("_cursorSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var initialSize = (PictureSize)field!.GetValue(_container)!;
        Assert.That(initialSize, Is.EqualTo(new PictureSize(32, 32)));

        // 2. ViewModel のサイズを変更
        _dockViewModel.CursorSize = new PictureSize(64, 64);

        // 3. 内部の _cursorSize が更新されていることを確認
        var updatedSize = (PictureSize)field!.GetValue(_container)!;
        Assert.That(updatedSize, Is.EqualTo(new PictureSize(64, 64)), "ViewModel の CursorSize 変更は即座に PictureContainer に反映されるべき");
    }
    
    [AvaloniaTest]
    public void Should_HaveOpaqueBackground_ToPreventTransparencyBleed()
    {
        // テンプレートの適用とビジュアルツリーの構築を強制
        _container.ApplyTemplate();
        var window = new Avalonia.Controls.Window { Content = _container };
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // ルート背景の検証
        // ドック内での透過漏れを防ぐため、ルート要素は不透明である必要がある
        Assert.That(_container.Opacity, Is.EqualTo(1.0), "PictureContainer は不透明度 1.0 であるべきです");
        
        var background = _container.Background;
        Assert.That(background, Is.Not.Null, "背景色が設定されている必要があります");
        // 1. ルート要素の不透明性検証
        Assert.That(_container.Opacity, Is.EqualTo(1.0), "PictureContainer 自体が不透明である必要があります");
        Assert.That(_container.Background is ISolidColorBrush b && b.Color.A == 255, "ルート背景は不透明である必要があります");

        var allChildren = _container.GetVisualDescendants();
        
        // 2. 外側のタイルシールド（ドックのグレーを遮断する最大の壁）
        var outsideTilePanel = allChildren.OfType<Avalonia.Controls.Panel>().FirstOrDefault(p => p.Name == "outsideTilePanel");
        Assert.That(outsideTilePanel, Is.Not.Null, "outsideTilePanel が見つかりません");
        var outsideBrush = outsideTilePanel!.Background as ISolidColorBrush;
        Assert.That(outsideBrush != null && outsideBrush.Color.A == 255, 
            "outsideTilePanel の背景は不透明であるべきです。タイルの隙間からの透過を防ぎます。");

        // 3. 画像直下のシールド（透過を妨げないよう Transparent であること）
        var renderingRoot = allChildren.OfType<Avalonia.Controls.Panel>().FirstOrDefault(p => p.Name == "renderingRoot");
        Assert.That(renderingRoot, Is.Not.Null, "renderingRoot が見つかりません");
        var rootBrush = renderingRoot!.Background as Avalonia.Media.ISolidColorBrush;
        Assert.That(rootBrush == null || rootBrush.Color == Avalonia.Media.Brushes.Transparent.Color, 
            "renderingRoot の背景は Transparent であるべきです。背後の OutsideBackGround.bmp を透過させて正しい市松模様を表示します。");
    }

    [AvaloniaTest]
    public void Should_Support16xMagnificationButton_AndSyncCheckedState()
    {
        _window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var m16Btn = _container.GetVisualDescendants()
            .OfType<Avalonia.Controls.Primitives.ToggleButton>()
            .FirstOrDefault(b => b.Name == "m16");
        Assert.That(m16Btn, Is.Not.Null, "m16 ボタンが存在するべき");
        Assert.That(m16Btn!.IsChecked, Is.False);

        _dockViewModel.Magnification = new Magnification(16);
        Assert.That(m16Btn.IsChecked, Is.True);

        _dockViewModel.Magnification = new Magnification(8);
        Assert.That(m16Btn.IsChecked, Is.False);

        m16Btn.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Assert.That(_dockViewModel.Magnification.Value, Is.EqualTo(16f));
        Assert.That(m16Btn.IsChecked, Is.True);
    }

    [AvaloniaTest]
    public void MiddleButtonDrag_OnPictureContainer_ShouldPanScrollViewer()
    {
        _dockViewModel.Initialize(Picture.CreateEmpty(new PictureSize(1024, 1024)), new FilePath("large.png"));
        _window.Width = 400;
        _window.Height = 400;
        _window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        _window.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var scrollViewer = _container.GetVisualDescendants().OfType<ScrollViewer>().First(s => s.Name == "scrollViewer");
        var initialOffset = scrollViewer.Offset;

        var startPoint = new Point(200, 200);
        var movePoint = new Point(150, 140);

        _window.MouseDown(startPoint, Avalonia.Input.MouseButton.Middle);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        _window.MouseMove(movePoint, RawInputModifiers.MiddleMouseButton);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        _window.MouseUp(movePoint, Avalonia.Input.MouseButton.Middle);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.That(scrollViewer.Offset.X, Is.EqualTo(initialOffset.X + 50).Within(1.0), "PictureContainer上の中ボタンドラッグでX方向にスクロールするべき");
        Assert.That(scrollViewer.Offset.Y, Is.EqualTo(initialOffset.Y + 60).Within(1.0), "PictureContainer上の中ボタンドラッグでY方向にスクロールするべき");
    }

    [AvaloniaTest]
    public void LargePicture_OnPictureContainer_ShouldAlignTopLeft_BothXAndY()
    {
        _dockViewModel.Initialize(Picture.CreateEmpty(new PictureSize(1024, 1024)), new FilePath("large.png"));
        _window.Width = 400;
        _window.Height = 400;
        _window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        _window.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var scrollViewer = _container.GetVisualDescendants().OfType<ScrollViewer>().First(s => s.Name == "scrollViewer");
        var canvasGrid = _container.GetVisualDescendants().OfType<Grid>().First(g => g.Name == "canvasGrid");

        Assert.That(scrollViewer.Offset.X, Is.EqualTo(canvasGrid.Margin.Left).Within(0.5), "大きい画像の初期Xオフセットは Margin.Left であるべき");
        Assert.That(scrollViewer.Offset.Y, Is.EqualTo(canvasGrid.Margin.Top).Within(0.5), "大きい画像の初期Yオフセットは Margin.Top であるべき");
    }

    [AvaloniaTest]
    public void LargePicture_OpenedInMainView_ShouldAlignTopLeft_BothXAndY()
    {
        var mainView = new Eede.Presentation.Views.Pages.MainView { DataContext = _mainViewModel };
        var win = new Window { Content = mainView, Width = 1024, Height = 720 };
        win.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        win.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        try
        {
            var largeVm = new DockPictureViewModel(new GlobalState(), _mainViewModel.AnimationViewModel, new AvaloniaBitmapAdapter(), new PictureFileIO(new SavePictureUseCase(new Mock<IPictureRepository>().Object, new Mock<ISettingsRepository>().Object), new LoadPictureUseCase(new Mock<IPictureRepository>().Object, new Mock<ISettingsRepository>().Object)));
            largeVm.Initialize(Picture.CreateEmpty(new PictureSize(800, 800)), new FilePath("large800.png"));
            _mainViewModel.Pictures.Add(largeVm);

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            win.UpdateLayout();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var pc = mainView.GetVisualDescendants().OfType<PictureContainer>().FirstOrDefault();
            Assert.That(pc, Is.Not.Null, "PictureContainer should be created in Dock");
            var sv = pc!.GetVisualDescendants().OfType<ScrollViewer>().First(s => s.Name == "scrollViewer");
            var cg = pc.GetVisualDescendants().OfType<Grid>().First(g => g.Name == "canvasGrid");

            Assert.That(sv.Offset.X, Is.EqualTo(cg.Margin.Left).Within(0.5), "Dock内で開いた大きい画像の初期Xオフセットは Margin.Left であるべき");
            Assert.That(sv.Offset.Y, Is.EqualTo(cg.Margin.Top).Within(0.5), "Dock内で開いた大きい画像の初期Yオフセットは Margin.Top であるべき");
        }
        finally
        {
            win.Close();
        }
    }
}






