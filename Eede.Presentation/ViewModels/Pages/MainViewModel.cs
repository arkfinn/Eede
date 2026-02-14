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
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Eede.Presentation.ViewModels.Pages;

#nullable enable

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<DockPictureViewModel> Pictures { get; } = [];
    public DrawableCanvasViewModel DrawableCanvasViewModel { get; }
    public AnimationViewModel AnimationViewModel { get; }

    [Reactive] public BackgroundColor CurrentBackgroundColor { get; set; } = BackgroundColor.Default;

    public Magnification Magnification
    {
        get => DrawableCanvasViewModel.Magnification;
        set => DrawableCanvasViewModel.Magnification = value;
    }

    [Reactive] public DrawStyleType DrawStyle { get; set; }

    [Reactive] public IImageBlender ImageBlender { get; set; } = new DirectImageBlender();

    [Reactive] public IImageTransfer ImageTransfer { get; set; } = new DirectImageTransfer();

    [Reactive] public ArgbColor PenColor { get; set; } = new ArgbColor(255, 0, 0, 0);
    [Reactive] public Color NowPenColor { get; set; }
    [Reactive] public Color SampleColor { get; set; }

    public int PenWidth
    {
        get => DrawableCanvasViewModel.PenSize;
        set => DrawableCanvasViewModel.PenSize = value;
    }

    [Reactive] public IImageBlender PullBlender { get; set; } = new DirectImageBlender();
    [Reactive] public IDockable? ActiveDockable { get; set; }

    [Reactive] public ObservableCollection<int> MinCursorSizeList { get; set; } = new([8, 16, 24, 32, 48, 64]);
    [Reactive] public int MinCursorWidth { get; set; } = 32;
    [Reactive] public int MinCursorHeight { get; set; } = 32;
    [Reactive] public PictureSize CursorSize { get; set; } = new PictureSize(32, 32);

    [Reactive] public DrawingSessionViewModel DrawingSessionViewModel { get; private set; }
    [Reactive] public IFileStorage? FileStorage { get; set; }
    [Reactive] public Cursor? AnimationCursor { get; set; }
    [Reactive] public bool IsAnimationPanelExpanded { get; set; } = false;
    [Reactive] public bool HasClipboardPicture { get; set; } = false;
    [Reactive] public bool IsTransparencyEnabled { get; set; } = false;
    [Reactive] public bool IsShowPixelGrid { get; set; } = false;
    [Reactive] public bool IsShowCursorGrid { get; set; } = false;

    [Reactive] public int SelectedThemeIndex { get; set; }

    public ReactiveCommand<Unit, Unit> UndoCommand => DrawingSessionViewModel.UndoCommand;
    public ReactiveCommand<Unit, Unit> RedoCommand => DrawingSessionViewModel.RedoCommand;
    public ReactiveCommand<IFileStorage, Unit> LoadPictureCommand { get; private set; }
    public ReactiveCommand<IFileStorage, Unit> SavePictureCommand { get; private set; }
    public ReactiveCommand<PictureActions, Unit> PictureActionCommand { get; private set; }
    public ReactiveCommand<int, Unit> PutPaletteColorCommand { get; private set; }
    public ReactiveCommand<int, Unit> GetPaletteColorCommand { get; private set; }

    public Interaction<NewPictureWindowViewModel, NewPictureWindowViewModel> ShowCreateNewPictureModal { get; private set; } = new();
    public ReactiveCommand<Unit, Unit> CreateNewPictureCommand { get; private set; }

    public ReactiveCommand<IFileStorage, Unit> LoadPaletteCommand { get; private set; }
    public ReactiveCommand<IFileStorage, Unit> SavePaletteCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PutBackgroundColorCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> GetBackgroundColorCommand { get; private set; }
    public PaletteContainerViewModel PaletteContainerViewModel { get; private set; }

    public Interaction<ScalingDialogViewModel, ResizeContext?> ShowScalingModal { get; private set; } = new();
    public ReactiveCommand<Unit, Unit> ScalingCommand { get; private set; }

    // Viewにウィンドウを閉じるよう通知するためのInteraction
    public Interaction<Unit, Unit> CloseWindowInteraction { get; private set; } = new();

    // Viewからのクローズ要求を受け取るためのコマンド
    public ReactiveCommand<Unit, Unit> RequestCloseCommand { get; private set; }

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
    private readonly SettingsService _settingsService;
    private readonly GlobalState _state;
    private readonly IClipboard _clipboard;
    private readonly Func<DockPictureViewModel> _dockPictureFactory;
    private readonly Func<NewPictureWindowViewModel> _newPictureWindowFactory;

    private bool _isInitializing = true;

    public ReactiveCommand<Unit, Unit> CopyCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> CutCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PasteCommand { get; private set; }

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
        SettingsService settingsService,
        Func<DockPictureViewModel> dockPictureFactory,
        Func<NewPictureWindowViewModel> newPictureWindowFactory)
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
        _settingsService = settingsService;
        _dockPictureFactory = dockPictureFactory;
        _newPictureWindowFactory = newPictureWindowFactory;

        DrawableCanvasViewModel = drawableCanvasViewModel;
        AnimationViewModel = animationViewModel;
        DrawingSessionViewModel = drawingSessionViewModel;
        PaletteContainerViewModel = paletteContainerViewModel;

        SelectedThemeIndex = _themeService.GetActualThemeVariant() == Avalonia.Styling.ThemeVariant.Dark ? 1 : 0;

        LoadPictureCommand = ReactiveCommand.Create<IFileStorage>(ExecuteLoadPicture);
        SavePictureCommand = ReactiveCommand.Create<IFileStorage>(ExecuteSavePicture);
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
        CopyCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask);
        CutCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask);
        PasteCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask);

        InitializeConnections();
        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        _isInitializing = true;
        var settings = await _settingsService.GetSettingsAsync();
        MinCursorWidth = settings.GridWidth;
        MinCursorHeight = settings.GridHeight;
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
                    await _settingsService.SaveGridSizeAsync(MinCursorWidth, MinCursorHeight);
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

        this.WhenAnyValue(x => x.ActiveDockable)
            .Select(active => active is Dock.Model.Avalonia.Controls.Document doc && doc.DataContext is DockPictureViewModel vm
                ? vm.WhenAnyValue(x => x.PictureBuffer)
                : Observable.Return<Picture?>(null))
            .Switch()
            .BindTo(this, x => x.AnimationViewModel.ActivePicture);

        DrawableCanvasViewModel.Drew += (previous, now, previousArea, nowArea, affectedArea) =>
        {
            // TODO: DrawingSessionViewModel側で位置情報の復元も管理するようにリファクタリング予定
            DrawingSessionViewModel.Push(now, nowArea, previousArea, affectedArea);
        };

        LoadPictureCommand = ReactiveCommand.Create<IFileStorage>(ExecuteLoadPicture);
        SavePictureCommand = ReactiveCommand.Create<IFileStorage>(ExecuteSavePicture);
        PictureActionCommand = ReactiveCommand.Create<PictureActions>(ExecutePictureAction);

        CreateNewPictureCommand = ReactiveCommand.Create(ExecuteCreateNewPicture);

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

        RequestCloseCommand = ReactiveCommand.CreateFromTask(RequestCloseAsync);

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
            }
        }
    }

    public void DragOverPicture(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.None;
        e.Handled = false;

        if (e.Data is not IDataObject dataObject)
        {
            return;
        }

        if (dataObject.GetDataFormats().Contains(DataFormats.Files) == false)
        {
            return;
        }

        IEnumerable<IStorageItem>? files = dataObject.GetFiles();
        if (files is null || !files.Any(f => IsSupportedImageFile(f)))
        {
            return;
        }

        e.DragEffects = DragDropEffects.Copy;
        e.Handled = true;
    }

    public async void DropPicture(object? sender, DragEventArgs e)
    {
        if (e.Data is not IDataObject dataObject)
        {
            return;
        }

        if (dataObject.GetDataFormats().Contains(DataFormats.Files) == false)
        {
            return;
        }

        IEnumerable<IStorageItem>? files = dataObject.GetFiles();
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
        }
    }

    private bool IsSupportedImageFile(IStorageItem file)
    {
        var path = file.Path.LocalPath.ToLower();
        return path.EndsWith(".png") || path.EndsWith(".bmp") || path.EndsWith(".arv");
    }

    private async void ExecuteLoadPicture(IFileStorage storage)
    {
        Uri? result = await storage.OpenFilePickerAsync();
        if (result == null)
        {
            return;
        }
        DockPictureViewModel? newPicture = await OpenPicture(result);
        if (newPicture != null)
        {
            Pictures.Add(newPicture);
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
        }
    }

    private void CleanupDockPicture(DockPictureViewModel vm)
    {
        vm.PicturePush -= OnPushToDrawArea;
        vm.PicturePull -= OnPullFromDrawArea;
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
        vm.PicturePush += OnPushToDrawArea;
        vm.PicturePull += OnPullFromDrawArea;
        vm.PictureUpdate += OnPictureUpdate;
        vm.PictureSave += OnPictureSave;
        vm.MinCursorSize = new PictureSize(MinCursorWidth, MinCursorHeight);
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
        }
    }

    private void ExecuteSavePicture(IFileStorage storage)
    {
        if (ActiveDockable is Dock.Model.Avalonia.Controls.Document doc)
        {
            if (doc.DataContext is DockPictureViewModel vm)
            {
                _ = vm.Save();
            }
        }
    }

    private async Task OnPictureSave(object? sender, PictureSaveEventArgs e)
    {
        if (FileStorage == null) return;
        SaveImageResult saveResult = await e.File.SaveAsync(FileStorage);
        if (saveResult.IsCanceled)
        {
            e.Cancel();
            return;
        }
        if (saveResult.IsSaved && saveResult.File != null)
        {
            e.UpdateFile(saveResult.File);
        }
    }

    private void OnPushToDrawArea(object? sender, PicturePushEventArgs args)
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

    private void OnPictureUpdate(object? sender, PictureUpdateEventArgs args)
    {
        if (sender is not DockPictureViewModel vm)
        {
            return;
        }
        // TODO: DockPictureViewModel 側も DrawingSession を持つようにリファクタリング予定
        vm.PictureBuffer = args.Updated;
    }

    private void OnPullFromDrawArea(object? sender, PicturePullEventArgs args)
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

        DrawingSessionViewModel.PushDockUpdate(vm.Id, args.Position, vm.PictureBuffer, updated);

        vm.PictureBuffer = updated;
    }

    private void ExecutePictureAction(PictureActions actionType)
    {
        DrawableCanvasViewModel.CommitSelection();
        PictureArea? area = DrawableCanvasViewModel.IsRegionSelecting ? DrawableCanvasViewModel.SelectingArea : null;
        Picture updated = area.HasValue ? _transformImageUseCase.Execute(
            DrawableCanvasViewModel.PictureBuffer.Previous,
            actionType,
            area.Value
        ) : _transformImageUseCase.Execute(
            DrawableCanvasViewModel.PictureBuffer.Previous,
            actionType
        );

        DrawingSessionViewModel.Push(updated, area, DrawableCanvasViewModel.SelectingArea);
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
            foreach (DockPictureViewModel picture in Pictures.ToList())
            {
                // ここは前回提案の通り、コマンドがboolを返す設計が望ましい
                bool canClosePicture = await picture.CloseCommand.Execute();
                if (!canClosePicture)
                {
                    return; // ユーザーがキャンセルしたため、処理を中断
                }
            }

            IsCloseConfirmed = true;
            // すべての確認が通ったら、Interactionを通じてViewに通知
            _ = await CloseWindowInteraction.Handle(Unit.Default);
        }
        finally
        {
        }
    }
}
