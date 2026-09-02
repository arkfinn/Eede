#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.Recovery;
using Eede.Application.Settings;
using Eede.Application.UseCase.Pictures;
using Eede.Application.UseCase.Settings;
using Eede.Application.UseCase.Updates;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Recovery;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Pictures;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.General;
using Eede.Presentation.ViewModels.Pages;
using Eede.Tests.Application.Recovery;
using Moq;
using NUnit.Framework;

namespace Eede.Tests.Presentation.ViewModels.Pages;

[TestFixture]
public class SessionRecoveryE2ETests
{
    private InMemorySessionStorage _storage = default!;
    private SkiaSharpPictureCodec _codec = default!;
    private PullContextTracker _pullTracker = default!;
    private SessionRecoverer _recoverer = default!;
    private SessionRecoveryCoordinator _coordinator = default!;

    private GlobalState _globalState = default!;
    private Mock<IClipboard> _clipboardMock = default!;
    private Mock<IBitmapAdapter<Bitmap>> _bitmapAdapterMock = default!;
    private Mock<IPictureRepository> _pictureRepositoryMock = default!;
    private Mock<IDrawStyleFactory> _drawStyleFactoryMock = default!;
    private Mock<ITransformImageUseCase> _transformImageUseCaseMock = default!;
    private Mock<IScalingImageUseCase> _scalingImageUseCaseMock = default!;
    private TransferImageToCanvasUseCase _transferImageToCanvasUseCase = default!;
    private TransferImageFromCanvasUseCase _transferImageFromCanvasUseCase = default!;
    private DrawingSessionProvider _drawingSessionProvider = default!;
    private Mock<IPictureIOService> _pictureIOServiceMock = default!;
    private Mock<IThemeService> _themeServiceMock = default!;
    private Mock<ILoadSettingsUseCase> _loadSettingsUseCaseMock = default!;
    private Mock<ISaveSettingsUseCase> _saveSettingsUseCaseMock = default!;
    private Mock<IAppUpdater> _appUpdaterMock = default!;
    private Mock<IAddFrameProvider> _addFrameProviderMock = default!;
    private Mock<ISelectionService> _selectionServiceMock = default!;
    private Mock<IInteractionCoordinator> _interactionCoordinatorMock = default!;

    private DrawableCanvasViewModel _drawableCanvasViewModel = default!;
    private AnimationViewModel _animationViewModel = default!;
    private DrawingSessionViewModel _drawingSessionViewModel = default!;
    private PaletteContainerViewModel _paletteContainerViewModel = default!;

