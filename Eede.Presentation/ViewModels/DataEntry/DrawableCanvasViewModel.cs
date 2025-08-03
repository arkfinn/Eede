using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Eede.Application.Colors;
using Eede.Application.Drawings;
using Eede.Domain.Colors;
using Eede.Domain.Drawing;
using Eede.Domain.DrawStyles;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using Eede.Domain.Sizes;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;

namespace Eede.Presentation.ViewModels.DataEntry;

public class DrawableCanvasViewModel : ViewModelBase
{
    [Reactive] public BackgroundColor BackgroundColor { get; set; }
    [Reactive] public Magnification Magnification { get; set; }
    [Reactive] public IDrawStyle DrawStyle { get; set; }
    [Reactive] public IImageBlender ImageBlender { get; set; }
    [Reactive] public ArgbColor PenColor { get; set; }
    [Reactive] public int PenSize { get; set; }
    [Reactive] public PenStyle PenStyle { get; set; }
    [Reactive] public IImageTransfer ImageTransfer { get; set; }
    [Reactive] public bool IsShifted { get; set; }
    [Reactive] public bool IsRegionSelecting { get; set; }
    [Reactive] public PictureArea SelectingArea { get; set; }
    [Reactive] public Thickness SelectingThickness { get; set; }
    [Reactive] public PictureSize SelectingSize { get; set; }

    public DrawableCanvasViewModel()
    {
        Magnification = new Magnification(4);
        DrawStyle = new FreeCurve();
        ImageBlender = new DirectImageBlender();
        PenColor = new ArgbColor(255, 0, 0, 0);
        PenSize = 1;
        PenStyle = new(ImageBlender, PenColor, PenSize);
        ImageTransfer = new DirectImageTransfer();
        IsRegionSelecting = false;
        SelectingThickness = new Thickness(0, 0, 0, 0);
        SelectingSize = new PictureSize(0, 0);
        //UpdatePicture(Picture.CreateEmpty(new PictureSize(32, 32)));


        OnColorPicked = ReactiveCommand.Create<ArgbColor>(ExecuteColorPicked);
        OnDrew = ReactiveCommand.Create<Picture>(ExecuteDrew);
        DrawBeginCommand = ReactiveCommand.Create<Position>(ExecuteDrawBeginAction);
        PointerRightButtonPressedCommand = ReactiveCommand.Create<Position>(ExecutePonterRightButtonPressedAction);
        DrawingCommand = ReactiveCommand.Create<Position>(ExecuteDrawingAction);
        DrawEndCommand = ReactiveCommand.Create<Position>(ExecuteDrawEndAction);
        CanvasLeaveCommand = ReactiveCommand.Create(ExecuteCanvasLeaveAction);

        Size defaultBoxSize = new(32, 32); //GlobalSetting.Instance().BoxSize;
        PictureSize gridSize = new(16, 16);
        DrawableArea = new(CanvasBackgroundService.Instance, new Magnification(1), gridSize, null);
        Picture picture = Picture.CreateEmpty(new PictureSize((int)defaultBoxSize.Width, (int)defaultBoxSize.Height));
        SetPicture(picture);

        _ = this.WhenAnyValue(x => x.ImageBlender, x => x.PenColor, x => x.PenSize)
            .Subscribe(x => PenStyle = new(ImageBlender, PenColor, PenSize));

        _ = this.WhenAnyValue(x => x.Magnification)
            .Subscribe(x =>
            {
                DrawableArea = DrawableArea.UpdateMagnification(Magnification);
                UpdateImage();
            });

        _ = this.WhenAnyValue(x => x.ImageTransfer)
            .Subscribe(x => UpdateImage());
        _ = this.WhenAnyValue(x => x.SelectingArea, x => x.Magnification)
            .Subscribe(area =>
            {
                if (SelectingArea != null)
                {
                    SelectingThickness = new Thickness(Magnification.Magnify(SelectingArea.X), Magnification.Magnify(SelectingArea.Y), 0, 0);
                    SelectingSize = new PictureSize(Magnification.Magnify(SelectingArea.Width), Magnification.Magnify(SelectingArea.Height));
                }
            });
    }



    private Bitmap _bitmap;
    public Bitmap MyBitmap
    {
        get => _bitmap;
        set
        {
            if (_bitmap != value)
            {
                _bitmap?.Dispose();
            }
            _ = this.RaiseAndSetIfChanged(ref _bitmap, value);
        }
    }

