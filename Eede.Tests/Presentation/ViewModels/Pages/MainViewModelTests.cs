using Moq;
using NUnit.Framework;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.General;
using Eede.Presentation.Coordinators;
using Eede.Presentation.Theming;
using Eede.Presentation.Settings;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Avalonia.Media.Imaging;
using Eede.Presentation.Common.Adapters;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Animations;
using Eede.Application.Animations;
using Eede.Application.UseCase.Settings;
using Eede.Domain.Files;
using Eede.Domain.Palettes;
using Eede.Presentation.Common.Enums;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia.Headless.NUnit;

namespace Eede.Presentation.Tests.ViewModels.Pages;

#nullable enable

[TestFixture]
public class MainViewModelTests
{
    private Mock<IClipboard> _clipboardMock = default!;
    private Mock<IBitmapAdapter<Bitmap>> _bitmapAdapterMock = default!;
    private Mock<IPictureRepository> _pictureRepositoryMock = default!;
    private Mock<IDrawStyleFactory> _drawStyleFactoryMock = default!;
    private Mock<ITransformImageUseCase> _transformImageUseCaseMock = default!;
    private Mock<IScalingImageUseCase> _scalingImageUseCaseMock = default!;
    private Mock<ITransferImageToCanvasUseCase> _transferImageToCanvasUseCaseMock = default!;
    private Mock<ITransferImageFromCanvasUseCase> _transferImageFromCanvasUseCaseMock = default!;
    private Mock<IDrawingSessionProvider> _drawingSessionProviderMock = default!;
    private Mock<IPictureFileIO> _PictureFileIOMock = default!;
    private Mock<IInteractionCoordinator> _interactionCoordinatorMock = default!;
    private Mock<IAddFrameProvider> _addFrameProviderMock = default!;
    private Mock<ISelectionClipboard> _SelectionClipboardMock = default!;

    private Mock<IAnimationPatternsProvider> _patternsProviderMock = default!;
    private Mock<IAnimationPatternEditor> _AnimationPatternEditorMock = default!;
    private Mock<IFileSystem> _fileSystemMock = default!;
    private Mock<ISettingsRepository> _settingsRepositoryMock = default!;
    private Mock<ILoadSettingsUseCase> _loadSettingsUseCaseMock = default!;
    private Mock<ISaveSettingsUseCase> _saveSettingsUseCaseMock = default!;
    private Mock<IAppUpdater> _appUpdaterMock = default!;

    private GlobalState _globalState = default!;
    private DrawableCanvasViewModel _drawableCanvasViewModel = default!;
    private AnimationViewModel _animationViewModel = default!;
    private DrawingSessionViewModel _drawingSessionViewModel = default!;
    private PaletteContainerViewModel _paletteContainerViewModel = default!;