    [SetUp]
    public void SetUp()
    {
        _storage = new InMemorySessionStorage();
        _codec = new SkiaSharpPictureCodec();
        _pullTracker = new PullContextTracker();
        _recoverer = new SessionRecoverer(_storage, _codec);
        _coordinator = new SessionRecoveryCoordinator(_storage, _codec, null);

        _globalState = new GlobalState();
        _clipboardMock = new Mock<IClipboard>();
        _bitmapAdapterMock = new Mock<IBitmapAdapter<Bitmap>>();
        _pictureRepositoryMock = new Mock<IPictureRepository>();
        _drawStyleFactoryMock = new Mock<IDrawStyleFactory>();
        _transformImageUseCaseMock = new Mock<ITransformImageUseCase>();
        _scalingImageUseCaseMock = new Mock<IScalingImageUseCase>();
        _transferImageToCanvasUseCase = new TransferImageToCanvasUseCase();
        _transferImageFromCanvasUseCase = new TransferImageFromCanvasUseCase();

        var initialPicture = Picture.CreateEmpty(new PictureSize(32, 32));
        _drawingSessionProvider = new DrawingSessionProvider();
        _drawingSessionProvider.Update(new DrawingSession(initialPicture));

        _pictureIOServiceMock = new Mock<IPictureIOService>();
        _themeServiceMock = new Mock<IThemeService>();
        _loadSettingsUseCaseMock = new Mock<ILoadSettingsUseCase>();
        _loadSettingsUseCaseMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(new AppSettings { GridWidth = 32, GridHeight = 32 });
        _saveSettingsUseCaseMock = new Mock<ISaveSettingsUseCase>();
        _appUpdaterMock = new Mock<IAppUpdater>();
        _appUpdaterMock.SetupGet(x => x.StatusChanged).Returns(Observable.Return(UpdateStatus.Idle));
        _addFrameProviderMock = new Mock<IAddFrameProvider>();
        _selectionServiceMock = new Mock<ISelectionService>();
        _interactionCoordinatorMock = new Mock<IInteractionCoordinator>();

        var patternsProviderMock = new Mock<IAnimationPatternsProvider>();
        patternsProviderMock.Setup(x => x.Current).Returns(new AnimationPatterns());

        _animationViewModel = new AnimationViewModel(
            patternsProviderMock.Object,
            new Mock<IAnimationPatternService>().Object,
            new Mock<IFileSystem>().Object,
            new AvaloniaBitmapAdapter());

        _drawingSessionViewModel = new DrawingSessionViewModel(_drawingSessionProvider);
        _paletteContainerViewModel = new PaletteContainerViewModel(
            new Mock<IPaletteRepository>().Object,
            new Mock<IPaletteSessionRepository>().Object);

        _drawableCanvasViewModel = new DrawableCanvasViewModel(
            _globalState,
            _addFrameProviderMock.Object,
            _clipboardMock.Object,
            _bitmapAdapterMock.Object,
            _drawingSessionProvider,
            _selectionServiceMock.Object,
            _interactionCoordinatorMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _coordinator.Dispose();
    }

    private MainViewModel CreateMainViewModel()
    {
        var checkUpdateUseCase = new CheckUpdateUseCase(_appUpdaterMock.Object);
        var settingsRepoMock = new Mock<ISettingsRepository>();
        settingsRepoMock.Setup(x => x.LoadAsync()).ReturnsAsync(new AppSettings());

        var welcomeVM = new WelcomeViewModel(
            settingsRepoMock.Object,
            new Mock<IExternalBrowserLauncher>().Object,
            _appUpdaterMock.Object,
            checkUpdateUseCase);

        var sessionProvider = new DrawingSessionProvider();
        var interactionCoord = new InteractionCoordinator(sessionProvider);
        var canvasVM = new DrawableCanvasViewModel(
            _globalState,
            _addFrameProviderMock.Object,
            _clipboardMock.Object,
            _bitmapAdapterMock.Object,
            sessionProvider,
            _selectionServiceMock.Object,
            interactionCoord);
        var drawingSessionVM = new DrawingSessionViewModel(sessionProvider);

        return new MainViewModel(
            _globalState,
            _clipboardMock.Object,
            _bitmapAdapterMock.Object,
            _pictureRepositoryMock.Object,
            _drawStyleFactoryMock.Object,
            _transformImageUseCaseMock.Object,
            _scalingImageUseCaseMock.Object,
            _transferImageToCanvasUseCase,
            _transferImageFromCanvasUseCase,
            sessionProvider,
            canvasVM,
            _animationViewModel,
            drawingSessionVM,
            _paletteContainerViewModel,
            _pictureIOServiceMock.Object,
            _themeServiceMock.Object,
            _loadSettingsUseCaseMock.Object,
            _saveSettingsUseCaseMock.Object,
            welcomeVM,
            () => new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _pictureIOServiceMock.Object),
            () => null!,
            _appUpdaterMock.Object,
            checkUpdateUseCase,
            _pullTracker,
            _coordinator,
            _recoverer,
            _storage);
    }