    private DrawableArea DrawableArea;
    public DrawingBuffer PictureBuffer;



    private Picture Picture = null;

    public ReactiveCommand<ArgbColor, Unit> OnColorPicked { get; }
    public event EventHandler<ColorPickedEventArgs> ColorPicked;
    private void ExecuteColorPicked(ArgbColor color)
    {
        ColorPicked?.Invoke(this, new ColorPickedEventArgs(color));
    }

    public ReactiveCommand<Picture, Unit> OnDrew { get; }
    public event Action<Picture, Picture> Drew;
    private void ExecuteDrew(Picture previous)
    {
        Drew?.Invoke(previous, Picture);
    }

    public void SetPicture(Picture source)
    {
        PictureBuffer = ContextFactory.Create(source);
        UpdateImage();
    }

    private void UpdateImage()
    {
        Picture = DrawableArea.Painted(PictureBuffer, PenStyle, ImageTransfer);
        PictureBitmapAdapter adapter = new();
        MyBitmap = PictureBitmapAdapter.ConvertToPremultipliedBitmap(Picture);
    }

    public ReactiveCommand<Position, Unit> DrawBeginCommand { get; }
    public ReactiveCommand<Position, Unit> PointerRightButtonPressedCommand { get; }
    public ReactiveCommand<Position, Unit> DrawingCommand { get; }
    public ReactiveCommand<Position, Unit> DrawEndCommand { get; }
    public ReactiveCommand<Unit, Unit> CanvasLeaveCommand { get; }

    private void ExecuteDrawBeginAction(Position pos)
    {
        if (PictureBuffer.IsDrawing())
        {
            return;
        }
        DrawingResult result = DrawableArea.DrawStart(DrawStyle, PenStyle, PictureBuffer, pos, IsShifted);
        PictureBuffer = result.PictureBuffer;
        DrawableArea = result.DrawableArea;
        UpdateImage();
    }

    private void ExecutePonterRightButtonPressedAction(Position pos)
    {
        if (PictureBuffer.IsDrawing())
        {
            ExecuteDrawCancelAction();
        }
        else
        {
            ArgbColor newColor = DrawableArea.PickColor(PictureBuffer.Fetch(), pos);
            ExecuteColorPicked(newColor);
        }
    }
    private void ExecuteDrawCancelAction()
    {
        DrawingResult result = DrawableArea.DrawCancel(PictureBuffer);
        PictureBuffer = result.PictureBuffer;
        DrawableArea = result.DrawableArea;
        UpdateImage();
    }

    private void ExecuteDrawingAction(Position pos)
    {
        DrawingResult result = DrawableArea.Move(DrawStyle, PenStyle, PictureBuffer, pos, IsShifted);
        PictureBuffer = result.PictureBuffer;
        DrawableArea = result.DrawableArea;
        UpdateImage();
    }

    private void ExecuteDrawEndAction(Position pos)
    {
        if (!PictureBuffer.IsDrawing())
        {
            return;
        }
        Picture previous = PictureBuffer.Previous;

        DrawingResult result = DrawableArea.DrawEnd(DrawStyle, PenStyle, PictureBuffer, new Position(pos.X, pos.Y), IsShifted);
        PictureBuffer = result.PictureBuffer;
        DrawableArea = result.DrawableArea;

        Drew?.Invoke(previous, PictureBuffer.Previous);
        UpdateImage();
    }

    private void ExecuteCanvasLeaveAction()
    {
        DrawableArea = DrawableArea.Leave(PictureBuffer);
        UpdateImage();
    }


    public RegionSelector SetupRegionSelector()
    {
        RegionSelector tool = new();
        tool.OnDrawStart += (sender, args) =>
        {
            IsRegionSelecting = false;
        };
        tool.OnDrawing += (sender, args) =>
        {
            SelectingArea = PictureArea.FromPosition(args.Start, args.Now, PictureBuffer.Previous.Size);
            IsRegionSelecting = true;
        };
        tool.OnDrawEnd += (sender, args) =>
        {
            SelectingArea = PictureArea.FromPosition(args.Start, args.Now, PictureBuffer.Previous.Size);
            IsRegionSelecting = true;
        };
        return tool;
    }
}
