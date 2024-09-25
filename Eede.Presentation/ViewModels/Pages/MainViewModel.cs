﻿using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Dock.Model.Core;
using Eede.Application.Pictures;
using Eede.Common.Drawings;
using Eede.Common.Pictures.Actions;
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
using Eede.Presentation.Common.Services;
using Eede.Presentation.Files;
using Eede.Presentation.Files.Pictures;
using Eede.ViewModels;
using Eede.ViewModels.DataDisplay;
using Eede.ViewModels.DataEntry;
using Eede.ViewModels.Pages;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Policy;
using System.Web;
using static System.Windows.Forms.Design.AxImporter;

namespace Eede.Presentation.ViewModels.Pages;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<DockPictureViewModel> Pictures { get; } = new ObservableCollection<DockPictureViewModel>();
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

    [Reactive] public Palette TempPalette { get; set; }

    public ReactiveCommand<Unit, Unit> UndoCommand { get; }
    public ReactiveCommand<Unit, Unit> RedoCommand { get; }
    public ReactiveCommand<StorageService, Unit> LoadPictureCommand { get; }
    public ReactiveCommand<StorageService, Unit> SavePictureCommand { get; }
    public ReactiveCommand<PictureActions, Unit> PictureActionCommand { get; }
    public ReactiveCommand<int, Unit> PutPaletteColorCommand { get; }
    public ReactiveCommand<int, Unit> GetPaletteColorCommand { get; }

    public Interaction<NewPictureWindowViewModel, NewPictureWindowViewModel> ShowCreateNewPictureModal { get; }
    public ReactiveCommand<Unit, Unit> CreateNewPictureCommand { get; }


    public MainViewModel()
    {
        ImageTransfer = new DirectImageTransfer();
        PenColor = DrawableCanvasViewModel.PenColor;
        PullBlender = new DirectImageBlender();
        this.WhenAnyValue(x => x.PenColor).BindTo(this, x => x.DrawableCanvasViewModel.PenColor);
        MinCursorSizeList = new()
        {
            8, 16, 24, 32, 48, 64
        };
        MinCursorWidth = 32;
        MinCursorHeight = 32;
        this.WhenAnyValue(x => x.MinCursorWidth, x => x.MinCursorHeight)
            .Subscribe(x =>
            {
                PictureSize size = new(MinCursorWidth, MinCursorHeight);
                foreach (var vm in Pictures)
                {
                    vm.MinCursorSize = size;
                }
            });

        this.WhenAnyValue(x => x.CursorSize)
           .Subscribe(size =>
           {
               foreach (var vm in Pictures)
               {
                   vm.CursorSize = size;
               }
           });
        DrawStyle = DrawStyles.Free;
        this.WhenAnyValue(x => x.DrawStyle).Subscribe(drawStyle => DrawableCanvasViewModel.DrawStyle = ExecuteUpdateDrawStyle(drawStyle));

        DrawableCanvasViewModel.ColorPicked += (sender, args) =>
        {
            PenColor = args.NewColor;
        };
        UndoSystem = new();
        DrawableCanvasViewModel.Drew += (previous, now) =>
        {
            UndoSystem = UndoSystem.Add(new UndoItem(
                new Action(() => { SetPicture(previous); }),
                new Action(() => { SetPicture(now); })));
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

        TempPalette = Palette.Create();
        PutPaletteColorCommand = ReactiveCommand.Create<int>(ExecutePutPaletteColor);
        GetPaletteColorCommand = ReactiveCommand.Create<int>(ExecuteGetPaletteColor);

        ShowCreateNewPictureModal = new Interaction<NewPictureWindowViewModel, NewPictureWindowViewModel>();
        CreateNewPictureCommand = ReactiveCommand.Create(ExecuteCreateNewPicture);
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

        if (e.Data is not IDataObject dataObject) return;
        if (dataObject.GetDataFormats().Contains(DataFormats.Files) == false) return;
        var files = dataObject.GetFiles();
        // TODO: 拡張子チェックしたい
        // ?.Where(file=> file.Path.);
        if (files is null) return;
        e.DragEffects = DragDropEffects.Copy;
        e.Handled = true;
    }

    public void DropPicture(object sender, DragEventArgs e)
    {
        if (e.Data is not IDataObject dataObject) return;
        if (dataObject.GetDataFormats().Contains(DataFormats.Files) == false) return;
        var files = dataObject.GetFiles();
        // TODO: 拡張子チェックしたい
        // ?.Where(file=> file.Path.);
        if (files is null) return;

        foreach (var file in files)
        {
            Pictures.Add(OpenPicture(file.Path));
        }
    }

    async void ExecuteLoadPicture(StorageService storage)
    {
        var options = new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = GetImageFileTypes(),
            //        Title = Title,
        };
        var result = await storage.StorageProvider.OpenFilePickerAsync(options);

        if (result == null || result.Count == 0)
        {
            return;
        }
        var uri = result[0].Path;
        Pictures.Add(OpenPicture(uri));
    }

    private static List<FilePickerFileType> GetImageFileTypes()
    {
        return new List<FilePickerFileType>
        {
            new("All Images")
            {
                Patterns = ["*.png", "*.bmp"],
                AppleUniformTypeIdentifiers = ["public.image"],
                MimeTypes = ["image/*"]
            },
            new("PNG Image")
            {
                Patterns = ["*.png"],
                AppleUniformTypeIdentifiers = ["public.png"],
                MimeTypes = ["image/png"]
            },
            new("BMP Image")
            {
                Patterns = ["*.bmp"],
                AppleUniformTypeIdentifiers = ["public.bmp"],
                MimeTypes = ["image/bmp"]
            },
            new("All")
            {
                Patterns = ["*.*"],
                MimeTypes = ["*/*"]
            }
        };
    }

    private readonly BitmapFileReader BitmapFileReader = new();
    private DockPictureViewModel OpenPicture(Uri path)
    {
        var vm = DockPictureViewModel.FromFile(BitmapFileReader.Read(path));
        return SetupDockPicture(vm);
    }

    private DockPictureViewModel SetupDockPicture(DockPictureViewModel vm)
    {
        vm.PicturePush += PushToDrawArea;
        vm.PicturePull += PullFromDrawArea;
        vm.MinCursorSize = new PictureSize(MinCursorWidth, MinCursorHeight);
        vm.PictureSave += async (sender, args) =>
        {
            var file = args.File;
            if (file.Path.IsEmpty())
            {
                var options = new FilePickerSaveOptions
                {

                };
                var result = await args.Storage.StorageProvider.SaveFilePickerAsync(options);

                if (result == null /*|| result.Count == 0)*/)
                {
                    return;
                }
                var uri = result.Path;

                var fullPath = HttpUtility.UrlDecode(uri.AbsolutePath);
                file.Bitmap.Save(fullPath);
                var updatedFile = new BitmapFile(
                    file.Bitmap,
                    new FilePath(fullPath));
                vm.Initialize(updatedFile);
            }
            else
            {
                var fullPath = file.Path.Path;
                file.Bitmap.Save(fullPath);
                vm.Initialize(file);
            }
        };
        return vm;
    }

    private async void ExecuteCreateNewPicture()
    {
        var store = new NewPictureWindowViewModel();
        var result = await ShowCreateNewPictureModal.Handle(store);
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

    private void PushToDrawArea(object sender, PicturePushEventArgs args)
    {
        if (sender is not DockPictureViewModel vm)
        {
            return;
        }
        PictureBitmapAdapter adapter = new();
        Picture from = adapter.ConvertToPicture(vm.Bitmap).CutOut(args.Rect);
        Picture previous = DrawableCanvasViewModel.PictureBuffer.Previous;
        UndoSystem = UndoSystem.Add(new UndoItem(
            new Action(() => { SetPicture(previous); }),
            new Action(() => { SetPicture(from); })));

        SetPicture(from);
    }

    private void SetPicture(Picture picture)
    {
        DrawableCanvasViewModel.SetPicture(picture);
        CursorSize = picture.Size;
    }

    private void PullFromDrawArea(object sender, PicturePullEventArgs args)
    {
        if (sender is not DockPictureViewModel vm)
        {

            return;
        }
        PictureBitmapAdapter adapter = new();
        var previous = vm.Bitmap;

        var now = adapter.ConvertToBitmap(
            adapter.ConvertToPicture(vm.Bitmap).Blend(PullBlender, DrawableCanvasViewModel.PictureBuffer.Previous, args.Position));

        UndoSystem = UndoSystem.Add(new UndoItem(
           new Action(() => { if (vm.Enabled) vm.Bitmap = previous; }),
           new Action(() => { if (vm.Enabled) vm.Bitmap = now; })));
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
                   new Action(() => { SetPicture(previous); }),
                   new Action(() => { SetPicture(updatedPicture); })));
        SetPicture(updatedPicture);
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

    #region パレット
    private void ExecutePutPaletteColor(int number)
    {
        TempPalette = TempPalette.Set(number, PenColor);
    }

    private void ExecuteGetPaletteColor(int number)
    {
        PenColor = TempPalette.Get(number);
    }
    #endregion

}