    [SetUp]
    public void SetUp()
    {
        _clipboardMock = new Mock<IClipboard>();
        _bitmapAdapterMock = new Mock<IBitmapAdapter<Bitmap>>();
        _pictureRepositoryMock = new Mock<IPictureRepository>();
        _drawStyleFactoryMock = new Mock<IDrawStyleFactory>();
        _transformImageUseCaseMock = new Mock<ITransformImageUseCase>();
        _scalingImageUseCaseMock = new Mock<IScalingImageUseCase>();
        _transferImageToCanvasUseCaseMock = new Mock<ITransferImageToCanvasUseCase>();
        _transferImageFromCanvasUseCaseMock = new Mock<ITransferImageFromCanvasUseCase>();
        _drawingSessionProviderMock = new Mock<IDrawingSessionProvider>();
        var initialPicture = Picture.CreateEmpty(new PictureSize(32, 32));
        _drawingSessionProviderMock.Setup(x => x.CurrentSession).Returns(new DrawingSession(initialPicture));
        _PictureFileIOMock = new Mock<IPictureFileIO>();
        _interactionCoordinatorMock = new Mock<IInteractionCoordinator>();
        _addFrameProviderMock = new Mock<IAddFrameProvider>();
        _SelectionClipboardMock = new Mock<ISelectionClipboard>();
        _patternsProviderMock = new Mock<IAnimationPatternsProvider>();
        _patternsProviderMock.Setup(x => x.Current).Returns(new AnimationPatterns());
        _AnimationPatternEditorMock = new Mock<IAnimationPatternEditor>();
        _fileSystemMock = new Mock<IFileSystem>();
        _settingsRepositoryMock = new Mock<ISettingsRepository>();
        _loadSettingsUseCaseMock = new Mock<ILoadSettingsUseCase>();
        _loadSettingsUseCaseMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(new AppSettings { GridWidth = 32, GridHeight = 32 });
        _saveSettingsUseCaseMock = new Mock<ISaveSettingsUseCase>();
        _appUpdaterMock = new Mock<IAppUpdater>();
        _appUpdaterMock.SetupGet(x => x.StatusChanged).Returns(System.Reactive.Linq.Observable.Return(UpdateStatus.Idle));

        _globalState = new GlobalState();
        _animationViewModel = new AnimationViewModel(_patternsProviderMock.Object, _AnimationPatternEditorMock.Object, _fileSystemMock.Object, new AvaloniaBitmapAdapter());
        _drawingSessionViewModel = new DrawingSessionViewModel(_drawingSessionProviderMock.Object);
        _paletteContainerViewModel = new PaletteContainerViewModel(new Mock<Eede.Application.Infrastructure.IPaletteRepository>().Object, new Mock<Eede.Application.Infrastructure.IPaletteSessionRepository>().Object);

        _drawableCanvasViewModel = new DrawableCanvasViewModel(
            _globalState,
            _addFrameProviderMock.Object,
            _clipboardMock.Object,
            _bitmapAdapterMock.Object,
            _drawingSessionProviderMock.Object,
            _SelectionClipboardMock.Object,
            _interactionCoordinatorMock.Object);
    }

