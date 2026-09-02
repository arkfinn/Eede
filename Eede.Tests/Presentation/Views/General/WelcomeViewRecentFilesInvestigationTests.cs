using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.Settings;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
using Eede.Application.UseCase.Settings;
using Eede.Application.UseCase.Updates;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.History;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Files;
using Eede.Presentation.Services;
using Eede.Presentation.Theming;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.General;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.DataDisplay;
using Eede.Presentation.Views.General;
using Eede.Presentation.Views.Pages;
using Moq;
using NUnit.Framework;

namespace Eede.Presentation.Tests.Views.General;

[TestFixture]
public class WelcomeViewRecentFilesInvestigationTests
{
    private Mock<ISettingsRepository> _settingsRepoMock = default!;
    private Mock<IExternalBrowserLauncher> _browserLauncherMock = default!;
    private Mock<IAppUpdater> _appUpdaterMock = default!;
    private AppSettings _appSettings = default!;
    private CheckUpdateUseCase _checkUpdateUseCase = default!;

    [SetUp]
    public void Setup()
    {
        _settingsRepoMock = new Mock<ISettingsRepository>();
        _browserLauncherMock = new Mock<IExternalBrowserLauncher>();
        _appUpdaterMock = new Mock<IAppUpdater>();

        var statusSubject = new BehaviorSubject<UpdateStatus>(UpdateStatus.Idle);
        _appUpdaterMock.SetupGet(s => s.StatusChanged).Returns(statusSubject);
        _appUpdaterMock.SetupGet(s => s.LatestVersion).Returns("1.0.0");

        _appSettings = new AppSettings();
        _appSettings.AddRecentFile("C:\\test\\file1.png", DateTime.Now.AddDays(-1));
        _appSettings.AddRecentFile("C:\\test\\file2.png", DateTime.Now.AddDays(-2));
        _settingsRepoMock.Setup(r => r.LoadAsync()).ReturnsAsync(_appSettings);

        _checkUpdateUseCase = new CheckUpdateUseCase(_appUpdaterMock.Object);
    }

    [AvaloniaTest]
    public async Task WelcomeViewModel_Instantiation_AutoLoadsRecentFiles()
    {
        // WelcomeViewModel 単体初期化時に、自動的に RecentFiles がロードされることを検証
        var welcomeVM = new WelcomeViewModel(_settingsRepoMock.Object, _browserLauncherMock.Object, _appUpdaterMock.Object, _checkUpdateUseCase);

        // 非同期ロードを待機
        for (int i = 0; i < 50; i++)
        {
            if (welcomeVM.RecentFiles.Count > 0) break;
            await Task.Delay(10);
        }

        Assert.That(welcomeVM.RecentFiles.Count, Is.EqualTo(2), "WelcomeViewModel 初期化時に RecentFiles が自動ロードされること");
    }

    [AvaloniaTest]
    public async Task MainViewModel_Constructor_SuccessfullyLoadsRecentFiles()
    {
        // MainViewModel 生成時に Subscribe されて RecentFiles が自動ロードされることを検証
        var (mainVM, _) = CreateMainViewModel();

        // 非同期完了を待機
        for (int i = 0; i < 50; i++)
        {
            if (mainVM.WelcomeViewModel.RecentFiles.Count > 0) break;
            await Task.Delay(10);
        }

        Assert.That(mainVM.WelcomeViewModel.RecentFiles.Count, Is.EqualTo(2),
            "MainViewModel 生成時に RecentFiles が自動ロードされること");
    }