    [AvaloniaTest]
    public async Task RestoreRecovery_FullyRestoresTabsEditedStatePullAndPalette()
    {
        // 1. Arrange: クラッシュセッションデータを事前にストレージへ用意
        var doc1Id = "doc-tab-1";
        var doc2Id = "doc-tab-2";

        // タブ1: 32x32、Edited=true、左上に赤色 (255, 255, 0, 0)
        var doc1Bytes = new byte[32 * 32 * 4];
        doc1Bytes[0] = 0;   // B
        doc1Bytes[1] = 0;   // G
        doc1Bytes[2] = 255; // R
        doc1Bytes[3] = 255; // A
        var doc1Picture = Picture.Create(new PictureSize(32, 32), doc1Bytes);
        var redColor = new ArgbColor(255, 255, 0, 0);
        var doc1PayloadRef = $"doc_{doc1Id}.png";
        var doc1Snapshot = new DocumentSnapshot(doc1Id, null, true, doc1Picture.Size, 2.0f, doc1PayloadRef);

        // タブ2: 64x64、Edited=false、オリジナルパスあり、バイナリ退避なし
        var doc2Picture = Picture.CreateEmpty(new PictureSize(64, 64));
        var doc2Path = "C:/test/file2.png";
        var doc2Snapshot = new DocumentSnapshot(doc2Id, doc2Path, false, doc2Picture.Size, 1.0f, null);

        // Pull状態: タブ1の (8, 8) から 16x16 を Pull。キャンバス上に青色 (255, 0, 0, 255)
        var canvasBytes = new byte[16 * 16 * 4];
        canvasBytes[0] = 255; // B
        canvasBytes[1] = 0;   // G
        canvasBytes[2] = 0;   // R
        canvasBytes[3] = 255; // A
        var canvasPicture = Picture.Create(new PictureSize(16, 16), canvasBytes);
        var blueColor = new ArgbColor(255, 0, 0, 255);
        var canvasPayloadRef = "canvas_pull.png";
        var pullArea = new PictureArea(new Position(8, 8), new PictureSize(16, 16));
        var pullSnapshot = new PullSnapshot(doc1Id, pullArea, true, canvasPayloadRef);

        // パレット状態: 選択色 (255, 12, 34, 56)、インデックス 5 に黄色 (255, 255, 255, 0)
        var selectedColor = new ArgbColor(255, 12, 34, 56);
        var paletteColors = new ArgbColor[Palette.MAX_LENGTH];
        for (int i = 0; i < Palette.MAX_LENGTH; i++)
        {
            paletteColors[i] = new ArgbColor(0, 0, 0, 0);
        }
        var yellowColor = new ArgbColor(255, 255, 255, 0);
        paletteColors[5] = yellowColor;
        var paletteSnapshot = new PaletteSnapshot(selectedColor, 0, paletteColors);

        var sessionSnapshot = new SessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            doc1Id,
            new[] { doc1Snapshot, doc2Snapshot },
            pullSnapshot,
            paletteSnapshot);

        var payloads = new Dictionary<string, byte[]>
        {
            [doc1PayloadRef] = _codec.EncodeToPng(doc1Picture),
            [canvasPayloadRef] = _codec.EncodeToPng(canvasPicture)
        };

        await _storage.SaveSnapshotAsync(sessionSnapshot, payloads);

        // 2. Act: MainViewModel を初期化
        var mainVM = CreateMainViewModel();
        await mainVM.InitializeAsync();

        // 3. Assert (起動時チェック): WelcomeViewModel にセッション再開情報が表示される
        Assert.That(mainVM.WelcomeViewModel.HasPreviousSession, Is.True);
        Assert.That(mainVM.WelcomeViewModel.PreviousSessionDescription, Does.Contain("2 件のファイル"));
        Assert.That(mainVM.Pictures, Is.Empty);

        // 4. Act: WelcomeViewModel から再開コマンドを実行
        await mainVM.WelcomeViewModel.ResumeLastSessionCommand.Execute().ToTask();

        // 5. Assert (復元結果):
        // 再開カード非表示
        Assert.That(mainVM.WelcomeViewModel.HasPreviousSession, Is.False);

        // タブ数
        Assert.That(mainVM.Pictures.Count, Is.EqualTo(2));

        // タブ1 (未保存・画像復元)
        var restoredTab1 = mainVM.Pictures.First(p => p.Id == doc1Id);
        Assert.That(restoredTab1.Edited, Is.True);
        Assert.That(restoredTab1.PictureBuffer.Size, Is.EqualTo(new PictureSize(32, 32)));
        Assert.That(restoredTab1.Magnification.Value, Is.EqualTo(2.0f));
        Assert.That(restoredTab1.PictureBuffer.PickColor(new Position(0, 0)), Is.EqualTo(redColor));

        // タブ2 (保存済み・空画像再生成)
        var restoredTab2 = mainVM.Pictures.First(p => p.Id == doc2Id);
        Assert.That(restoredTab2.Edited, Is.False);
        Assert.That(restoredTab2.FilePath.ToString(), Is.EqualTo(doc2Path));
        Assert.That(restoredTab2.PictureBuffer.Size, Is.EqualTo(new PictureSize(64, 64)));
        Assert.That(restoredTab2.Magnification.Value, Is.EqualTo(1.0f));

        // Pull状態
        Assert.That(_pullTracker.CurrentContext, Is.Not.Null);
        Assert.That(_pullTracker.CurrentContext!.SourceDocumentId, Is.EqualTo(doc1Id));
        Assert.That(_pullTracker.CurrentContext!.SourceArea, Is.EqualTo(pullArea));
        Assert.That(mainVM.DrawableCanvasViewModel.PictureBuffer.Previous.PickColor(new Position(0, 0)), Is.EqualTo(blueColor));

