using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Dock.Model.Core;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.Pictures;
using Eede.Domain.Pictures.Actions;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Actions;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Common.Services;
using Eede.Presentation.Events;
using Eede.Presentation.Files;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
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

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<DockPictureViewModel> Pictures { get; } = [];
    public DrawableCanvasViewModel DrawableCanvasViewModel { get; } = new DrawableCanvasViewModel();

    [Reactive] public BackgroundColor CurrentBackgroundColor { get; set; }

    public Magnification Magnification
    {
        get => DrawableCanvasViewModel.Magnification;
        set => DrawableCanvasViewModel.Magnification = value;
    }

    [Reactive] public DrawStyleType DrawStyle { get; set; }

    public IImageBlender ImageBlender
    {
        get => DrawableCanvasViewModel.ImageBlender;
        set => DrawableCanvasViewModel.ImageBlender = value;
    }

    public IImageTransfer ImageTransfer
    {
        get => DrawableCanvasViewModel.ImageTransfer;
        set => DrawableCanvasViewModel.ImageTransfer = value;
    }

    [Reactive] public ArgbColor PenColor { get; set; }
    [Reactive] public Color NowPenColor { get; set; }
    [Reactive] public Color SampleColor { get; set; }

    public int PenWidth
    {
        get => DrawableCanvasViewModel.PenSize;
        set => DrawableCanvasViewModel.PenSize = value;
    }

    [Reactive] public IImageBlender PullBlender { get; set; }
    [Reactive] public IDockable ActiveDockable { get; set; }

    [Reactive] public ObservableCollection<int> MinCursorSizeList { get; set; }
    [Reactive] public int MinCursorWidth { get; set; }
    [Reactive] public int MinCursorHeight { get; set; }
    [Reactive] public PictureSize CursorSize { get; set; }

    [Reactive] public UndoSystem UndoSystem { get; private set; }
    [Reactive] public StorageService StorageService { get; set; }

    public ReactiveCommand<Unit, Unit> UndoCommand { get; }
    public ReactiveCommand<Unit, Unit> RedoCommand { get; }
    public ReactiveCommand<StorageService, Unit> LoadPictureCommand { get; }
    public ReactiveCommand<StorageService, Unit> SavePictureCommand { get; }
    public ReactiveCommand<PictureActions, Unit> PictureActionCommand { get; }
    public ReactiveCommand<int, Unit> PutPaletteColorCommand { get; }
    public ReactiveCommand<int, Unit> GetPaletteColorCommand { get; }

    public Interaction<NewPictureWindowViewModel, NewPictureWindowViewModel> ShowCreateNewPictureModal { get; }
    public ReactiveCommand<Unit, Unit> CreateNewPictureCommand { get; }

    public ReactiveCommand<StorageService, Unit> LoadPaletteCommand { get; }
    public ReactiveCommand<StorageService, Unit> SavePaletteCommand { get; }
    public ReactiveCommand<Unit, Unit> PutBackgroundColorCommand { get; }
    public ReactiveCommand<Unit, Unit> GetBackgroundColorCommand { get; }
    public PaletteContainerViewModel PaletteContainerViewModel { get; } = new PaletteContainerViewModel();


    // Viewにウィンドウを閉じるよう通知するためのInteraction
    public Interaction<Unit, Unit> CloseWindowInteraction { get; }

    // Viewからのクローズ要求を受け取るためのコマンド
    public ReactiveCommand<Unit, Unit> RequestCloseCommand { get; }

    private bool _isCloseConfirmed;
    public bool IsCloseConfirmed
    {
        get => _isCloseConfirmed;
        private set => this.RaiseAndSetIfChanged(ref _isCloseConfirmed, value);
    }

    public MainViewModel()
    {
        ImageTransfer = new DirectImageTransfer();
        CurrentBackgroundColor = BackgroundColor.Default;
        _ = this.WhenAnyValue(x => x.CurrentBackgroundColor)
            .BindTo(this, x => x.DrawableCanvasViewModel.BackgroundColor);
        PullBlender = new DirectImageBlender();
        PenColor = DrawableCanvasViewModel.PenColor;
        _ = this.WhenAnyValue(x => x.PenColor)
            .Select(x => Color.FromArgb(x.Alpha, x.Red, x.Green, x.Blue))
            .BindTo(this, x => x.NowPenColor);
        _ = this.WhenAnyValue(x => x.PenColor).BindTo(this, x => x.DrawableCanvasViewModel.PenColor);
        MinCursorSizeList = new ObservableCollection<int>([8, 16, 24, 32, 48, 64]);
        MinCursorWidth = 32;
        MinCursorHeight = 32;
        _ = this.WhenAnyValue(x => x.MinCursorWidth, x => x.MinCursorHeight)
            .Subscribe(x =>
            {
                PictureSize size = new(MinCursorWidth, MinCursorHeight);
                foreach (DockPictureViewModel vm in Pictures)
                {
                    vm.MinCursorSize = size;
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

        DrawableCanvasViewModel.ColorPicked += (sender, args) =>
        {
            PenColor = args.NewColor;
        };
        UndoSystem = new();
        DrawableCanvasViewModel.Drew += (previous, now) =>
        {
            UndoSystem = UndoSystem.Add(new UndoItem(
                new Action(() => { SetPictureToDrawArea(previous); }),
                new Action(() => { SetPictureToDrawArea(now); })));
        };

        UndoCommand = ReactiveCommand.Create(ExecuteUndo, this.WhenAnyValue(
            x => x.UndoSystem,
            (undoSystem) => undoSystem.CanUndo()));
        RedoCommand = ReactiveCommand.Create(ExecuteRedo, this.WhenAnyValue(
           x => x.UndoSystem,
           (undoSystem) => undoSystem.CanRedo()));
        LoadPictureCommand = ReactiveCommand.Create<StorageService>(ExecuteLoadPicture);
        SavePictureCommand = ReactiveCommand.Create<StorageService>(ExecuteSavePicture);
        PictureActionCommand = ReactiveCommand.Create<PictureActions>(ExecutePictureAction);

        ShowCreateNewPictureModal = new Interaction<NewPictureWindowViewModel, NewPictureWindowViewModel>();
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

        CloseWindowInteraction = new Interaction<Unit, Unit>();
        RequestCloseCommand = ReactiveCommand.CreateFromTask(RequestCloseAsync);
    }

    private void ExecuteUndo()
    {
        UndoSystem = UndoSystem.Undo();
    }

    private void ExecuteRedo()
    {
        UndoSystem = UndoSystem.Redo();
    }

    public void DragOverPicture(object sender, DragEventArgs e)
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

        IEnumerable<IStorageItem> files = dataObject.GetFiles();
        // TODO: 拡張子チェックしたい
        // ?.Where(file=> file.Path.);
        if (files is null)
        {
            return;
        }

        e.DragEffects = DragDropEffects.Copy;
        e.Handled = true;
    }

    public void DropPicture(object sender, DragEventArgs e)
    {
        if (e.Data is not IDataObject dataObject)
        {
            return;
        }

        if (dataObject.GetDataFormats().Contains(DataFormats.Files) == false)
        {
            return;
        }

        IEnumerable<IStorageItem> files = dataObject.GetFiles();
        // TODO: 拡張子チェックしたい
        // ?.Where(file=> file.Path.);
        if (files is null)
        {
            return;
        }

        foreach (IStorageItem file in files)
        {
            DockPictureViewModel newPicture = OpenPicture(file.Path);
            if (newPicture != null)
            {
                Pictures.Add(newPicture);
            }
        }
    }

    private async void ExecuteLoadPicture(StorageService storage)
    {
        Uri result = await storage.OpenFilePickerAsync();
        if (result == null)
        {
            return;
        }
        DockPictureViewModel newPicture = OpenPicture(result);
        if (newPicture != null)
        {
            Pictures.Add(newPicture);
        }

    }

    private DockPictureViewModel OpenPicture(Uri path)
    {
        IImageFile imageFile = ReadBitmapFile(path);
        if (imageFile == null)
        {
            // エラーが発生した場合、またはファイルが読み込めなかった場合はnullを返す
            return null;
        }
        DockPictureViewModel vm = DockPictureViewModel.FromFile(imageFile);
        return SetupDockPicture(vm);
    }

    private IImageFile ReadBitmapFile(Uri path)
    {
        return new BitmapFileReader().Read(path);
    }

    private DockPictureViewModel SetupDockPicture(DockPictureViewModel vm)
    {
        vm.PicturePush += OnPushToDrawArea;
        vm.PicturePull += OnPullFromDrawArea;
        vm.PictureSave += OnPictureSave;
        vm.MinCursorSize = new PictureSize(MinCursorWidth, MinCursorHeight);
        return vm;
    }

    private async void ExecuteCreateNewPicture()
    {
        NewPictureWindowViewModel store = new();
        NewPictureWindowViewModel result = await ShowCreateNewPictureModal.Handle(store);
        if (result.Result)
        {
            Pictures.Add(SetupDockPicture(DockPictureViewModel.FromSize(result.Size)));
        }
    }

    private void ExecuteSavePicture(StorageService storage)
    {
        if (ActiveDockable is Dock.Model.Avalonia.Controls.Document doc)
        {
            if (doc.DataContext is DockPictureViewModel vm)
            {
                _ = vm.Save();
            }
        }
    }

    private async Task OnPictureSave(object sender, PictureSaveEventArgs e)
    {
        SaveImageResult saveResult = await e.File.SaveAsync(StorageService);
        if (saveResult.IsCanceled)
        {
            e.Cancel();
            return;
        }
        if (saveResult.IsSaved)
        {
            e.UpdateFile(saveResult.File);
        }
    }

    private void OnPushToDrawArea(object sender, PicturePushEventArgs args)
    {
        if (sender is not DockPictureViewModel vm)
        {
            return;
        }
        PictureBitmapAdapter adapter = new();
        PictureEditingUseCase.EditResult result = PictureEditingUseCase.PushToCanvas(
            DrawableCanvasViewModel.PictureBuffer.Previous,
            vm.PictureBuffer,
            args.Rect);

        UndoSystem = UndoSystem.Add(new UndoItem(
            new Action(() => { SetPictureToDrawArea(result.Previous); }),
            new Action(() => { SetPictureToDrawArea(result.Updated); })));

        SetPictureToDrawArea(result.Updated);
    }

    private void SetPictureToDrawArea(Picture picture)
    {
        DrawableCanvasViewModel.SetPicture(picture);
        CursorSize = picture.Size;
    }

    private void OnPullFromDrawArea(object sender, PicturePullEventArgs args)
    {
        if (sender is not DockPictureViewModel vm)
        {
            return;
        }
        PictureEditingUseCase.EditResult result = PictureEditingUseCase.PullFromCanvas(
            vm.PictureBuffer,
            DrawableCanvasViewModel.PictureBuffer.Previous,
            args.Position,
            PullBlender);

        UndoSystem = UndoSystem.Add(new UndoItem(
           new Action(() => { if (vm.Enabled) { vm.PictureBuffer = result.Previous; } }),
           new Action(() => { if (vm.Enabled) { vm.PictureBuffer = result.Updated; } })));

        vm.PictureBuffer = result.Updated;
    }

    private void ExecutePictureAction(PictureActions actionType)
    {
        PictureEditingUseCase.EditResult result = DrawableCanvasViewModel.IsRegionSelecting ? PictureEditingUseCase.ExecuteAction(
            DrawableCanvasViewModel.PictureBuffer.Previous,
            actionType,
            DrawableCanvasViewModel.SelectingArea
        ) : PictureEditingUseCase.ExecuteAction(
            DrawableCanvasViewModel.PictureBuffer.Previous,
            actionType
        );
        UndoSystem = UndoSystem.Add(new UndoItem(
         new Action(() => { SetPictureToDrawArea(result.Previous); }),
         new Action(() => { SetPictureToDrawArea(result.Updated); })));

        SetPictureToDrawArea(result.Updated);
    }

    private IDrawStyle ExecuteUpdateDrawStyle(DrawStyleType drawStyle)
    {
        DrawableCanvasViewModel.IsRegionSelecting = false;
        return drawStyle switch
        {
            DrawStyleType.RegionSelect => DrawableCanvasViewModel.SetupRegionSelector(),
            DrawStyleType.FreeCurve => new FreeCurve(),
            DrawStyleType.Line => new Line(),
            DrawStyleType.Fill => new Fill(),
            _ => throw new ArgumentOutOfRangeException(nameof(drawStyle), $"Unknown DrawStyle: {drawStyle}"),
        };
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
