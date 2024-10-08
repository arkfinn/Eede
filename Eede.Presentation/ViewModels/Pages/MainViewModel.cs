﻿using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Dock.Model.Core;
using Eede.Application.Pictures;
using Eede.Domain.Colors;
using Eede.Domain.DrawStyles;
using Eede.Domain.Files;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Scales;
using Eede.Domain.Systems;
using Eede.Presentation.Actions;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Drawings;
using Eede.Presentation.Common.Pictures.Actions;
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
using System.Web;

namespace Eede.Presentation.ViewModels.Pages;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<DockPictureViewModel> Pictures { get; } = [];
    public DrawableCanvasViewModel DrawableCanvasViewModel { get; } = new DrawableCanvasViewModel();

    public Magnification Magnification
    {
        get => DrawableCanvasViewModel.Magnification;
        set => DrawableCanvasViewModel.Magnification = value;
    }

    [Reactive] public DrawStyles DrawStyle { get; set; }

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

    public int PenWidth
    {
        get => DrawableCanvasViewModel.PenSize;
        set => DrawableCanvasViewModel.PenSize = value;
    }

    [Reactive] public IImageBlender PullBlender { get; set; }
    [Reactive] public IDockable ActiveDockable { get; set; }

    [Reactive] public List<int> MinCursorSizeList { get; set; }
    [Reactive] public int MinCursorWidth { get; set; }
    [Reactive] public int MinCursorHeight { get; set; }
    [Reactive] public PictureSize CursorSize { get; set; }

    [Reactive] public UndoSystem UndoSystem { get; private set; }

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

    public PaletteContainerViewModel PaletteContainerViewModel { get; } = new PaletteContainerViewModel();

    public MainViewModel()
    {
        ImageTransfer = new DirectImageTransfer();
        PenColor = DrawableCanvasViewModel.PenColor;
        PullBlender = new DirectImageBlender();
        _ = this.WhenAnyValue(x => x.PenColor).BindTo(this, x => x.DrawableCanvasViewModel.PenColor);
        MinCursorSizeList =
        [
            8, 16, 24, 32, 48, 64
        ];
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
        DrawStyle = DrawStyles.Free;
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

        PaletteContainerViewModel.OnApplyColor += OnApplyPaletteColor;
        PaletteContainerViewModel.OnFetchColor += OnFetchPaletteColor;
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
            Pictures.Add(OpenPicture(file.Path));
        }
    }

    private async void ExecuteLoadPicture(StorageService storage)
    {
        Uri result = await storage.OpenFilePickerAsync();
        if (result == null)
        {
            return;
        }
        Pictures.Add(OpenPicture(result));
    }

    private readonly BitmapFileReader BitmapFileReader = new();
    private DockPictureViewModel OpenPicture(Uri path)
    {
        DockPictureViewModel vm = DockPictureViewModel.FromFile(BitmapFileReader.Read(path));
        return SetupDockPicture(vm);
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
                vm.Save(storage);
            }
        }
    }

    private async void OnPictureSave(object sender, PictureSaveEventArgs e)
    {
        BitmapFile file = e.File;
        string fullPath;

        if (file.IsNewFile())
        {
            Uri result = await e.Storage.SaveFilePickerAsync();
            if (result == null)
            {
                return;
            }
            fullPath = HttpUtility.UrlDecode(result.AbsolutePath);
        }
        else
        {
            fullPath = file.GetPathString();
        }

        file.Bitmap.Save(fullPath);
        e.UpdateFile(file with { Path = new FilePath(fullPath) });
    }

    private void OnPushToDrawArea(object sender, PicturePushEventArgs args)
    {
        if (sender is not DockPictureViewModel vm)
        {
            return;
        }
        PictureBitmapAdapter adapter = new();
        Picture from = adapter.ConvertToPicture(vm.Bitmap).CutOut(args.Rect);
        Picture previous = DrawableCanvasViewModel.PictureBuffer.Previous;
        UndoSystem = UndoSystem.Add(new UndoItem(
            new Action(() => { SetPictureToDrawArea(previous); }),
            new Action(() => { SetPictureToDrawArea(from); })));

        SetPictureToDrawArea(from);
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
        PictureBitmapAdapter adapter = new();
        Bitmap previous = vm.Bitmap;
        Bitmap now = adapter.ConvertToBitmap(
            adapter.ConvertToPicture(vm.Bitmap)
                .Blend(PullBlender, DrawableCanvasViewModel.PictureBuffer.Previous, args.Position));

        UndoSystem = UndoSystem.Add(new UndoItem(
           new Action(() => { if (vm.Enabled) { vm.Bitmap = previous; } }),
           new Action(() => { if (vm.Enabled) { vm.Bitmap = now; } })));
        vm.Bitmap = now;
    }

    private void ExecutePictureAction(PictureActions actionType)
    {
        Picture previous = DrawableCanvasViewModel.PictureBuffer.Previous;

        Picture updatedPicture;
        if (DrawableCanvasViewModel.IsRegionSelecting)
        {
            Picture region = previous.CutOut(DrawableCanvasViewModel.SelectingArea);
            Picture updatedRegion = actionType.Execute(region);
            DirectImageBlender blender = new();
            updatedPicture = blender.Blend(updatedRegion, previous, DrawableCanvasViewModel.SelectingArea.Position);
        }
        else
        {
            updatedPicture = actionType.Execute(previous);
        }
        UndoSystem = UndoSystem.Add(new UndoItem(
                   new Action(() => { SetPictureToDrawArea(previous); }),
                   new Action(() => { SetPictureToDrawArea(updatedPicture); })));
        SetPictureToDrawArea(updatedPicture);
    }

    private IDrawStyle ExecuteUpdateDrawStyle(DrawStyles drawStyle)
    {
        DrawableCanvasViewModel.IsRegionSelecting = false;
        return drawStyle switch
        {
            DrawStyles.RegionSelect => DrawableCanvasViewModel.SetupRegionSelector(),
            DrawStyles.Free => new FreeCurve(),
            DrawStyles.Line => new Line(),
            DrawStyles.Fill => new Fill(),
            _ => throw new ArgumentException(null, nameof(drawStyle)),
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
}