    private MainViewModel CreateMainViewModel(Eede.Application.Palettes.IImagePaletteExtractor? imagePaletteExtractor = null)
    {
        var checkUpdateUseCase = new Eede.Application.UseCase.Updates.CheckUpdateUseCase(_appUpdaterMock.Object);
        var welcomeVM = new WelcomeViewModel(_settingsRepositoryMock.Object, new Mock<IExternalBrowserLauncher>().Object, _appUpdaterMock.Object, checkUpdateUseCase);
        return new MainViewModel(
            _globalState,
            _clipboardMock.Object,
            _bitmapAdapterMock.Object,
            _pictureRepositoryMock.Object,
            _drawStyleFactoryMock.Object,
            _transformImageUseCaseMock.Object,
            _scalingImageUseCaseMock.Object,
            _transferImageToCanvasUseCaseMock.Object,
            _transferImageFromCanvasUseCaseMock.Object,
            _drawingSessionProviderMock.Object,
            _drawableCanvasViewModel,
            _animationViewModel,
            _drawingSessionViewModel,
            _paletteContainerViewModel,
            _PictureFileIOMock.Object,
            new Mock<IThemeDetector>().Object,
            _loadSettingsUseCaseMock.Object,
            _saveSettingsUseCaseMock.Object,
            welcomeVM,
            () => new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object),
            () => null!,
            _appUpdaterMock.Object,
            checkUpdateUseCase,
            imagePaletteExtractor: imagePaletteExtractor);
    }

    [AvaloniaTest]
    public void OnPullToDrawArea_ShouldCallCommitSelectionWithTrue()
    {
        var mainVM = CreateMainViewModel();
        var dockPictureVM = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object);
        mainVM.Pictures.Add(dockPictureVM);

        var area = new PictureArea(new Position(0, 0), new PictureSize(16, 16));
        var dummyPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        _transferImageToCanvasUseCaseMock.Setup(x => x.Execute(It.IsAny<Picture>(), It.IsAny<PictureArea>()))
            .Returns(dummyPicture);

        _interactionCoordinatorMock.Invocations.Clear();

        dockPictureVM.OnPicturePull.Execute(area).Subscribe();

        _interactionCoordinatorMock.Verify(x => x.CommitSelection(true), Times.AtLeastOnce);
    }

    [AvaloniaTest]
    public void OnPushFromDrawArea_ShouldCallCommitSelectionWithTrue()
    {
        var mainVM = CreateMainViewModel();
        var dockPictureVM = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object);
        mainVM.Pictures.Add(dockPictureVM);

        var pos = new Position(10, 10);
        var dummyPicture = Picture.CreateEmpty(new PictureSize(32, 32));
        _transferImageFromCanvasUseCaseMock.Setup(x => x.Execute(It.IsAny<Picture>(), It.IsAny<Picture>(), It.IsAny<Position>(), It.IsAny<Eede.Domain.ImageEditing.Blending.IImageBlender>()))
            .Returns(dummyPicture);

        _interactionCoordinatorMock.Invocations.Clear();

        dockPictureVM.OnPicturePush.Execute(pos).Subscribe();

        _interactionCoordinatorMock.Verify(x => x.CommitSelection(true), Times.AtLeastOnce);
    }

    [AvaloniaTest]
    public void GridFlagsInitialValueAndChangeTest()
    {
        var mainVM = CreateMainViewModel();

        Assert.Multiple(() =>
        {
            Assert.That(mainVM.IsShowPixelGrid, Is.False);
            Assert.That(mainVM.IsShowCursorGrid, Is.False);
        });

        mainVM.IsShowPixelGrid = true;
        Assert.That(mainVM.IsShowPixelGrid, Is.True);

        mainVM.IsShowCursorGrid = true;
        Assert.That(mainVM.IsShowCursorGrid, Is.True);
    }

    [AvaloniaTest]
    public void GridFlagPropagationAndVisibilityIntegrationTest()
    {
        var mainVM = CreateMainViewModel();
        var canvasVM = mainVM.DrawableCanvasViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(canvasVM.IsShowPixelGrid, Is.False);
            Assert.That(canvasVM.IsPixelGridEffectivelyVisible, Is.False);
            Assert.That(canvasVM.Magnification.Value, Is.EqualTo(4.0f));
        });

        mainVM.IsShowPixelGrid = true;
        Assert.Multiple(() =>
        {
            Assert.That(canvasVM.IsShowPixelGrid, Is.True);
            Assert.That(canvasVM.IsPixelGridEffectivelyVisible, Is.True);
        });

        mainVM.Magnification = new Magnification(2);
        Assert.Multiple(() =>
        {
            Assert.That(canvasVM.IsShowPixelGrid, Is.True);
            Assert.That(canvasVM.IsPixelGridEffectivelyVisible, Is.False);
        });

        mainVM.IsShowCursorGrid = true;
        Assert.That(canvasVM.IsShowCursorGrid, Is.True);
        Assert.That(canvasVM.IsCursorGridEffectivelyVisible, Is.True);
    }

    [AvaloniaTest]
    public void CursorSizeInitializationTest()
    {
        var mainVM = CreateMainViewModel();
        var canvasVM = mainVM.DrawableCanvasViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(mainVM.CursorSize.Width, Is.EqualTo(32));
            Assert.That(canvasVM.CursorSize.Width, Is.EqualTo(32));
        });

        mainVM.MinCursorWidth = 16;
        mainVM.MinCursorHeight = 16;
        Assert.Multiple(() =>
        {
            Assert.That(mainVM.CursorSize.Width, Is.EqualTo(16));
            Assert.That(canvasVM.CursorSize.Width, Is.EqualTo(16));
        });
    }

    [AvaloniaTest]
    public void NewDockPicture_ShouldInheritCurrentCursorSize()
    {
        var mainVM = CreateMainViewModel();
        
        // 1. カーソルサイズを変更
        mainVM.MinCursorWidth = 48;
        mainVM.MinCursorHeight = 48;
        var currentSize = new PictureSize(48, 48);
        Assert.That(mainVM.CursorSize, Is.EqualTo(currentSize));

        // 2. 新しい画像を追加（Pictures.Add 時に SetupDockPicture が走ることを期待）
        var newDockVM = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object);
        mainVM.Pictures.Add(newDockVM);

        // 3. 新しい DockPictureViewModel が現在のサイズを引き継いでいることを確認
        Assert.That(newDockVM.CursorSize, Is.EqualTo(currentSize), "新規追加された画像はメインの設定からカーソルサイズを引き継ぐべき");
    }

    [AvaloniaTest]
    public async Task Initialization_ShouldLoadGridSizeFromUseCase()
    {
        var settings = new AppSettings { GridWidth = 48, GridHeight = 64 };
        _loadSettingsUseCaseMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(settings);

        var mainVM = CreateMainViewModel();

        for (int i = 0; i < 50; i++)
        {
            if (mainVM.MinCursorWidth == 48) break;
            await System.Threading.Tasks.Task.Delay(10);
        }

        Assert.Multiple(() =>
        {
            Assert.That(mainVM.MinCursorWidth, Is.EqualTo(48));
            Assert.That(mainVM.MinCursorHeight, Is.EqualTo(64));
        });
    }

    [AvaloniaTest]
    public async Task ChangeGridSize_ShouldCallSaveGridSizeAsync()
    {
        var settings = new AppSettings { GridWidth = 32, GridHeight = 32 };
        _loadSettingsUseCaseMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(settings);

        var mainVM = CreateMainViewModel();

        bool loaded = false;
        for (int i = 0; i < 50; i++)
        {
            if (_loadSettingsUseCaseMock.Invocations.Any(x => x.Method.Name == "ExecuteAsync"))
            {
                loaded = true;
                break;
            }
            await Task.Delay(10);
        }
        Assert.That(loaded, Is.True);

        await Task.Delay(50);

        _saveSettingsUseCaseMock.Invocations.Clear();

        mainVM.MinCursorWidth = 16;
        mainVM.MinCursorHeight = 24;

        bool saved = false;
        for (int i = 0; i < 50; i++)
        {
            if (_saveSettingsUseCaseMock.Invocations.Any(x => x.Method.Name == "ExecuteAsync"))
            {
                saved = true;
                break;
            }
            await Task.Delay(10);
        }

        Assert.That(saved, Is.True);
        _saveSettingsUseCaseMock.Verify(x => x.ExecuteAsync(It.Is<AppSettings>(s => s.GridWidth == 16 && s.GridHeight == 24)), Times.AtLeastOnce);
    }

    [AvaloniaTest]
    public async Task ManualCheckUpdateCommand_ShouldCallCheckForUpdates()
    {
        _appUpdaterMock.Setup(x => x.CheckForUpdatesAsync()).ReturnsAsync(false);
        var mainVM = CreateMainViewModel();

        // WelcomeViewModel の初期化による呼び出しをクリア
        _appUpdaterMock.Invocations.Clear();

        await mainVM.WelcomeViewModel.ManualCheckUpdateCommand.Execute().ToTask();

        _appUpdaterMock.Verify(x => x.CheckForUpdatesAsync(), Times.Once);
    }

    [AvaloniaTest]
    public void IsUpdateReady_ShouldSyncWithUpdater()
    {
        var statusSubject = new System.Reactive.Subjects.BehaviorSubject<UpdateStatus>(UpdateStatus.Idle);
        _appUpdaterMock.SetupGet(x => x.StatusChanged).Returns(statusSubject);
        
        var mainVM = CreateMainViewModel();

        Assert.That(mainVM.IsUpdateReady, Is.False);

        statusSubject.OnNext(UpdateStatus.ReadyToApply);

        Assert.That(mainVM.IsUpdateReady, Is.True);
    }

    [AvaloniaTest]
    public async Task RequestCloseCommand_WhenUserCancelsEditedTab_ShouldAbortClosing()
    {
        var mainVM = CreateMainViewModel();

        // 未編集タブと編集済みタブを用意
        var uneditedTab = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object);
        uneditedTab.Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), FilePath.Empty());
        uneditedTab.Edited = false;

        var editedTab = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object);
        editedTab.Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), FilePath.Empty());
        editedTab.Edited = true;
        // キャンセルをシミュレート（Closable = false）
        editedTab.RequestClose += async (s, e) =>
        {
            editedTab.SaveAlertResult = SaveAlertResult.Cancel;
            await editedTab.ExecuteClosing();
        };

        mainVM.Pictures.Add(uneditedTab);
        mainVM.Pictures.Add(editedTab);

        bool windowCloseInvoked = false;
        mainVM.CloseWindowInteraction.RegisterHandler(interaction =>
        {
            windowCloseInvoked = true;
            interaction.SetOutput(Unit.Default);
        });

        await mainVM.RequestCloseCommand.Execute().ToTask();

        Assert.That(mainVM.IsCloseConfirmed, Is.False);
        Assert.That(windowCloseInvoked, Is.False);
    }

    [AvaloniaTest]
    public async Task RequestCloseCommand_WhenAllTabsConfirmed_ShouldInvokeCloseWindowInteraction()
    {
        var mainVM = CreateMainViewModel();

        var uneditedTab = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object);
        uneditedTab.Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), FilePath.Empty());
        uneditedTab.Edited = false;

        var editedTab = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object);
        editedTab.Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), FilePath.Empty());
        editedTab.Edited = true;
        editedTab.RequestClose += async (s, e) =>
        {
            editedTab.SaveAlertResult = SaveAlertResult.NoSave;
            await editedTab.ExecuteClosing();
        };

        mainVM.Pictures.Add(uneditedTab);
        mainVM.Pictures.Add(editedTab);

        bool windowCloseInvoked = false;
        mainVM.CloseWindowInteraction.RegisterHandler(interaction =>
        {
            windowCloseInvoked = true;
            interaction.SetOutput(Unit.Default);
        });

        await mainVM.RequestCloseCommand.Execute().ToTask();

        Assert.That(mainVM.IsCloseConfirmed, Is.True);
        Assert.That(windowCloseInvoked, Is.True);
    }

    [AvaloniaTest]
    public async Task OpenPicture_WhenImageHasPalette_AddsPaletteTabToPaletteContainer()
    {
        var extractorMock = new Mock<Eede.Application.Palettes.IImagePaletteExtractor>();
        var expectedPalette = Palette.Create();
        extractorMock
            .Setup(x => x.ExtractAsync(It.IsAny<System.IO.Stream>(), It.IsAny<Picture>(), It.IsAny<string>()))
            .ReturnsAsync(expectedPalette);

        var checkUpdateUseCase = new Eede.Application.UseCase.Updates.CheckUpdateUseCase(_appUpdaterMock.Object);
        var welcomeVM = new WelcomeViewModel(_settingsRepositoryMock.Object, new Mock<IExternalBrowserLauncher>().Object, _appUpdaterMock.Object, checkUpdateUseCase);

        var mainVM = new MainViewModel(
            _globalState,
            _clipboardMock.Object,
            _bitmapAdapterMock.Object,
            _pictureRepositoryMock.Object,
            _drawStyleFactoryMock.Object,
            _transformImageUseCaseMock.Object,
            _scalingImageUseCaseMock.Object,
            _transferImageToCanvasUseCaseMock.Object,
            _transferImageFromCanvasUseCaseMock.Object,
            _drawingSessionProviderMock.Object,
            _drawableCanvasViewModel,
            _animationViewModel,
            _drawingSessionViewModel,
            _paletteContainerViewModel,
            _PictureFileIOMock.Object,
            new Mock<IThemeDetector>().Object,
            _loadSettingsUseCaseMock.Object,
            _saveSettingsUseCaseMock.Object,
            welcomeVM,
            () => new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object),
            () => null!,
            _appUpdaterMock.Object,
            checkUpdateUseCase,
            imagePaletteExtractor: extractorMock.Object);

        var dummyPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        _PictureFileIOMock.Setup(x => x.LoadAsync(It.IsAny<FilePath>())).ReturnsAsync(dummyPicture);

        var storageMock = new Mock<IFileStorage>();
        var dummyUri = new Uri("file:///C:/test/hero.png");
        storageMock.Setup(x => x.OpenFilePickerAsync()).ReturnsAsync(dummyUri);
        storageMock.Setup(x => x.OpenReadStreamAsync(dummyUri)).ReturnsAsync(new System.IO.MemoryStream([0x89, 0x50]));
        mainVM.FileStorage = storageMock.Object;

        await mainVM.LoadPictureCommand.Execute(storageMock.Object).ToTask();

        Assert.That(_paletteContainerViewModel.Tabs.Count, Is.EqualTo(2), "パレットタブが追加されること");
        var importedTab = _paletteContainerViewModel.Tabs[1];
        Assert.That(importedTab.CustomTitle, Is.EqualTo("hero.png"));
        Assert.That(importedTab.Palette, Is.EqualTo(expectedPalette));
        Assert.That(_paletteContainerViewModel.SelectedTab, Is.EqualTo(importedTab), "インポートされたタブが選択されること");
    }

    [AvaloniaTest]
    public async Task LoadPictureCommand_SupportsNonFileUri_InBrowserWasmEnvironment()
    {
        var mainVM = CreateMainViewModel();
        var dummyPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        _PictureFileIOMock.Setup(x => x.LoadAsync(It.IsAny<FilePath>())).ReturnsAsync(dummyPicture);

        var storageMock = new Mock<IFileStorage>();
        var blobUri = new Uri("blob:http://localhost:5000/550e8400-e29b-41d4-a716-446655440000");
        storageMock.Setup(x => x.OpenFilePickerAsync()).ReturnsAsync(blobUri);
        mainVM.FileStorage = storageMock.Object;

        await mainVM.LoadPictureCommand.Execute(storageMock.Object).ToTask();

        Assert.That(mainVM.Pictures.Count, Is.EqualTo(1), "非ファイルURI（WASM/blob）でもピクチャが正しく読み込まれること");
    }

    [AvaloniaTest]
    public async Task LoadPictureCommand_WhenOpeningWebBrowserImage_AutoExtractsPaletteFromCache()
    {
        var extractorMock = new Mock<Eede.Application.Palettes.IImagePaletteExtractor>();
        var expectedPalette = Palette.Create();
        extractorMock.Setup(x => x.ExtractAsync(It.IsAny<Stream>(), It.IsAny<Picture>(), ".png"))
            .ReturnsAsync(expectedPalette);

        var mainVM = CreateMainViewModel(extractorMock.Object);
        var dummyPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        _PictureFileIOMock.Setup(x => x.LoadAsync(It.IsAny<FilePath>())).ReturnsAsync(dummyPicture);

        // Web仮想ファイル（blob:）をキャッシュに登録
        var blobUri = new Uri("blob:http://localhost:5000/character_sheet.png");
        var storageFileMock = new Mock<Avalonia.Platform.Storage.IStorageFile>();
        storageFileMock.SetupGet(f => f.Path).Returns(blobUri);
        storageFileMock.SetupGet(f => f.Name).Returns("character_sheet.png");
        storageFileMock.Setup(f => f.OpenReadAsync()).ReturnsAsync(new MemoryStream([1, 2, 3]));
        AvaloniaFileStorage.CacheFile(storageFileMock.Object);

        var storageMock = new Mock<IFileStorage>();
        storageMock.Setup(x => x.OpenFilePickerAsync()).ReturnsAsync(blobUri);

        await mainVM.LoadPictureCommand.Execute(storageMock.Object).ToTask();

        Assert.That(mainVM.Pictures.Count, Is.EqualTo(1));
        Assert.That(_paletteContainerViewModel.Tabs.Count, Is.EqualTo(2), "Web版でもパレットが自動抽出されてタブが追加されること");
        var importedTab = _paletteContainerViewModel.Tabs[1];
        Assert.That(importedTab.CustomTitle, Is.EqualTo("character_sheet.png"));
        Assert.That(importedTab.Palette, Is.EqualTo(expectedPalette));
    }

    [AvaloniaTest]
    public async Task LoadPictureCommand_WhenOpeningGuidBlobUri_WithSingleStreamRead_AutoExtractsPaletteAndRestoresOriginalFileName()
    {
        var extractorMock = new Mock<Eede.Application.Palettes.IImagePaletteExtractor>();
        var expectedPalette = Palette.Create();
        extractorMock.Setup(x => x.ExtractAsync(It.IsAny<Stream>(), It.IsAny<Picture>(), ".png"))
            .ReturnsAsync(expectedPalette);

        var mainVM = CreateMainViewModel(extractorMock.Object);
        var dummyPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        _PictureFileIOMock.Setup(x => x.LoadAsync(It.IsAny<FilePath>())).ReturnsAsync(dummyPicture);

        // Web仮想ファイル: URLはGUIDのみ（拡張子なし）、Nameは "character_sheet.png"
        var guidBlobUri = new Uri("blob:http://localhost:5000/3e9b16f3-13bb-4573-8a39-55e1db320f2b");
        var storageFileMock = new Mock<Avalonia.Platform.Storage.IStorageFile>();
        storageFileMock.SetupGet(f => f.Path).Returns(guidBlobUri);
        storageFileMock.SetupGet(f => f.Name).Returns("character_sheet.png");

        // 1回目の OpenReadAsync() は正常、2回目はブラウザ制限を模して InvalidOperationException をスロー
        int readCallCount = 0;
        storageFileMock.Setup(f => f.OpenReadAsync()).Returns(() =>
        {
            readCallCount++;
            if (readCallCount > 1)
            {
                throw new InvalidOperationException("Browser stream cannot be reopened.");
            }
            return Task.FromResult<Stream>(new MemoryStream([1, 2, 3]));
        });

        AvaloniaFileStorage.CacheFile(storageFileMock.Object);

        // PictureRepository で 1 回ストリームを読み込ませる（キャッシュ化トリガー）
        var firstStream = await AvaloniaFileStorage.TryOpenReadStreamStaticAsync(guidBlobUri.ToString());
        Assert.That(firstStream, Is.Not.Null);
        await firstStream!.DisposeAsync();

        // 2回目の読み込み（TryExtractPaletteFromImageAsync）がメモリキャッシュから成功することを確認
        var storageMock = new Mock<IFileStorage>();
        storageMock.Setup(x => x.OpenFilePickerAsync()).ReturnsAsync(guidBlobUri);

        await mainVM.LoadPictureCommand.Execute(storageMock.Object).ToTask();

        Assert.That(mainVM.Pictures.Count, Is.EqualTo(1));
        Assert.That(_paletteContainerViewModel.Tabs.Count, Is.EqualTo(2), "GUID blob URI でもパレットが抽出されてタブが追加されること");
        var importedTab = _paletteContainerViewModel.Tabs[1];
        Assert.That(importedTab.CustomTitle, Is.EqualTo("character_sheet.png"), "元のファイル名がタブタイトルに復元されること");
        Assert.That(importedTab.Palette, Is.EqualTo(expectedPalette));
    }
}