    [AvaloniaTest]
    public async Task Proof2_Fixed_WhenSubscribed_RecentFilesAreLoadedSuccessfully()
    {
        // 修正方針の検証: Subscribe() を付与すれば MainViewModel 生成時に即座にロードされることを検証
        var (mainVM, welcomeVM) = CreateMainViewModel();

        // 修正後の動作をシミュレート: Subscribe() を実行
        welcomeVM.LoadRecentFilesCommand.Execute().Subscribe();

        // 非同期完了を待機
        for (int i = 0; i < 50; i++)
        {
            if (mainVM.WelcomeViewModel.RecentFiles.Count > 0) break;
            await Task.Delay(10);
        }

        Assert.That(mainVM.WelcomeViewModel.RecentFiles.Count, Is.EqualTo(2),
            "Subscribe() を呼ぶことで、非同期ロードが完了し RecentFiles が正常に 2件ロードされること");
    }

    [AvaloniaTest]
    public async Task Proof3_VisualTree_WelcomeView_Displays_RecentFiles_WhenLoaded()
    {
        // 調査観点3: XAML バインディングと ItemsControl のビジュアルツリー描画検証
        var welcomeVM = new WelcomeViewModel(_settingsRepoMock.Object, _browserLauncherMock.Object, _appUpdaterMock.Object, _checkUpdateUseCase);
        
        // 正常にロードされた場合
        await welcomeVM.LoadRecentFilesCommand.Execute().ToTask();

        var welcomeView = new WelcomeView
        {
            DataContext = welcomeVM
        };

        var window = new Window
        {
            Content = welcomeView,
            Width = 800,
            Height = 600
        };
        window.Show();

        Dispatcher.UIThread.RunJobs();
        await Task.Delay(50);
        Dispatcher.UIThread.RunJobs();

        // VisualTree 内の ItemsControl を検索
        var itemsControl = welcomeView.GetVisualDescendants().OfType<ItemsControl>().FirstOrDefault();
        Assert.That(itemsControl, Is.Not.Null, "ItemsControl が存在すること");
        Assert.That(itemsControl!.ItemsSource, Is.EqualTo(welcomeVM.RecentFiles));

        // ItemsControl 内に生成された Button を検索
        var buttons = itemsControl.GetVisualDescendants().OfType<Button>().ToList();
        Assert.That(buttons.Count, Is.EqualTo(2), "2件の RecentFile ボタンが描画されていること");

        var textBlocks = buttons.SelectMany(b => b.GetVisualDescendants().OfType<TextBlock>()).ToList();
        var texts = textBlocks.Select(t => t.Text).ToList();

        Assert.That(texts, Does.Contain("C:\\test\\file1.png"));
        Assert.That(texts, Does.Contain("C:\\test\\file2.png"));

        window.Close();
    }

    [AvaloniaTest]
    public async Task Proof4_PictureFrame_DataContext_Propagation_To_WelcomeView()
    {
        // 調査観点1 & 3: PictureFrame 内の WelcomeView に WelcomeViewModel が正しく伝達されるか？
        var welcomeVM = new WelcomeViewModel(_settingsRepoMock.Object, _browserLauncherMock.Object, _appUpdaterMock.Object, _checkUpdateUseCase);
        await welcomeVM.LoadRecentFilesCommand.Execute().ToTask();

        var pictureFrame = new PictureFrame
        {
            WelcomeViewModel = welcomeVM
        };

        var window = new Window
        {
            Content = pictureFrame,
            Width = 800,
            Height = 600
        };
        window.Show();

        Dispatcher.UIThread.RunJobs();
        await Task.Delay(50);
        Dispatcher.UIThread.RunJobs();

        // PictureFrame のビジュアル子孫から WelcomeView を探索
        var welcomeView = pictureFrame.GetVisualDescendants().OfType<WelcomeView>().FirstOrDefault();
        Assert.That(welcomeView, Is.Not.Null, "PictureFrame 内に WelcomeView が存在すること");
        Assert.That(welcomeView!.DataContext, Is.EqualTo(welcomeVM), "WelcomeView の DataContext が PictureFrame.WelcomeViewModel と一致していること");

        // WelcomeView 内のボタン描画を確認
        var buttons = welcomeView.GetVisualDescendants().OfType<ItemsControl>()
            .SelectMany(ic => ic.GetVisualDescendants().OfType<Button>()).ToList();
        Assert.That(buttons.Count, Is.EqualTo(2), "PictureFrame 経由でも RecentFile のボタンが描画されること");

        window.Close();
    }

