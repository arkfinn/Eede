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
    public void OnPullToDrawArea_ShouldUpdateMainAndAllDockCursorSizes()
    {
        var mainVM = CreateMainViewModel();
        var dockPictureVM1 = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object);
        var dockPictureVM2 = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object);
        mainVM.Pictures.Add(dockPictureVM1);
        mainVM.Pictures.Add(dockPictureVM2);

        var newSize = new PictureSize(128, 128);
        var area = new PictureArea(new Position(0, 0), newSize);
        var dummyPicture = Picture.CreateEmpty(newSize);
        _transferImageToCanvasUseCaseMock.Setup(x => x.Execute(It.IsAny<Picture>(), It.IsAny<PictureArea>()))
            .Returns(dummyPicture);

        dockPictureVM1.OnPicturePull.Execute(area).Subscribe();

        Assert.That(mainVM.CursorSize, Is.EqualTo(newSize));
        Assert.That(dockPictureVM1.CursorSize, Is.EqualTo(newSize));
        Assert.That(dockPictureVM2.CursorSize, Is.EqualTo(newSize));
        Assert.That(mainVM.DrawableCanvasViewModel.CursorSize, Is.EqualTo(newSize));
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
    public async Task RequestCloseCommand_WhenUserSelectsSaveWithAsyncPictureSave_ShouldSaveAndInvokeCloseWindowInteraction()
    {
        var realBitmapAdapter = new AvaloniaBitmapAdapter();
        var fileStorageMock = new Mock<IFileStorage>();
        using var outStream = new System.IO.MemoryStream();
        bool saveStreamOpened = false;
        fileStorageMock
            .Setup(x => x.OpenWriteStreamAsync(It.IsAny<Uri>()))
            .Returns(async () =>
            {
                await Task.Delay(10);
                saveStreamOpened = true;
                return outStream;
            });

        var mainVM = CreateMainViewModel();
        mainVM.FileStorage = fileStorageMock.Object;

        var editedTab = new DockPictureViewModel(_globalState, _animationViewModel, realBitmapAdapter, _PictureFileIOMock.Object);
        editedTab.Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), new FilePath("edited.png"));
        editedTab.Edited = true;

        editedTab.RequestClose += async (s, e) =>
        {
            editedTab.SaveAlertResult = SaveAlertResult.Save;
            await editedTab.OnClosing.Execute().ToTask();
        };

        mainVM.Pictures.Add(editedTab);

        bool windowCloseInvoked = false;
        mainVM.CloseWindowInteraction.RegisterHandler(interaction =>
        {
            windowCloseInvoked = true;
            interaction.SetOutput(Unit.Default);
        });

        await mainVM.RequestCloseCommand.Execute().ToTask();

        Assert.Multiple(() =>
        {
            Assert.That(saveStreamOpened, Is.True);
            Assert.That(editedTab.Edited, Is.False);
            Assert.That(editedTab.Closable, Is.True);
            Assert.That(mainVM.IsCloseConfirmed, Is.True);
            Assert.That(windowCloseInvoked, Is.True);
        });
    }

    [AvaloniaTest]
    public async Task RequestCloseCommand_WhenUserSelectsSaveButSaveFilePickerIsCanceled_ShouldAbortClosing()
    {
        var realBitmapAdapter = new AvaloniaBitmapAdapter();
        var fileStorageMock = new Mock<IFileStorage>();
        fileStorageMock
            .Setup(x => x.SaveFilePickerAsync())
            .Returns(async () =>
            {
                await Task.Delay(10);
                return (Uri?)null;
            });

        var mainVM = CreateMainViewModel();
        mainVM.FileStorage = fileStorageMock.Object;

        var editedTab = new DockPictureViewModel(_globalState, _animationViewModel, realBitmapAdapter, _PictureFileIOMock.Object);
        editedTab.Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), FilePath.Empty());
        editedTab.Edited = true;

        editedTab.RequestClose += async (s, e) =>
        {
            editedTab.SaveAlertResult = SaveAlertResult.Save;
            await editedTab.OnClosing.Execute().ToTask();
        };

        mainVM.Pictures.Add(editedTab);

        bool windowCloseInvoked = false;
        mainVM.CloseWindowInteraction.RegisterHandler(interaction =>
        {
            windowCloseInvoked = true;
            interaction.SetOutput(Unit.Default);
        });

        await mainVM.RequestCloseCommand.Execute().ToTask();

        Assert.Multiple(() =>
        {
            Assert.That(editedTab.Closable, Is.False);
            Assert.That(editedTab.Edited, Is.True);
            Assert.That(mainVM.IsCloseConfirmed, Is.False);
            Assert.That(windowCloseInvoked, Is.False);
        });
    }

    [AvaloniaTest]
    public async Task OpenPicture_WhenImageHasPalette_AddsPaletteTabToPaletteContainer()
    {
        var extractorMock = new Mock<Eede.Application.Palettes.IImagePaletteExtractor>();
        var expectedPalette = Palette.Create();
        extractorMock
            .Setup(x => x.ExtractAsync(It.IsAny<System.IO.Stream>(), It.IsAny<Picture>(), It.IsAny<string>()))
            .ReturnsAsync(new Eede.Application.Palettes.PaletteExtractionResult(expectedPalette, false));

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
    public async Task OpenPicture_WhenImageHasRepresentativePalette_AddsPaletteTabWithRepresentativeSuffix()
    {
        var extractorMock = new Mock<Eede.Application.Palettes.IImagePaletteExtractor>();
        var expectedPalette = Palette.Create();
        extractorMock
            .Setup(x => x.ExtractAsync(It.IsAny<System.IO.Stream>(), It.IsAny<Picture>(), It.IsAny<string>()))
            .ReturnsAsync(new Eede.Application.Palettes.PaletteExtractionResult(expectedPalette, true));

        var mainVM = CreateMainViewModel(extractorMock.Object);
        var dummyPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        _PictureFileIOMock.Setup(x => x.LoadAsync(It.IsAny<FilePath>())).ReturnsAsync(dummyPicture);

        var storageMock = new Mock<IFileStorage>();
        var dummyUri = new Uri("file:///C:/test/illustration.png");
        storageMock.Setup(x => x.OpenFilePickerAsync()).ReturnsAsync(dummyUri);
        storageMock.Setup(x => x.OpenReadStreamAsync(dummyUri)).ReturnsAsync(new System.IO.MemoryStream([0x89, 0x50]));
        mainVM.FileStorage = storageMock.Object;

        await mainVM.LoadPictureCommand.Execute(storageMock.Object).ToTask();

        Assert.That(_paletteContainerViewModel.Tabs.Count, Is.EqualTo(2));
        var importedTab = _paletteContainerViewModel.Tabs[1];
        Assert.That(importedTab.CustomTitle, Is.EqualTo("illustration.png (代表256色)"));
        Assert.That(importedTab.Palette, Is.EqualTo(expectedPalette));
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
            .ReturnsAsync(new Eede.Application.Palettes.PaletteExtractionResult(expectedPalette, false));

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
            .ReturnsAsync(new Eede.Application.Palettes.PaletteExtractionResult(expectedPalette, false));

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

    [AvaloniaTest]
    public async Task PaletteReplaceColor_WhenTargetColorExistsInCanvas_ReplacesColorAndPushesUndoHistory()
    {
        var color1 = new ArgbColor(255, 255, 0, 0);
        var color2 = new ArgbColor(255, 0, 255, 0);

        byte[] rawPixels = new byte[4 * 4 * 4];
        rawPixels[0] = color2.Blue;
        rawPixels[1] = color2.Green;
        rawPixels[2] = color2.Red;
        rawPixels[3] = color2.Alpha;
        var initialPic = Picture.Create(new PictureSize(4, 4), rawPixels);

        var currentSession = new DrawingSession(initialPic);
        _drawingSessionProviderMock.SetupGet(x => x.CurrentSession).Returns(() => currentSession);
        _drawingSessionProviderMock.Setup(x => x.Update(It.IsAny<DrawingSession>()))
            .Callback<DrawingSession>(s =>
            {
                currentSession = s;
                _drawingSessionProviderMock.Raise(p => p.SessionChanged += null, s);
            });

        var mainVM = CreateMainViewModel();
        mainVM.DrawableCanvasViewModel.SetPicture(initialPic);

        var tab = _paletteContainerViewModel.SelectedTab!;
        tab.Palette = tab.Palette.Apply(0, color1).Apply(1, color2);

        await _paletteContainerViewModel.ReplaceColorCommand.Execute((0, 1)).ToTask();

        Assert.Multiple(() =>
        {
            Assert.That(currentSession.CurrentPicture.PickColor(new Position(0, 0)), Is.EqualTo(color1), "キャンバス上の色2が色1に置換されること");
            Assert.That(currentSession.CanUndo(), Is.True, "Undo履歴が追加されること");
        });

        await mainVM.UndoCommand.Execute().ToTask();
        Assert.That(currentSession.CurrentPicture.PickColor(new Position(0, 0)), Is.EqualTo(color2), "Undoで元の色2に戻ること");

        await mainVM.RedoCommand.Execute().ToTask();
        Assert.That(currentSession.CurrentPicture.PickColor(new Position(0, 0)), Is.EqualTo(color1), "Redoで置換後の色1に戻ること");
    }

    [AvaloniaTest]
    public async Task PaletteReplaceColor_WhenTargetColorDoesNotExistInCanvas_DoesNotPushUndoHistory()
    {
        var color1 = new ArgbColor(255, 255, 0, 0);
        var color2 = new ArgbColor(255, 0, 255, 0);
        var initialPic = Picture.CreateEmpty(new PictureSize(4, 4));

        var currentSession = new DrawingSession(initialPic);
        _drawingSessionProviderMock.SetupGet(x => x.CurrentSession).Returns(() => currentSession);
        _drawingSessionProviderMock.Setup(x => x.Update(It.IsAny<DrawingSession>()))
            .Callback<DrawingSession>(s =>
            {
                currentSession = s;
                _drawingSessionProviderMock.Raise(p => p.SessionChanged += null, s);
            });

        var mainVM = CreateMainViewModel();
        mainVM.DrawableCanvasViewModel.SetPicture(initialPic);

        var tab = _paletteContainerViewModel.SelectedTab!;
        tab.Palette = tab.Palette.Apply(0, color1).Apply(1, color2);

        await _paletteContainerViewModel.ReplaceColorCommand.Execute((0, 1)).ToTask();

        Assert.That(currentSession.CanUndo(), Is.False, "対象色がキャンバスに存在しない場合はUndo履歴が増えないこと");
    }

    [AvaloniaTest]
    public void TilePatternAndColorHistory_ManageSelectionDeduplicationAndMaxLimit()
    {
        var mainVM = CreateMainViewModel();

        // TilePattern selection propagates to DrawableCanvasViewModel
        Assert.That(mainVM.TilePatterns.Count, Is.EqualTo(8));
        mainVM.SelectTilePatternCommand.Execute(TilePattern.Dense62).Subscribe();
        Assert.Multiple(() =>
        {
            Assert.That(mainVM.SelectedTilePattern, Is.EqualTo(TilePattern.Dense62));
            Assert.That(mainVM.DrawableCanvasViewModel.TilePattern, Is.EqualTo(TilePattern.Dense62));
        });

        // ColorHistory records from Eyedropper, Palette fetch, Swap, Background, and Committed draws, up to 16 with LRU dedup
        var c1 = new ArgbColor(255, 10, 0, 0);
        var c2 = new ArgbColor(255, 0, 20, 0);
        var c3 = new ArgbColor(255, 0, 0, 30);

        // 1. Committed draw
        mainVM.PenColor = c1;
        mainVM.DrawableCanvasViewModel.DrawStyle = new FreeCurve();
        var blank = Picture.CreateEmpty(new PictureSize(4, 4));
        var painted = new Drawer(blank, new PenStyle(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), c1, 1))
            .DrawPoint(new Position(0, 0)).Picture;
        var dummyArea = new PictureArea(new Position(0, 0), new PictureSize(4, 4));
        _interactionCoordinatorMock.Raise(
            x => x.Drew += null,
            blank,
            painted,
            dummyArea,
            dummyArea,
            new PictureRegion(new PictureArea(new Position(0, 0), new PictureSize(1, 1))));

        // 2. Eyedropper pick (ColorPicked)
        _interactionCoordinatorMock.Setup(x => x.PointerRightButtonPressed(
            It.IsAny<Position>(),
            It.IsAny<DrawingBuffer>(),
            It.IsAny<IDrawStyle>(),
            It.IsAny<bool>(),
            It.IsAny<PictureSize>(),
            It.IsAny<Action<ArgbColor>>(),
            It.IsAny<ReactiveUI.ReactiveCommand<Picture, ReactiveUI.Primitives.RxVoid>>()))
            .Callback<Position, DrawingBuffer, IDrawStyle, bool, PictureSize, Action<ArgbColor>, ReactiveUI.ReactiveCommand<Picture, ReactiveUI.Primitives.RxVoid>>(
                (_, _, _, _, _, cb, _) => cb(c2));
        mainVM.DrawableCanvasViewModel.PointerRightButtonPressedCommand.Execute(new Position(0, 0)).Subscribe();

        // 3. Palette Fetch
        var tab = _paletteContainerViewModel.SelectedTab!;
        tab.Palette = tab.Palette.Apply(0, c3);
        _paletteContainerViewModel.FetchColorCommand.Execute(0).Subscribe();

        Assert.That(mainVM.ColorHistory, Is.EqualTo(new[] { c3, c2, c1 }));

        // Select from history: updates PenColor without reordering ColorHistory
        mainVM.SelectHistoryColorCommand.Execute(c1).Subscribe();
        Assert.Multiple(() =>
        {
            Assert.That(mainVM.PenColor, Is.EqualTo(c1));
            Assert.That(mainVM.ColorHistory, Is.EqualTo(new[] { c3, c2, c1 }));
        });

        // SwapColors & GetBackgroundColor record foreground color
        var bg = new ArgbColor(255, 40, 50, 60);
        mainVM.CurrentBackgroundColor = new BackgroundColor(bg);
        mainVM.GetBackgroundColorCommand.Execute().Subscribe();
        Assert.That(mainVM.ColorHistory[0], Is.EqualTo(bg));

        // Duplicate color moves to front, max count is 16
        for (int i = 0; i < 20; i++)
        {
            mainVM.DrawableCanvasViewModel.OnColorPicked.Execute(new ArgbColor(255, (byte)(i * 10), 1, 1)).Subscribe();
        }
        Assert.That(mainVM.ColorHistory.Count, Is.EqualTo(MainViewModel.MaxColorHistory));

        // Picking an existing color (i=5 -> R=50) removes old copy and places at index 0 without increasing count
        var dup = new ArgbColor(255, 50, 1, 1);
        mainVM.DrawableCanvasViewModel.OnColorPicked.Execute(dup).Subscribe();
        Assert.Multiple(() =>
        {
            Assert.That(mainVM.ColorHistory.Count, Is.EqualTo(MainViewModel.MaxColorHistory));
            Assert.That(mainVM.ColorHistory[0], Is.EqualTo(dup));
            Assert.That(mainVM.ColorHistory.Count(c => c.EqualsArgb(dup)), Is.EqualTo(1));
        });
    }

    [AvaloniaTest]
    public void PaletteContainer_HostsPaletteSidePanel_WithCompactButtonsAndBoundedHeight()
    {
        var mainVM = CreateMainViewModel();
        mainVM.DrawableCanvasViewModel.OnColorPicked.Execute(new ArgbColor(255, 12, 34, 56)).Subscribe();

        var sidePanel = new Eede.Presentation.Views.DataEntry.PaletteSidePanel
        {
            DataContext = mainVM
        };
        var container = new Eede.Presentation.Views.DataEntry.PaletteContainer
        {
            DataContext = mainVM.PaletteContainerViewModel,
            SideContent = sidePanel
        };
        var window = new Avalonia.Controls.Window
        {
            Content = container,
            Width = 400,
            Height = 300
        };
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        try
        {
            var allButtons = Avalonia.VisualTree.VisualExtensions
                .GetVisualDescendants(sidePanel)
                .OfType<Avalonia.Controls.Button>()
                .ToList();
            var patternButtons = allButtons.Where(b => b.Classes.Contains("tile-pattern-side")).ToList();
            var historyButtons = allButtons.Where(b => b.Classes.Contains("compact-palette-side")).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(patternButtons.Count, Is.EqualTo(8));
                Assert.That(patternButtons.All(b => Math.Abs(b.Bounds.Width - 26) <= 0.5 && Math.Abs(b.Bounds.Height - 26) <= 0.5), Is.True, "Pattern buttons must be 26x26 in 3x3 grid");
                Assert.That(historyButtons.Count, Is.EqualTo(1));
                Assert.That(historyButtons.All(b => b.Bounds.Width <= 16.5 && b.Bounds.Height <= 16.5), Is.True, "History buttons must stay 16x16");
                Assert.That(sidePanel.Bounds.Height, Is.LessThanOrEqualTo(180));
            });

            // Invoking 5th tile pattern button (center cell) switches SelectedTilePattern to Checker50
            patternButtons[4].Command?.Execute(patternButtons[4].CommandParameter);
            Assert.That(mainVM.SelectedTilePattern, Is.EqualTo(TilePattern.Checker50));

            // Toggling horizontal 1px shift button updates SelectedTilePattern, DrawableCanvasViewModel, Swatch.ShiftX, and ShiftIcon.ShiftX
            var shiftToggle = Avalonia.VisualTree.VisualExtensions
                .GetVisualDescendants(sidePanel)
                .OfType<Avalonia.Controls.Primitives.ToggleButton>()
                .FirstOrDefault(b => b.Classes.Contains("tile-pattern-shift"));
            Assert.That(shiftToggle, Is.Not.Null);
            shiftToggle!.IsChecked = true;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var swatches = Avalonia.VisualTree.VisualExtensions
                .GetVisualDescendants(sidePanel)
                .OfType<Eede.Presentation.Views.DataEntry.TilePatternSwatch>()
                .ToList();
            var shiftIcon = Avalonia.VisualTree.VisualExtensions
                .GetVisualDescendants(sidePanel)
                .OfType<Eede.Presentation.Views.DataEntry.TilePatternShiftIcon>()
                .FirstOrDefault();

            Avalonia.Media.IBrush? expectedBorderBrush = Avalonia.Application.Current?.TryGetResource("ToolBorderedBorderChecked", window.ActualThemeVariant, out var borderRes) == true
                ? borderRes as Avalonia.Media.IBrush
                : null;
            var selectedPatternOverlay = Avalonia.VisualTree.VisualExtensions
                .GetVisualDescendants(patternButtons[4])
                .OfType<Avalonia.Controls.Border>()
                .FirstOrDefault(b => b.BorderThickness == new Avalonia.Thickness(2));
            var unselectedPatternOverlay = Avalonia.VisualTree.VisualExtensions
                .GetVisualDescendants(patternButtons[0])
                .OfType<Avalonia.Controls.Border>()
                .FirstOrDefault(b => b.BorderThickness == new Avalonia.Thickness(2));
            var shiftActiveBorder = Avalonia.VisualTree.VisualExtensions
                .GetVisualDescendants(shiftToggle!)
                .OfType<Avalonia.Controls.Border>()
                .FirstOrDefault(b => b.Classes.Contains("shift-active"));

            Assert.Multiple(() =>
            {
                Assert.That(mainVM.IsTilePatternShiftedX, Is.True);
                Assert.That(mainVM.SelectedTilePattern.OffsetX, Is.EqualTo(1));
                Assert.That(mainVM.DrawableCanvasViewModel.TilePattern, Is.EqualTo(TilePattern.Checker50.WithOffsetX(1)));
                Assert.That(swatches.Count, Is.EqualTo(8));
                Assert.That(swatches.All(s => s.ShiftX), Is.True);
                Assert.That(shiftIcon?.ShiftX, Is.True);
                Assert.That(expectedBorderBrush, Is.Not.Null);
                Assert.That(selectedPatternOverlay?.IsVisible, Is.True);
                Assert.That(selectedPatternOverlay?.BorderBrush, Is.EqualTo(expectedBorderBrush));
                Assert.That(unselectedPatternOverlay?.IsVisible, Is.False);
                Assert.That(shiftActiveBorder?.IsVisible, Is.True);
                Assert.That(shiftActiveBorder?.BorderBrush, Is.EqualTo(expectedBorderBrush));
                Assert.That(shiftIcon?.ActiveBackground, Is.Not.Null);
                Assert.That(shiftIcon?.ActiveArrowBrush, Is.Not.Null);
            });

            // Selecting another pattern while shift toggle is active preserves OffsetX == 1
            patternButtons[5].Command?.Execute(patternButtons[5].CommandParameter);
            Assert.That(mainVM.SelectedTilePattern, Is.EqualTo(TilePattern.Sparse37.WithOffsetX(1)));

            // Invoking history color button switches PenColor
            historyButtons[0].Command?.Execute(historyButtons[0].CommandParameter);
            Assert.That(mainVM.PenColor, Is.EqualTo(new ArgbColor(255, 12, 34, 56)));
        }
        finally
        {
            window.Close();
        }
    }
}