        // パレット状態
        Assert.That(mainVM.PenColor, Is.EqualTo(selectedColor));
        Assert.That(mainVM.PaletteContainerViewModel.SelectedTab, Is.Not.Null);
        Assert.That(mainVM.PaletteContainerViewModel.SelectedTab!.Palette.Fetch(5), Is.EqualTo(yellowColor));

        // ストレージがクリアされていること
        Assert.That(await _storage.HasActiveSessionAsync(), Is.False);
    }

    [AvaloniaTest]
    public async Task DiscardRecovery_HidesPromptAndClearsStorage()
    {
        // 1. Arrange: クラッシュセッションデータを用意
        var docSnapshot = new DocumentSnapshot("doc-1", null, true, new PictureSize(16, 16), 1.0f, null);
        var paletteSnapshot = new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, new ArgbColor[256]);
        var sessionSnapshot = new SessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "doc-1",
            new[] { docSnapshot },
            null,
            paletteSnapshot);

        await _storage.SaveSnapshotAsync(sessionSnapshot, new Dictionary<string, byte[]>());

        // 2. Act: MainViewModel を初期化
        var mainVM = CreateMainViewModel();
        await mainVM.InitializeAsync();

        Assert.That(mainVM.WelcomeViewModel.HasPreviousSession, Is.True);

        // 3. Act: 破棄コマンドを実行
        await mainVM.WelcomeViewModel.DiscardLastSessionCommand.Execute().ToTask();

        // 4. Assert: プロンプトが非表示になり、ストレージがクリアされている
        Assert.That(mainVM.WelcomeViewModel.HasPreviousSession, Is.False);
        Assert.That(mainVM.Pictures, Is.Empty);
        Assert.That(await _storage.HasActiveSessionAsync(), Is.False);
    }

    [AvaloniaTest]
    public async Task CleanExit_ShowsResumeOptionOnWelcomeView_AndCanRestore()
    {
        // 1. Arrange: 正常終了時のセッションデータを用意 (clean_exit.marker あり)
        var docSnapshot = new DocumentSnapshot("doc-1", null, true, new PictureSize(16, 16), 1.0f, null);
        var paletteSnapshot = new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, new ArgbColor[256]);
        var sessionSnapshot = new SessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "doc-1",
            new[] { docSnapshot },
            null,
            paletteSnapshot);

        await _storage.SaveSnapshotAsync(sessionSnapshot, new Dictionary<string, byte[]>());
        await _storage.MarkCleanExitAsync(); // 正常終了マーク！

        // 2. Act: MainViewModel を初期化
        var mainVM = CreateMainViewModel();
        await mainVM.InitializeAsync();

        // 3. Assert: 正常終了後でも WelcomeViewModel に「前回の作業を再開」が表示される
        Assert.That(mainVM.WelcomeViewModel.HasPreviousSession, Is.True);
        Assert.That(mainVM.WelcomeViewModel.IsCrashRecovery, Is.False);
        Assert.That(mainVM.WelcomeViewModel.PreviousSessionTitle, Is.EqualTo("前回の作業を再開"));

        // 4. Act: 再開コマンドを実行
        await mainVM.WelcomeViewModel.ResumeLastSessionCommand.Execute().ToTask();

        // 5. Assert: 正常にタブが復元される
        Assert.That(mainVM.Pictures.Count, Is.EqualTo(1));
        Assert.That(mainVM.WelcomeViewModel.HasPreviousSession, Is.False);
    }

    [AvaloniaTest]
    public async Task PullAndPushTracking_LifecycleWorksCorrectly()
    {
        var mainVM = CreateMainViewModel();

        var dummyPicture = Picture.CreateEmpty(new PictureSize(32, 32));
        var dockVm = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _pictureIOServiceMock.Object);
        dockVm.Initialize(dummyPicture, FilePath.Empty());
        mainVM.Pictures.Add(dockVm);

        // 1. Pull: ドックからキャンバスへ読み込み
        var pullArea = new PictureArea(new Position(4, 4), new PictureSize(16, 16));
        await dockVm.OnPicturePull.Execute(pullArea).ToTask();

        Assert.That(_pullTracker.CurrentContext, Is.Not.Null);
        Assert.That(_pullTracker.CurrentContext!.SourceDocumentId, Is.EqualTo(dockVm.Id));
        Assert.That(_pullTracker.CurrentContext!.SourceArea, Is.EqualTo(pullArea));

        // 2. Push: キャンバスからドックへ書き戻し
        await dockVm.OnPicturePush.Execute(new Position(4, 4)).ToTask();

        Assert.That(_pullTracker.CurrentContext, Is.Null);

        // 3. 再度 Pull してからドキュメントを閉じる
        await dockVm.OnPicturePull.Execute(pullArea).ToTask();
        Assert.That(_pullTracker.CurrentContext, Is.Not.Null);

        mainVM.Pictures.Remove(dockVm);
        Assert.That(_pullTracker.CurrentContext, Is.Null);
    }

    [AvaloniaTest]
    public async Task CaptureSession_CapturesCurrentViewModelStateCorrectly()
    {
        var mainVM = CreateMainViewModel();

        var pic1 = Picture.CreateEmpty(new PictureSize(32, 32));
        var tab1 = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _pictureIOServiceMock.Object);
        tab1.Initialize(pic1, FilePath.Empty());
        tab1.Edited = true;

        var pic2 = Picture.CreateEmpty(new PictureSize(16, 16));
        var tab2 = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _pictureIOServiceMock.Object);
        tab2.Initialize(pic2, new FilePath("C:/saved.png"));
        tab2.Edited = false;

        mainVM.Pictures.Add(tab1);
        mainVM.Pictures.Add(tab2);

        var capture = mainVM.CaptureSession();

        Assert.That(capture, Is.Not.Null);
        Assert.That(capture!.Snapshot.Documents.Count, Is.EqualTo(2));

        // tab1 は Edited=true なのでペイロード参照あり & Pictures 辞書に格納
        var doc1 = capture.Snapshot.Documents.First(d => d.DocumentId == tab1.Id);
        Assert.That(doc1.IsEdited, Is.True);
        Assert.That(doc1.ImagePayloadRef, Is.Not.Null);
        Assert.That(capture.Pictures.ContainsKey(doc1.ImagePayloadRef!), Is.True);

        // tab2 は Edited=false なのでペイロード参照なし
        var doc2 = capture.Snapshot.Documents.First(d => d.DocumentId == tab2.Id);
        Assert.That(doc2.IsEdited, Is.False);
        Assert.That(doc2.OriginalFilePath, Is.EqualTo("C:/saved.png"));
        Assert.That(doc2.ImagePayloadRef, Is.Null);
    }

    [AvaloniaTest]
    public async Task CleanExit_MarksCleanExitAndNoRecoveryPromptOnNextLaunch()
    {
        // 1. Arrange: MainViewModel を作成し、タブを追加
        var mainVM = CreateMainViewModel();
        mainVM.CloseWindowInteraction.RegisterHandler(c => c.SetOutput(System.Reactive.Unit.Default));

        var pic = Picture.CreateEmpty(new PictureSize(16, 16));
        var tab = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _pictureIOServiceMock.Object);
        tab.Initialize(pic, FilePath.Empty());
        tab.Edited = false;
        mainVM.Pictures.Add(tab);

        // 2. Act: ウィンドウのクローズ要求を実行 (正常終了)
        await mainVM.RequestCloseCommand.Execute().ToTask();

        // 3. Assert: クリーン終了マーカーが記録されていること
        Assert.That(_storage.IsCleanExitMarked, Is.True);

        // 4. 次回起動: 同じストレージを用いて再度 MainViewModel を初期化
        var nextMainVM = CreateMainViewModel();
        await nextMainVM.InitializeAsync();

        // 5. Assert: リカバリプロンプトは表示されない
        Assert.That(nextMainVM.IsRecoveryPromptVisible, Is.False);
    }

    [AvaloniaTest]
    public async Task PullToCanvas_RestoresCanvasPictureAndSessionCorrectly()
    {
        // 1. Arrange: 復元用セッションデータを作成 (ドキュメント + キャンバスPullスナップショット)
        var redColor = new ArgbColor(255, 255, 0, 0);
        var blueColor = new ArgbColor(255, 0, 0, 255);
        var pullArea = new PictureArea(new Position(0, 0), new PictureSize(8, 8));

        var docPicture = CreateFilledPicture(new PictureSize(32, 32), redColor);
        var canvasPicture = CreateFilledPicture(new PictureSize(8, 8), blueColor);

        var docSnapshot = new DocumentSnapshot("doc-1", null, true, new PictureSize(32, 32), 1.0f, "doc_1.png");
        var pullSnapshot = new PullSnapshot("doc-1", pullArea, hasUnpushedChanges: true, "canvas_pull.png");
        var paletteSnapshot = new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, new ArgbColor[256]);
        var sessionSnapshot = new SessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "doc-1",
            new[] { docSnapshot },
            pullSnapshot,
            paletteSnapshot);

        var payloads = new Dictionary<string, byte[]>
        {
            ["doc_1.png"] = _codec.EncodeToPng(docPicture),
            ["canvas_pull.png"] = _codec.EncodeToPng(canvasPicture)
        };

        await _storage.SaveSnapshotAsync(sessionSnapshot, payloads);

        // 2. Act: MainViewModel 初期化 & 再開実行
        var mainVM = CreateMainViewModel();
        await mainVM.InitializeAsync();
        Assert.That(mainVM.WelcomeViewModel.HasPreviousSession, Is.True);

        await mainVM.WelcomeViewModel.ResumeLastSessionCommand!.Execute().ToTask();

        // 3. Assert: キャンバスの画像バッファが確実に blueColor (復帰した画像) になっていること
        Assert.That(mainVM.DrawableCanvasViewModel.PictureBuffer.Previous.Size, Is.EqualTo(new PictureSize(8, 8)));
        Assert.That(mainVM.DrawableCanvasViewModel.PictureBuffer.Previous.PickColor(new Position(0, 0)), Is.EqualTo(blueColor));
        Assert.That(mainVM.DrawingSessionViewModel.CurrentSession.Buffer.Previous.PickColor(new Position(0, 0)), Is.EqualTo(blueColor));
    }

    [AvaloniaTest]
    public async Task RestoreRecovery_RestoresMultiplePaletteTabs_WithoutOverwritingTemporaryPalette()
    {
        // 1. Arrange: 一時パレット(Tabs[0])とファイルパレット(Tabs[1])のタブ情報を持ったセッション
        var tempColor = new ArgbColor(255, 10, 20, 30);
        var fileColor = new ArgbColor(255, 200, 100, 50);

        var tempColors = Enumerable.Repeat(tempColor, 256).ToArray();
        var fileColors = Enumerable.Repeat(fileColor, 256).ToArray();

        var tab0Snapshot = new PaletteTabSnapshot(null, false, tempColors);
        var tab1Snapshot = new PaletteTabSnapshot("C:/palettes/custom.eede", false, fileColors);

        var paletteSnapshot = new PaletteSnapshot(
            fileColor,
            1, // ファイルパレットを選択状態に
            fileColors,
            new[] { tab0Snapshot, tab1Snapshot });

        var docSnapshot = new DocumentSnapshot("doc-1", null, false, new PictureSize(16, 16), 1.0f, null);
        var sessionSnapshot = new SessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "doc-1",
            new[] { docSnapshot },
            null,
            paletteSnapshot);

        await _storage.SaveSnapshotAsync(sessionSnapshot, new Dictionary<string, byte[]>());

        // 2. Act: MainViewModel 初期化 & 再開実行
        var mainVM = CreateMainViewModel();
        await mainVM.InitializeAsync();

        await mainVM.WelcomeViewModel.ResumeLastSessionCommand!.Execute().ToTask();

        // 3. Assert: パレットタブが2つ復元され、一時パレットが上書きされていないこと！
        var paletteVM = mainVM.PaletteContainerViewModel;
        Assert.That(paletteVM.Tabs.Count, Is.EqualTo(2));

        // Tabs[0] は一時パレット (FilePath == null, tempColor)
        Assert.That(paletteVM.Tabs[0].FilePath, Is.Null);
        Assert.That(paletteVM.Tabs[0].Palette.Fetch(0), Is.EqualTo(tempColor));

        // Tabs[1] はファイルパレット (FilePath == "C:/palettes/custom.eede", fileColor)
        Assert.That(paletteVM.Tabs[1].FilePath, Is.EqualTo("C:/palettes/custom.eede"));
        Assert.That(paletteVM.Tabs[1].Palette.Fetch(0), Is.EqualTo(fileColor));

        // 選択中タブは Tabs[1] (ファイルパレット)
        Assert.That(paletteVM.SelectedTab, Is.EqualTo(paletteVM.Tabs[1]));
    }

    [AvaloniaTest]
    public async Task Pull_ThenPushToDock_ThenExitAndResume_CanvasPictureIsStillRestored()
    {
        // 1. Arrange: ドックを作成し、キャンバスに Pull して編集し、ドックに Push して書き戻す
        var mainVM = CreateMainViewModel();
        mainVM.CloseWindowInteraction.RegisterHandler(c => c.SetOutput(System.Reactive.Unit.Default));

        var redColor = new ArgbColor(255, 255, 0, 0);
        var initialPic = CreateFilledPicture(new PictureSize(32, 32), redColor);
        var dockVm = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _pictureIOServiceMock.Object);
        dockVm.Initialize(initialPic, FilePath.Empty());
        mainVM.Pictures.Add(dockVm);

        // ドックから Pull (8x8)
        var pullArea = new PictureArea(new Position(0, 0), new PictureSize(8, 8));
        await dockVm.OnPicturePull.Execute(pullArea).ToTask();

        // キャンバスに青色を塗る
        var blueColor = new ArgbColor(255, 0, 0, 255);
        var bluePic = CreateFilledPicture(new PictureSize(8, 8), blueColor);
        mainVM.DrawingSessionViewModel.Push(bluePic, null, null);

        // ドックへ Push (書き戻し) -> これにより _pullContextTracker.ClearPullContext() が呼ばれる！
        await dockVm.OnPicturePush.Execute(new Position(0, 0)).ToTask();
        Assert.That(_pullTracker.CurrentContext, Is.Null, "Push後はPullContextがクリアされているはず");
        dockVm.Edited = false;

        Assert.That(mainVM.DrawableCanvasViewModel.PictureBuffer.Previous.Size, Is.EqualTo(new PictureSize(8, 8)), "クローズ直前のキャンバスサイズは8x8のはず");

        // 2. Act: ウィンドウのクローズ要求 (セッション保存＆正常終了)
        await mainVM.RequestCloseCommand.Execute().ToTask();
        Assert.That(_storage.IsCleanExitMarked, Is.True);

        var savedSnapshot = await _storage.LoadLatestSnapshotAsync();
        Assert.That(savedSnapshot, Is.Not.Null);
        Assert.That(savedSnapshot!.PullState, Is.Not.Null, "PullState must not be null in saved snapshot");
        Assert.That(savedSnapshot.PullState!.CanvasImagePayloadRef, Is.Not.Null);

        // 3. 次回起動: 同じストレージを用いて再度 MainViewModel を初期化＆再開
        var nextMainVM = CreateMainViewModel();
        await nextMainVM.InitializeAsync();
        Assert.That(nextMainVM.WelcomeViewModel.HasPreviousSession, Is.True);

        var service = new SessionRecoverer(_storage, _codec);
        var restoredData = await service.RestoreSessionAsync();
        Assert.That(restoredData.PullState, Is.Not.Null, "Restored PullState must not be null");
        Assert.That(restoredData.PullState!.CanvasPicture, Is.Not.Null, "Restored CanvasPicture must not be null");
        Assert.That(restoredData.PullState!.CanvasPicture!.Size, Is.EqualTo(new PictureSize(8, 8)));

        await nextMainVM.WelcomeViewModel.ResumeLastSessionCommand!.Execute().ToTask();

        // 4. Assert: Push後であってもキャンバスに描いていた画像 (8x8, blueColor) が確実に復元されること！
        Assert.That(nextMainVM.DrawableCanvasViewModel.PictureBuffer.Previous.Size, Is.EqualTo(new PictureSize(8, 8)));
        Assert.That(nextMainVM.DrawableCanvasViewModel.PictureBuffer.Previous.PickColor(new Position(0, 0)), Is.EqualTo(blueColor));
        Assert.That(nextMainVM.DrawingSessionViewModel.CurrentSession.Buffer.Previous.PickColor(new Position(0, 0)), Is.EqualTo(blueColor));
    }

    private static Picture CreateFilledPicture(PictureSize size, ArgbColor color)
    {
        var bytes = new byte[size.Width * size.Height * 4];
        for (int i = 0; i < bytes.Length; i += 4)
        {
            bytes[i] = color.Blue;
            bytes[i + 1] = color.Green;
            bytes[i + 2] = color.Red;
            bytes[i + 3] = color.Alpha;
        }
        return Picture.Create(size, bytes);
    }
}


