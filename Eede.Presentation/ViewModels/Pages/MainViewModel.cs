using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Eede.Actions;
using Eede.Application.Pictures;
using Eede.Common.Enums;
using Eede.Common.Pictures.Actions;
using Eede.Domain.Colors;
using Eede.Domain.DrawStyles;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Pictures.Actions;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using Eede.Domain.Systems;
using Eede.Presentation.Actions;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Services;
using Eede.Ui;
using Eede.ViewModels.DataDisplay;
using Eede.ViewModels.DataEntry;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Eede.ViewModels.Pages;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<DockPictureViewModel> Pictures { get; } = new ObservableCollection<DockPictureViewModel>();
    public DrawableCanvasViewModel DrawableCanvasViewModel { get; } = new DrawableCanvasViewModel();

    public Magnification Magnification
    {
        get => DrawableCanvasViewModel.Magnification;
        set => DrawableCanvasViewModel.Magnification = value;
    }

    public IDrawStyle DrawStyle
    {
        get => DrawableCanvasViewModel.DrawStyle;
        set => DrawableCanvasViewModel.DrawStyle = value;
    }

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
    [Reactive] public IDockable? ActiveDockable { get; set; }

    [Reactive] public List<int> MinCursorSizeList { get; set; }
    [Reactive] public int MinCursorWidth { get; set; }
    [Reactive] public int MinCursorHeight { get; set; }

    [Reactive] public UndoSystem UndoSystem { get; private set; }

    public ReactiveCommand<Unit, Unit> UndoCommand { get; }
    public ReactiveCommand<Unit, Unit> RedoCommand { get; }
    public ReactiveCommand<StorageService, Unit> LoadPictureCommand { get; }
    public ReactiveCommand<Unit, Unit> SavePictureCommand { get; }
    public ReactiveCommand<PictureActions, Unit> PictureActionCommand { get; }
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

        DrawableCanvasViewModel.ColorPicked += (sender, args) =>
        {
            PenColor = args.NewColor;
        };
        UndoSystem = new();
        DrawableCanvasViewModel.Drew += (previous, now) =>
        {
            UndoSystem = UndoSystem.Add(new UndoItem(
                new Action(() => { DrawableCanvasViewModel.SetPicture(previous); }),
                new Action(() => { DrawableCanvasViewModel.SetPicture(now); })));
        };

        UndoCommand = ReactiveCommand.Create(ExecuteUndo, this.WhenAnyValue(
            x => x.UndoSystem,
            (undoSystem) => undoSystem.CanUndo()));
        RedoCommand = ReactiveCommand.Create(ExecuteRedo, this.WhenAnyValue(
           x => x.UndoSystem,
           (undoSystem) => undoSystem.CanRedo()));
        LoadPictureCommand = ReactiveCommand.Create<StorageService>(ExecuteLoadPicture);
        SavePictureCommand = ReactiveCommand.Create(ExecuteSavePicture);
        PictureActionCommand = ReactiveCommand.Create<PictureActions>(ExecutePictureAction);
    }


    private void ExecuteUndo()
    {
        UndoSystem = UndoSystem.Undo();
    }

    private void ExecuteRedo()
    {
        UndoSystem = UndoSystem.Redo();
    }

    public void DragOverPicture(object? sender, DragEventArgs e)
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

    public void DropPicture(object? sender, DragEventArgs e)
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
            new FilePickerFileType("All Images")
            {
                Patterns = new[] { "*.png", "*.bmp" },
                AppleUniformTypeIdentifiers = new[] { "public.image" },
                MimeTypes = new[] { "image/*" }
            },
            new FilePickerFileType("PNG Image")
            {
                Patterns = new[] { "*.png" },
                AppleUniformTypeIdentifiers = new[] { "public.png" },
                MimeTypes = new[] { "image/png" }
            },
            new FilePickerFileType("BMP Image")
            {
                Patterns = new[] { "*.bmp" },
                AppleUniformTypeIdentifiers = new[] { "public.bmp" },
                MimeTypes = new[] { "image/bmp" }
            },
            new FilePickerFileType("All")
            {
                Patterns = new[] { "*.*" },
                MimeTypes = new[] { "*/*" }
            }
        };
    }

    private DockPictureViewModel OpenPicture(Uri path)
    {
        var vm = DockPictureViewModel.FromUri(path);
        vm.PicturePush += PushToDrawArea;
        vm.PicturePull += PullFromDrawArea;
        vm.MinCursorSize = new PictureSize(MinCursorWidth, MinCursorHeight);
        return vm;
    }

    private void ExecuteSavePicture()
    {
        if (ActiveDockable is Dock.Model.Avalonia.Controls.Document doc)
        {
            if (doc.DataContext is DockPictureViewModel vm)
            {
                vm.Save();
            }
        }
    }

    private void PushToDrawArea(object? sender, PicturePushEventArgs args)
    {
        if (sender is not DockPictureViewModel vm)
        {
            return;
        }
        PictureBitmapAdapter adapter = new();
        Picture from = adapter.ConvertToPicture(vm.Bitmap).CutOut(args.Rect);
        Picture previous = DrawableCanvasViewModel.PictureBuffer.Previous;
        UndoSystem = UndoSystem.Add(new UndoItem(
            new Action(() => { DrawableCanvasViewModel.SetPicture(previous); }),
            new Action(() => { DrawableCanvasViewModel.SetPicture(from); })));

        DrawableCanvasViewModel.SetPicture(from);
    }

    private void PullFromDrawArea(object? sender, PicturePullEventArgs args)
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
        Picture updatedPicture = FindPictureAction(actionType, previous);
        UndoSystem = UndoSystem.Add(new UndoItem(
                   new Action(() => { DrawableCanvasViewModel.SetPicture(previous); }),
                   new Action(() => { DrawableCanvasViewModel.SetPicture(updatedPicture); })));
        DrawableCanvasViewModel.SetPicture(updatedPicture);
    }

    private Picture FindPictureAction(PictureActions actionType, Picture previous)
    {
        switch (actionType)
        {
            case PictureActions.ShiftUp:
                return new ShiftUpAction(previous).Execute();
            case PictureActions.ShiftDown:
                return new ShiftDownAction(previous).Execute();
            case PictureActions.ShiftLeft:
                return new ShiftLeftAction(previous).Execute();
            case PictureActions.ShiftRight:
                return new ShiftRightAction(previous).Execute();
            case PictureActions.HorizontalFlip:
                return new HorizontalFlipAction(previous).Execute();
            //case PictureActions.VerticalFlip:
            //    return;
            //case PictureActions.RotateLeft:
            //    return;
            case PictureActions.RotateRight:
                return new RotateRightAction(previous).Execute();
            default:
                throw new ArgumentException(nameof(actionType));
        }
    }
}
