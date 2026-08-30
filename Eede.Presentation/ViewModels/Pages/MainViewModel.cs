using Eede.Domain.ImageEditing.Filters;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Dock.Model.Core;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.ImageEditing.History;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Actions;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Models;
using Eede.Application.Infrastructure;
using Eede.Presentation.Events;
using Eede.Presentation.Files;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.General;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Settings;
using Eede.Application.UseCase.Settings;
using Eede.Application.UseCase.Updates;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using ReactiveUI.SourceGenerators;

namespace Eede.Presentation.ViewModels.Pages;

#nullable enable

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<DockPictureViewModel> Pictures { get; } = [];
    public DrawableCanvasViewModel DrawableCanvasViewModel { get; }
    public AnimationViewModel AnimationViewModel { get; }

    [Reactive] public partial BackgroundColor CurrentBackgroundColor { get; set; }

    public Magnification Magnification
    {
        get => DrawableCanvasViewModel.Magnification;
        set => DrawableCanvasViewModel.Magnification = value;
    }

    [Reactive] public partial DrawStyleType DrawStyle { get; set; }

    [Reactive] public partial IImageBlender ImageBlender { get; set; }
    [Reactive] public partial IImageTransfer ImageTransfer { get; set; }

    [Reactive] public partial ArgbColor PenColor { get; set; }
    [Reactive] public partial Color NowPenColor { get; set; }
    [Reactive] public partial Color SampleColor { get; set; }

    public int PenWidth
    {
        get => DrawableCanvasViewModel.PenSize;
        set => DrawableCanvasViewModel.PenSize = value;
    }

    [Reactive] public partial IImageBlender PullBlender { get; set; }
    [Reactive] public partial IDockable? ActiveDockable { get; set; }

    [Reactive] public partial ObservableCollection<int> MinCursorSizeList { get; set; }
    [Reactive] public partial int MinCursorWidth { get; set; }
    [Reactive] public partial int MinCursorHeight { get; set; }
    [Reactive] public partial PictureSize CursorSize { get; set; }

    [Reactive] public partial DrawingSessionViewModel DrawingSessionViewModel { get; set; }
    [Reactive] public partial IFileStorage? FileStorage { get; set; }
    [Reactive] public partial Cursor? AnimationCursor { get; set; }
    [Reactive] public partial bool IsAnimationPanelExpanded { get; set; }
    [Reactive] public partial bool HasClipboardPicture { get; set; }
    [Reactive] public partial bool IsTransparencyEnabled { get; set; }
    [Reactive] public partial bool IsShowPixelGrid { get; set; }
    [Reactive] public partial bool IsShowCursorGrid { get; set; }

    [Reactive] public partial int SelectedThemeIndex { get; set; }

    public WelcomeViewModel WelcomeViewModel { get; }

    [ObservableAsProperty] private bool _isUpdateReady;
    public ReactiveCommand<RxVoid, RxVoid> ApplyUpdateCommand { get; private set; }

    public ReactiveCommand<RxVoid, RxVoid> UndoCommand => DrawingSessionViewModel.UndoCommand;
    public ReactiveCommand<RxVoid, RxVoid> RedoCommand => DrawingSessionViewModel.RedoCommand;
    public ReactiveCommand<IFileStorage?, RxVoid> LoadPictureCommand { get; private set; }
    public ReactiveCommand<IFileStorage?, RxVoid> SavePictureCommand { get; private set; }
    public ReactiveCommand<IFileStorage?, RxVoid> SavePictureAsCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> DeselectCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> SwapColorsCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> ResetDefaultColorsCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> IncreasePenWidthCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> DecreasePenWidthCommand { get; private set; }
    public ReactiveCommand<int, RxVoid> SetPenWidthCommand { get; private set; }
    public ReactiveCommand<DrawStyleType, RxVoid> SetDrawStyleCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> TogglePixelGridCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> ToggleCursorGridCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> ToggleTransparencyCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> ZoomInCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> ZoomOutCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> ResetZoomCommand { get; private set; }
    public ReactiveCommand<float, RxVoid> SetMagnificationCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> CloseActivePictureCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> ToggleAnimationPanelCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> SetLayerStyleRgbCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> SetLayerStyleAlphaCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> SetLayerStyleArgbCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> IncreaseMinCursorSizeCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> DecreaseMinCursorSizeCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> PushToActiveDockCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> PullFromActiveDockCommand { get; private set; }
    public ReactiveCommand<PictureActions, RxVoid> PictureActionCommand { get; private set; }
    public ReactiveCommand<int, RxVoid> PutPaletteColorCommand { get; private set; }
    public ReactiveCommand<int, RxVoid> GetPaletteColorCommand { get; private set; }

    public Interaction<NewPictureWindowViewModel, NewPictureWindowViewModel> ShowCreateNewPictureModal { get; private set; } = new();
    public ReactiveCommand<RxVoid, RxVoid> CreateNewPictureCommand { get; private set; }

    public ReactiveCommand<IFileStorage, RxVoid> LoadPaletteCommand { get; private set; }
    public ReactiveCommand<IFileStorage, RxVoid> SavePaletteCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> PutBackgroundColorCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> GetBackgroundColorCommand { get; private set; }
    public PaletteContainerViewModel PaletteContainerViewModel { get; private set; }

    public Interaction<ScalingDialogViewModel, ResizeContext?> ShowScalingModal { get; private set; } = new();
    public ReactiveCommand<RxVoid, RxVoid> ScalingCommand { get; private set; }

    // Viewにウィンドウを閉じるよう通知するためのInteraction
    public Interaction<Unit, Unit> CloseWindowInteraction { get; private set; } = new();

    // Viewからのクローズ要求を受け取るためのコマンド
    public ReactiveCommand<RxVoid, RxVoid> RequestCloseCommand { get; private set; }

    private bool _isCloseConfirmed;
    public bool IsCloseConfirmed
    {
        get => _isCloseConfirmed;
        private set => this.RaiseAndSetIfChanged(ref _isCloseConfirmed, value);
    }

    private readonly IBitmapAdapter<Avalonia.Media.Imaging.Bitmap> _bitmapAdapter;
    private readonly IPictureRepository _pictureRepository;
    private readonly IDrawStyleFactory _drawStyleFactory;
    private readonly ITransformImageUseCase _transformImageUseCase;
    private readonly IScalingImageUseCase _scalingImageUseCase;
    private readonly ITransferImageToCanvasUseCase _transferImageToCanvasUseCase;
    private readonly ITransferImageFromCanvasUseCase _transferImageFromCanvasUseCase;
    private readonly IDrawingSessionProvider _drawingSessionProvider;
    private readonly IPictureIOService _pictureIOService;
    private readonly IThemeService _themeService;
    private readonly ILoadSettingsUseCase _loadSettingsUseCase;
    private readonly ISaveSettingsUseCase _saveSettingsUseCase;
    private readonly IUpdateService? _updateService;
    private readonly CheckUpdateUseCase? _checkUpdateUseCase;
    private readonly GlobalState _state;
    private readonly IClipboard _clipboard;
    private readonly Func<DockPictureViewModel> _dockPictureFactory;
    private readonly Func<NewPictureWindowViewModel> _newPictureWindowFactory;

    private bool _isInitializing = true;
    private AppSettings? _appSettings;

    public ReactiveCommand<RxVoid, RxVoid> CopyCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> CutCommand { get; private set; }
    public ReactiveCommand<RxVoid, RxVoid> PasteCommand { get; private set; }

    public MainViewModel(
        GlobalState State,
        IClipboard clipboard,
        IBitmapAdapter<Avalonia.Media.Imaging.Bitmap> bitmapAdapter,
        IPictureRepository pictureRepository,
        IDrawStyleFactory drawStyleFactory,
        ITransformImageUseCase transformImageUseCase,
        IScalingImageUseCase scalingImageUseCase,
        ITransferImageToCanvasUseCase transferImageToCanvasUseCase,
        ITransferImageFromCanvasUseCase transferImageFromCanvasUseCase,
        IDrawingSessionProvider drawingSessionProvider,
        DrawableCanvasViewModel drawableCanvasViewModel,
        AnimationViewModel animationViewModel,
        DrawingSessionViewModel drawingSessionViewModel,
        PaletteContainerViewModel paletteContainerViewModel,
        IPictureIOService pictureIOService,
        IThemeService themeService,
        ILoadSettingsUseCase loadSettingsUseCase,
        ISaveSettingsUseCase saveSettingsUseCase,
        WelcomeViewModel welcomeViewModel,
        Func<DockPictureViewModel> dockPictureFactory,
        Func<NewPictureWindowViewModel> newPictureWindowFactory,
        IUpdateService? updateService = null,
        CheckUpdateUseCase? checkUpdateUseCase = null)
    {
        _state = State;
        _clipboard = clipboard;
        _bitmapAdapter = bitmapAdapter;
        _pictureRepository = pictureRepository;
        _drawStyleFactory = drawStyleFactory;
        _transformImageUseCase = transformImageUseCase;
        _scalingImageUseCase = scalingImageUseCase;
        _transferImageToCanvasUseCase = transferImageToCanvasUseCase;
        _transferImageFromCanvasUseCase = transferImageFromCanvasUseCase;
        _drawingSessionProvider = drawingSessionProvider;
        _pictureIOService = pictureIOService;
        _themeService = themeService;
        _loadSettingsUseCase = loadSettingsUseCase;
        _saveSettingsUseCase = saveSettingsUseCase;
        _updateService = updateService;
        _checkUpdateUseCase = checkUpdateUseCase;
        WelcomeViewModel = welcomeViewModel;
        _dockPictureFactory = dockPictureFactory;
        _newPictureWindowFactory = newPictureWindowFactory;

        _isUpdateReadyHelper = null!;
        _imageBlender = null!;
        _imageTransfer = null!;
        _pullBlender = null!;
        _minCursorSizeList = null!;

        DrawableCanvasViewModel = drawableCanvasViewModel;
        AnimationViewModel = animationViewModel;
        DrawingSessionViewModel = drawingSessionViewModel;
        PaletteContainerViewModel = paletteContainerViewModel;

        SelectedThemeIndex = _themeService.GetActualThemeVariant() == Avalonia.Styling.ThemeVariant.Dark ? 1 : 0;

        welcomeViewModel.CreateNewPictureCommand.Subscribe(_ => CreateNewPictureCommand?.Execute().Subscribe());
        welcomeViewModel.OpenPictureCommand.Subscribe(_ =>
        {
            if (FileStorage != null)
            {
                LoadPictureCommand?.Execute(FileStorage).Subscribe();
            }
        });
        welcomeViewModel.OpenRecentFileCommand.Subscribe(async path =>
        {
            if (!System.IO.File.Exists(path))
            {
                // ファイルが存在しない場合はリストから削除して更新
                var settings = await _loadSettingsUseCase.ExecuteAsync();
                var item = settings.RecentFiles.FirstOrDefault(f => f.Path == path);
                if (item != null)
                {
                    settings.RecentFiles.Remove(item);
                    await _saveSettingsUseCase.ExecuteAsync(settings);
                    WelcomeViewModel.LoadRecentFilesCommand.Execute().Subscribe();
                }
                return;
            }

            DockPictureViewModel? newPicture = await OpenPicture(new Uri(path));
            if (newPicture != null)
            {
                Pictures.Add(newPicture);
                WelcomeViewModel.LoadRecentFilesCommand.Execute().Subscribe();
            }
        });
        welcomeViewModel.LoadRecentFilesCommand.Execute().Subscribe();

        LoadPictureCommand = ReactiveCommand.Create<IFileStorage?>(ExecuteLoadPicture);
        SavePictureCommand = ReactiveCommand.Create<IFileStorage?>(ExecuteSavePicture);
        SavePictureAsCommand = ReactiveCommand.Create<IFileStorage?>(ExecuteSavePictureAs);
        PictureActionCommand = ReactiveCommand.Create<PictureActions>(ExecutePictureAction);
        PutPaletteColorCommand = ReactiveCommand.Create<int>(_ => { });
        GetPaletteColorCommand = ReactiveCommand.Create<int>(_ => { });
        CreateNewPictureCommand = ReactiveCommand.Create(ExecuteCreateNewPicture);
        LoadPaletteCommand = ReactiveCommand.Create<IFileStorage>(_ => { });
        SavePaletteCommand = ReactiveCommand.Create<IFileStorage>(_ => { });
        PutBackgroundColorCommand = ReactiveCommand.Create(() => { });
        GetBackgroundColorCommand = ReactiveCommand.Create(() => { });
        ScalingCommand = ReactiveCommand.CreateFromTask(ExecuteScalingAsync);
        RequestCloseCommand = ReactiveCommand.CreateFromTask(RequestCloseAsync);
        DeselectCommand = ReactiveCommand.Create(() =>
        {
            DrawableCanvasViewModel.CommitSelection(true);
        });
        SwapColorsCommand = ReactiveCommand.Create(() =>
        {
            var temp = PenColor;
            PenColor = CurrentBackgroundColor.Value;
            CurrentBackgroundColor = new BackgroundColor(temp);
        });
        ResetDefaultColorsCommand = ReactiveCommand.Create(() =>
        {
            PenColor = new ArgbColor(255, 0, 0, 0); // 前景: 黒
            CurrentBackgroundColor = new BackgroundColor(new ArgbColor(255, 255, 255, 255)); // 背景: 白
        });
        IncreasePenWidthCommand = ReactiveCommand.Create(() =>
        {
            PenWidth = Math.Min(64, PenWidth + 1);
        });
        DecreasePenWidthCommand = ReactiveCommand.Create(() =>
        {
            PenWidth = Math.Max(1, PenWidth - 1);
        });
        SetPenWidthCommand = ReactiveCommand.Create<int>((size) =>
        {
            PenWidth = Math.Clamp(size, 1, 64);
        });
        SetDrawStyleCommand = ReactiveCommand.Create<DrawStyleType>((style) =>
        {
            DrawStyle = style;
        });
        TogglePixelGridCommand = ReactiveCommand.Create(() =>
        {
            IsShowPixelGrid = !IsShowPixelGrid;
        });
        ToggleCursorGridCommand = ReactiveCommand.Create(() =>
        {
            IsShowCursorGrid = !IsShowCursorGrid;
        });
        ToggleTransparencyCommand = ReactiveCommand.Create(() =>
        {
            IsTransparencyEnabled = !IsTransparencyEnabled;
        });
        float[] magnificationSteps = [1f, 2f, 4f, 6f, 8f, 12f];
        ZoomInCommand = ReactiveCommand.Create(() =>
        {
            float current = Magnification.Value;
            foreach (float step in magnificationSteps)
            {
                if (step > current)
                {
                    Magnification = new Magnification(step);
                    return;
                }
            }
        });
        ZoomOutCommand = ReactiveCommand.Create(() =>
        {
            float current = Magnification.Value;
            for (int i = magnificationSteps.Length - 1; i >= 0; i--)
            {
                if (magnificationSteps[i] < current)
                {
                    Magnification = new Magnification(magnificationSteps[i]);
                    return;
                }
            }
        });
        ResetZoomCommand = ReactiveCommand.Create(() =>
        {
            Magnification = new Magnification(1f);
        });
        SetMagnificationCommand = ReactiveCommand.Create<float>((mag) =>
        {
            Magnification = new Magnification(mag);
        });
        CloseActivePictureCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (ActiveDockable is Dock.Model.Avalonia.Controls.Document doc && doc.DataContext is DockPictureViewModel vm)
            {
                bool canClose = await vm.ExecuteClosing();
                if (canClose)
                {
                    Pictures.Remove(vm);
                }
            }
        });
        ToggleAnimationPanelCommand = ReactiveCommand.Create(() =>
        {
            IsAnimationPanelExpanded = !IsAnimationPanelExpanded;
        });
        SetLayerStyleRgbCommand = ReactiveCommand.Create(() =>
        {
            ImageTransfer = new RGBToneImageTransfer();
            ImageBlender = new RGBOnlyImageBlender();
        });
        SetLayerStyleAlphaCommand = ReactiveCommand.Create(() =>
        {
            ImageTransfer = new AlphaToneImageTransfer();
            ImageBlender = new AlphaOnlyImageBlender();
        });
        SetLayerStyleArgbCommand = ReactiveCommand.Create(() =>
        {
            ImageTransfer = new DirectImageTransfer();
            ImageBlender = new DirectImageBlender();
        });
        IncreaseMinCursorSizeCommand = ReactiveCommand.Create(() =>
        {
            if (MinCursorSizeList != null && MinCursorSizeList.Count > 0)
            {
                int currentIdx = MinCursorSizeList.IndexOf(MinCursorWidth);
                if (currentIdx >= 0 && currentIdx < MinCursorSizeList.Count - 1)
                {
                    int next = MinCursorSizeList[currentIdx + 1];
                    MinCursorWidth = next;
                    MinCursorHeight = next;
                }
            }
        });
        DecreaseMinCursorSizeCommand = ReactiveCommand.Create(() =>
        {
            if (MinCursorSizeList != null && MinCursorSizeList.Count > 0)
            {
                int currentIdx = MinCursorSizeList.IndexOf(MinCursorWidth);
                if (currentIdx > 0)
                {
                    int prev = MinCursorSizeList[currentIdx - 1];
                    MinCursorWidth = prev;
                    MinCursorHeight = prev;
                }
            }
        });
        PushToActiveDockCommand = ReactiveCommand.Create(() =>
        {
            if (ActiveDockable is Dock.Model.Avalonia.Controls.Document doc && doc.DataContext is DockPictureViewModel vm)
            {
                // メインキャンバスの内容をドック側のカーソル位置へ転送・書き戻し (Push)
                Position targetPos = vm.GlobalState.CursorArea.RealPosition;
                OnPushFromDrawArea(vm, new PicturePushEventArgs(vm.PictureBuffer, targetPos));
            }
        });
        PullFromActiveDockCommand = ReactiveCommand.Create(() =>
        {
            if (ActiveDockable is Dock.Model.Avalonia.Controls.Document doc && doc.DataContext is DockPictureViewModel vm)
            {
                // ドック側のカーソル位置の画像をメインキャンバスへ再読み込み (Pull)
                Position targetPos = vm.GlobalState.CursorArea.RealPosition;
                OnPullToDrawArea(vm, new PicturePullEventArgs(vm.PictureBuffer, new PictureArea(targetPos, CursorSize)));
            }
        });
        CopyCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask);
        CutCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask);
        PasteCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask);

        if (_updateService != null)
        {
            var canApplyUpdate = _updateService.StatusChanged
                .Select(status => status == UpdateStatus.ReadyToApply);
            ApplyUpdateCommand = ReactiveCommand.Create(() =>
            {
                _updateService.ApplyAndRestart();
            }, canApplyUpdate);

            _updateService.StatusChanged
                .Select(status => status == UpdateStatus.ReadyToApply)
                .ToProperty(this, nameof(IsUpdateReady), out _isUpdateReadyHelper);
        }
        else
        {
            ApplyUpdateCommand = ReactiveCommand.Create(() => { });
        }

        InitializeConnections();
        _ = LoadSettingsAsync();
        _ = UpdateClipboardStatusAsync();
    }

    private async Task LoadSettingsAsync()
    {
        _isInitializing = true;
        _appSettings = await _loadSettingsUseCase.ExecuteAsync();
        if (_appSettings != null)
        {
            MinCursorWidth = _appSettings.GridWidth;
            MinCursorHeight = _appSettings.GridHeight;
        }
        _isInitializing = false;
    }

    private void InitializeConnections()
    {
        Pictures.CollectionChanged += Pictures_CollectionChanged;

        ImageBlender = new DirectImageBlender();
        ImageTransfer = new DirectImageTransfer();
        CurrentBackgroundColor = BackgroundColor.Default;
        _ = this.WhenAnyValue(x => x.CurrentBackgroundColor)
            .BindTo(this, x => x.DrawableCanvasViewModel.BackgroundColor);
        _ = this.WhenAnyValue(x => x.IsShowPixelGrid).BindTo(this, x => x.DrawableCanvasViewModel.IsShowPixelGrid);
        _ = this.WhenAnyValue(x => x.IsShowCursorGrid).BindTo(this, x => x.DrawableCanvasViewModel.IsShowCursorGrid);
        _ = this.WhenAnyValue(x => x.CursorSize).BindTo(this, x => x.DrawableCanvasViewModel.CursorSize);

        _ = this.WhenAnyValue(x => x.ImageBlender)
            .Subscribe(x => DrawableCanvasViewModel.ImageBlender = x);
        _ = DrawableCanvasViewModel.WhenAnyValue(x => x.ImageBlender)
            .Subscribe(x => ImageBlender = x);

        _ = this.WhenAnyValue(x => x.ImageTransfer)
            .Subscribe(x => DrawableCanvasViewModel.ImageTransfer = x);
        _ = DrawableCanvasViewModel.WhenAnyValue(x => x.ImageTransfer)
            .Subscribe(x => ImageTransfer = x);

        PullBlender = new DirectImageBlender();
        PenColor = DrawableCanvasViewModel.PenColor;
        _ = this.WhenAnyValue(x => x.PenColor)
            .Select(x => Color.FromArgb(x.Alpha, x.Red, x.Green, x.Blue))
            .BindTo(this, x => x.NowPenColor);

        _ = this.WhenAnyValue(x => x.PenColor)
            .Subscribe(x => DrawableCanvasViewModel.PenColor = x);

        _ = DrawableCanvasViewModel.WhenAnyValue(x => x.PenColor)
            .Subscribe(x => PenColor = x);

        MinCursorSizeList = new ObservableCollection<int>([8, 16, 24, 32, 48, 64]);
        MinCursorWidth = 32;
        MinCursorHeight = 32;
        _ = this.WhenAnyValue(x => x.MinCursorWidth, x => x.MinCursorHeight)
            .Subscribe(async _ =>
            {
                PictureSize size = new(MinCursorWidth, MinCursorHeight);
                CursorSize = size;
                foreach (DockPictureViewModel vm in Pictures)
                {
                    vm.MinCursorSize = size;
                }
                if (!_isInitializing)
                {
                    if (_appSettings != null)
                    {
                        _appSettings.GridWidth = MinCursorWidth;
                        _appSettings.GridHeight = MinCursorHeight;
                        await _saveSettingsUseCase.ExecuteAsync(_appSettings);
                    }
                }
            });

        _ = this.WhenAnyValue(x => x.CursorSize)
           .Subscribe(size =>
           {
               foreach (DockPictureViewModel vm in Pictures)
               {
                   vm.CursorSize = size;
               }
           });
        DrawStyle = DrawStyleType.FreeCurve;
        _ = this.WhenAnyValue(x => x.DrawStyle).Subscribe(drawStyle => DrawableCanvasViewModel.DrawStyle = ExecuteUpdateDrawStyle(drawStyle));

        _ = this.WhenAnyValue(x => x.IsTransparencyEnabled)
            .Subscribe(enabled =>
            {
                ImageBlender = enabled ? new AlphaImageBlender() : new DirectImageBlender();
                PullBlender = enabled ? new AlphaImageBlender() : new DirectImageBlender();
            });

        _ = this.WhenAnyValue(x => x.CurrentBackgroundColor)
            .Subscribe(color =>
            {
                foreach (var vm in Pictures)
                {
                    vm.BackgroundColor = color;
                }
            });

        this.WhenAnyValue(x => x.ActiveDockable)
            .Select(active => active is Dock.Model.Avalonia.Controls.Document doc && doc.DataContext is DockPictureViewModel vm
                ? vm.WhenAnyValue(x => x.PictureBuffer)
                : Observable.Return<Picture?>(null))
            .Switch()
            .BindTo(this, x => x.AnimationViewModel.ActivePicture);

        _ = DrawingSessionViewModel.Attach(DrawableCanvasViewModel);
        DrawableCanvasViewModel.Drew += (previous, now, previousArea, nowArea, affectedArea) =>
        {
            MarkActiveDockEdited();
        };

        PutBackgroundColorCommand = ReactiveCommand.Create(() =>
        {
            CurrentBackgroundColor = new BackgroundColor(new ArgbColor(NowPenColor.A, NowPenColor.R, NowPenColor.G, NowPenColor.B));
        });
        GetBackgroundColorCommand = ReactiveCommand.Create(() =>
        {
            PenColor = CurrentBackgroundColor.Value;
        });

        PaletteContainerViewModel.OnApplyColor += OnApplyPaletteColor;
        PaletteContainerViewModel.OnFetchColor += OnFetchPaletteColor;

        this.WhenAnyValue(x => x.IsAnimationPanelExpanded)
            .Where(expanded => !expanded)
            .Subscribe(_ => AnimationViewModel.IsAnimationMode = false);

        var canCopyCut = this.WhenAnyValue(
            x => x.DrawStyle,
            x => x.DrawableCanvasViewModel.IsRegionSelecting,
            (style, isSelecting) => style == DrawStyleType.RegionSelect && isSelecting);
        CopyCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await DrawableCanvasViewModel.CopyCommand.Execute();
            HasClipboardPicture = true;
        }, canCopyCut);
        CutCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await DrawableCanvasViewModel.CutCommand.Execute();
            HasClipboardPicture = true;
        }, canCopyCut);
        PasteCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await DrawableCanvasViewModel.PasteCommand.Execute();
            DrawStyle = DrawStyleType.RegionSelect;
        }, this.WhenAnyValue(x => x.HasClipboardPicture));


        DrawingSessionViewModel.Undone += OnUndone;
        DrawingSessionViewModel.Redone += OnRedone;

        this.WhenAnyValue(x => x.ActiveDockable)
            .Subscribe(active =>
            {
                if (active is Dock.Model.Avalonia.Controls.Document doc && doc.DataContext is DockPictureViewModel vm)
                {
                    // ドキュメントが切り替わった時にキャンバスを初期化
                    // TODO: DrawingSessionProvider 側の切り替えと同期させる
                }
            });
    }

    private void OnUndone(object? sender, UndoResult e)
    {
        DrawableCanvasViewModel.PictureBuffer = e.Session.Buffer;
        DrawableCanvasViewModel.SyncWithSession(true);
        SetPictureToDrawArea(e.Session.FetchPicture(ImageBlender));

        if (e.Item is DockActiveHistoryItem dockItem)
        {
            var vm = Pictures.FirstOrDefault(x => x.Id == dockItem.DockId);
            if (vm != null)
            {
                vm.PictureBuffer = dockItem.Before;
                vm.Edited = dockItem.BeforeEdited;
            }
        }
    }

    private void OnRedone(object? sender, RedoResult e)
    {
        DrawableCanvasViewModel.PictureBuffer = e.Session.Buffer;
        DrawableCanvasViewModel.SyncWithSession(true);
        SetPictureToDrawArea(e.Session.FetchPicture(ImageBlender));

        if (e.Item is DockActiveHistoryItem dockItem)
        {
            var vm = Pictures.FirstOrDefault(x => x.Id == dockItem.DockId);
            if (vm != null)
            {
                vm.PictureBuffer = dockItem.After;
                vm.Edited = dockItem.AfterEdited;
            }
        }
    }

    public void DragOverPicture(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.None;
        e.Handled = false;

        if (e.DataTransfer.Contains(DataFormat.File) == false)
        {
            return;
        }

        e.DragEffects = DragDropEffects.Copy;
        e.Handled = true;
    }

    public async void DropPicture(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File) == false)
        {
            return;
        }

        IEnumerable<IStorageItem>? files = e.DataTransfer.TryGetFiles();
        if (files is null)
        {
            return;
        }

        foreach (IStorageItem file in files)
        {
            if (IsSupportedImageFile(file))
            {
                DockPictureViewModel? newPicture = await OpenPicture(file.Path);
                if (newPicture != null)
                {
                    Pictures.Add(newPicture);
                }
            }
            else if (IsSupportedPaletteFile(file))
            {
                PaletteContainerViewModel.LoadPalette(file.Path.LocalPath);
            }
        }
    }

    private bool IsSupportedPaletteFile(IStorageItem file)
    {
        var path = file.Path.LocalPath.ToLower();
        return path.EndsWith(".act") || path.EndsWith(".aact");
    }

    private bool IsSupportedImageFile(IStorageItem file)
    {
        var path = file.Path.LocalPath.ToLower();
        return path.EndsWith(".png") || path.EndsWith(".bmp") || path.EndsWith(".arv");
    }

    private async void ExecuteLoadPicture(IFileStorage? storage)
    {
        var targetStorage = storage ?? FileStorage;
        if (targetStorage == null)
        {
            return;
        }
        Uri? result = await targetStorage.OpenFilePickerAsync();
        if (result == null)
        {
            return;
        }
        DockPictureViewModel? newPicture = await OpenPicture(result);
        if (newPicture != null)
        {
            Pictures.Add(newPicture);
            WelcomeViewModel.LoadRecentFilesCommand.Execute().Subscribe();
        }

    }

    private void Pictures_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (DockPictureViewModel vm in e.NewItems)
            {
                SetupDockPicture(vm);
            }
        }
        if (e.OldItems != null)
        {
            foreach (DockPictureViewModel vm in e.OldItems)
            {
                CleanupDockPicture(vm);
            }
            if (Pictures.Count == 0)
            {
                WelcomeViewModel.LoadRecentFilesCommand.Execute().Subscribe();
            }
        }
    }

    private void CleanupDockPicture(DockPictureViewModel vm)
    {
        vm.PicturePull -= OnPullToDrawArea;
        vm.PicturePush -= OnPushFromDrawArea;
        vm.PictureUpdate -= OnPictureUpdate;
        vm.PictureSave -= OnPictureSave;
    }

    private async Task<DockPictureViewModel?> OpenPicture(Uri path)
    {
        FilePath filePath = new(path.LocalPath);
        Picture? picture = await _pictureIOService.LoadAsync(filePath);
        if (picture == null)
        {
            return null;
        }
        DockPictureViewModel vm = _dockPictureFactory();
        vm.Initialize(picture, filePath);
        return vm;
    }

    private void SetupDockPicture(DockPictureViewModel vm)
    {
        vm.PicturePull += OnPullToDrawArea;
        vm.PicturePush += OnPushFromDrawArea;
        vm.PictureUpdate += OnPictureUpdate;
        vm.BackgroundColor = CurrentBackgroundColor;
        vm.PictureSave += OnPictureSave;
        vm.MinCursorSize = new PictureSize(MinCursorWidth, MinCursorHeight);
        vm.CursorSize = CursorSize;
        _ = this.WhenAnyValue(x => x.AnimationCursor).BindTo(vm, x => x.AnimationCursor);
    }

    private async void ExecuteCreateNewPicture()
    {
        NewPictureWindowViewModel store = _newPictureWindowFactory();
        NewPictureWindowViewModel result = await ShowCreateNewPictureModal.Handle(store);
        if (result.Result)
        {
            DockPictureViewModel vm = _dockPictureFactory();
            vm.Initialize(Picture.CreateEmpty(result.Size), FilePath.Empty());
            Pictures.Add(vm);
        }
    }

    private async Task ExecuteScalingAsync()
    {
        PictureArea? area = DrawableCanvasViewModel.IsRegionSelecting ? DrawableCanvasViewModel.SelectingArea : null;
        PictureSize size = area?.Size ?? DrawableCanvasViewModel.PictureBuffer.Previous.Size;

        ScalingDialogViewModel vm = new(size);
        ResizeContext? context = await ShowScalingModal.Handle(vm);
        if (context != null)
        {
            DrawingSession updated = _scalingImageUseCase.Execute(_drawingSessionProvider.CurrentSession, context);
            DrawingSessionViewModel.Sync(updated);
            DrawableCanvasViewModel.SyncWithSession(true);
            SetPictureToDrawArea(updated.CurrentPicture);
            MarkActiveDockEdited();
        }
    }

    private void ExecuteSavePicture(IFileStorage? storage)
    {
        if (ActiveDockable is Dock.Model.Avalonia.Controls.Document doc)
        {
            if (doc.DataContext is DockPictureViewModel vm)
            {
                _ = vm.Save();
            }
        }
    }

    private void ExecuteSavePictureAs(IFileStorage? storage)
    {
        if (ActiveDockable is Dock.Model.Avalonia.Controls.Document doc)
        {
            if (doc.DataContext is DockPictureViewModel vm)
            {
                _ = vm.SaveAs();
            }
        }
    }

    private async Task OnPictureSave(object? sender, PictureSaveEventArgs e)
    {
        if (FileStorage == null) return;
        SaveImageResult saveResult = e.IsSaveAs
            ? await e.File.SaveAsAsync(FileStorage)
            : await e.File.SaveAsync(FileStorage);
        if (saveResult.IsCanceled)
        {
            e.Cancel();
            return;
        }
        if (saveResult.IsSaved && saveResult.File != null)
        {
            e.UpdateFile(saveResult.File);

            var settings = await _loadSettingsUseCase.ExecuteAsync();
            settings.AddRecentFile(saveResult.File.Path.ToString(), DateTime.Now);
            await _saveSettingsUseCase.ExecuteAsync(settings);
            WelcomeViewModel.LoadRecentFilesCommand.Execute().Subscribe();
        }
    }

    private void OnPullToDrawArea(object? sender, PicturePullEventArgs args)
    {
        if (sender is not DockPictureViewModel vm)
        {
            return;
        }
        DrawableCanvasViewModel.CommitSelection(true);
        Picture updated = _transferImageToCanvasUseCase.Execute(
            vm.PictureBuffer,
            args.Rect);

        DrawingSessionViewModel.Push(updated, null, DrawableCanvasViewModel.SelectingArea);
    }

    private void SetPictureToDrawArea(Picture picture)
    {
        DrawableCanvasViewModel.SetPicture(picture);
        CursorSize = picture.Size;
    }

    private void MarkActiveDockEdited()
    {
        if (ActiveDockable is Dock.Model.Avalonia.Controls.Document doc && doc.DataContext is DockPictureViewModel vm)
        {
            vm.Edited = true;
        }
    }

    private void OnPictureUpdate(object? sender, PictureUpdateEventArgs args)
    {
        if (sender is not DockPictureViewModel vm)
        {
            return;
        }
        // TODO: DockPictureViewModel 側も DrawingSession を持つようにリファクタリング予定
        vm.PictureBuffer = args.Updated;
    }

    private void OnPushFromDrawArea(object? sender, PicturePushEventArgs args)
    {
        if (sender is not DockPictureViewModel vm)
        {
            return;
        }

        DrawableCanvasViewModel.CommitSelection(true);
        Picture updated = _transferImageFromCanvasUseCase.Execute(
            vm.PictureBuffer,
            DrawableCanvasViewModel.PictureBuffer.Previous,
            args.Position,
            PullBlender);

        DrawingSessionViewModel.PushDockUpdate(vm.Id, args.Position, vm.PictureBuffer, updated, vm.Edited, true);

        vm.PictureBuffer = updated;
    }

    private void ExecutePictureAction(PictureActions actionType)
    {
        DrawableCanvasViewModel.CommitSelection();
        PictureArea? area = DrawableCanvasViewModel.IsRegionSelecting ? DrawableCanvasViewModel.SelectingArea : null;
        var mode = GetAntiAliasMode(ImageBlender);
        Picture updated = area.HasValue ? _transformImageUseCase.Execute(
            DrawableCanvasViewModel.PictureBuffer.Previous,
            actionType,
            area.Value,
            mode
        ) : _transformImageUseCase.Execute(
            DrawableCanvasViewModel.PictureBuffer.Previous,
            actionType,
            mode
        );

        DrawingSessionViewModel.Push(updated, area, DrawableCanvasViewModel.SelectingArea);
        MarkActiveDockEdited();
    }

    private AntiAliasMode GetAntiAliasMode(IImageBlender blender)
    {
        return blender switch
        {
            RGBOnlyImageBlender => AntiAliasMode.Rgb,
            AlphaOnlyImageBlender => AntiAliasMode.Alpha,
            _ => AntiAliasMode.Argb
        };
    }

    private DrawStyleType? _lastDrawStyle;
    private IDrawStyle ExecuteUpdateDrawStyle(DrawStyleType drawStyle)
    {
        if (_lastDrawStyle == drawStyle)
        {
            return DrawableCanvasViewModel.DrawStyle;
        }
        _lastDrawStyle = drawStyle;
        DrawableCanvasViewModel.IsRegionSelecting = false;
        var style = _drawStyleFactory.Create(drawStyle);
        if (style is RegionSelector regionSelector)
        {
            DrawableCanvasViewModel.SetupRegionSelector(regionSelector);
        }
        return style;
    }

    private ArgbColor OnApplyPaletteColor()
    {
        return PenColor;
    }

    private void OnFetchPaletteColor(ArgbColor color)
    {
        PenColor = color;
    }

    private async Task RequestCloseAsync()
    {
        // 二重実行を防止
        if (IsCloseConfirmed)
        {
            return;
        }

        try
        {

            // 各PictureViewModelのクローズ確認処理を実行
            // 【重要】ToList() は意図的なスナップショット生成（削除・インライン化禁止）。
            // CloseCommand の非同期実行中に UI や Dock 側で Pictures コレクションが変更され、
            // InvalidOperationException (Collection was modified) が発生するのを防ぐために必須。
            var picturesSnapshot = Pictures.ToList();
            foreach (DockPictureViewModel picture in picturesSnapshot)
            {
                bool canClosePicture = await picture.CloseCommand.Execute().ToTask();
                if (!canClosePicture)
                {
                    return; // ユーザーがキャンセルしたため、処理を中断
                }
            }

            if (!await PaletteContainerViewModel.TryCloseAllAsync())
            {
                return;
            }

            IsCloseConfirmed = true;
            // すべての確認が通ったら、Interactionを通じてViewに通知
            _ = await CloseWindowInteraction.Handle(Unit.Default);
        }
        finally
        {
        }
    }

    public async Task UpdateClipboardStatusAsync()
    {
        try
        {
            var hasPicture = await _clipboard.HasPictureAsync();
            global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                HasClipboardPicture = hasPicture;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to check clipboard: {ex.Message}");
            global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                HasClipboardPicture = false;
            });
        }
    }
}