    private (MainViewModel, WelcomeViewModel) CreateMainViewModel()
    {
        var globalState = new GlobalState();
        var bitmapAdapter = new AvaloniaBitmapAdapter();
        var mockDrawingSessionProvider = new Mock<IDrawingSessionProvider>();
        mockDrawingSessionProvider.Setup(x => x.CurrentSession).Returns(new DrawingSession(Picture.CreateEmpty(new PictureSize(1, 1))));
        var drawingSessionVM = new DrawingSessionViewModel(mockDrawingSessionProvider.Object);
        var mockClipboard = new Mock<IClipboard>();
        var mockCoordinator = new Mock<IInteractionCoordinator>();
        var mockAddFrameProvider = new Mock<IAddFrameProvider>();

        var copyUseCase = new CopySelectionUseCase(mockClipboard.Object);
        var cutUseCase = new CutSelectionUseCase(mockClipboard.Object);
        var pasteUseCase = new PasteFromClipboardUseCase(mockClipboard.Object, mockDrawingSessionProvider.Object);
        var selectionService = new SelectionService(copyUseCase, cutUseCase, pasteUseCase);

        var mockPictureRepo = new Mock<IPictureRepository>();
        var savePictureUseCase = new SavePictureUseCase(mockPictureRepo.Object, _settingsRepoMock.Object);
        var loadPictureUseCase = new LoadPictureUseCase(mockPictureRepo.Object, _settingsRepoMock.Object);
        var pictureIOService = new PictureIOService(savePictureUseCase, loadPictureUseCase);

        var patternsProvider = new AnimationPatternsProvider();
        var patternService = new AnimationPatternService(
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider));
        var animationVM = new AnimationViewModel(
            patternsProvider,
            patternService,
            new Mock<IFileSystem>().Object,
            new AvaloniaBitmapAdapter());

        var drawableCanvasVM = new DrawableCanvasViewModel(
            globalState,
            mockAddFrameProvider.Object,
            mockClipboard.Object,
            bitmapAdapter,
            mockDrawingSessionProvider.Object,
            selectionService,
            mockCoordinator.Object
        );

        var paletteVM = new PaletteContainerViewModel(new Mock<IPaletteRepository>().Object, new Mock<IPaletteSessionRepository>().Object);
        var loadSettingsUseCase = new LoadSettingsUseCase(_settingsRepoMock.Object);
        var saveSettingsUseCase = new SaveSettingsUseCase(_settingsRepoMock.Object);
        var welcomeVM = new WelcomeViewModel(_settingsRepoMock.Object, _browserLauncherMock.Object, _appUpdaterMock.Object, _checkUpdateUseCase);

        var mainVM = new MainViewModel(
            globalState,
            mockClipboard.Object,
            bitmapAdapter,
            mockPictureRepo.Object,
            new Mock<IDrawStyleFactory>().Object,
            new Mock<ITransformImageUseCase>().Object,
            new Mock<IScalingImageUseCase>().Object,
            new Mock<ITransferImageToCanvasUseCase>().Object,
            new Mock<ITransferImageFromCanvasUseCase>().Object,
            mockDrawingSessionProvider.Object,
            drawableCanvasVM,
            animationVM,
            drawingSessionVM,
            paletteVM,
            pictureIOService,
            new Mock<IThemeDetector>().Object,
            loadSettingsUseCase,
            saveSettingsUseCase,
            welcomeVM,
            () => new DockPictureViewModel(globalState, animationVM, bitmapAdapter, pictureIOService),
            () => new NewPictureWindowViewModel()
        );

        return (mainVM, welcomeVM);
    }
}



