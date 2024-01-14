using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eede.Application.Colors;
using Eede.Application.Drawings;
using Eede.Common.Drawings;
using Eede.Domain.Colors;
using Eede.Domain.Drawings;
using Eede.Domain.DrawStyles;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using Eede.Presentation.Common.Adapters;
using Eede.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Runtime.InteropServices;

namespace Eede.ViewModels.DataEntry;

public class DrawableCanvasViewModel : ViewModelBase
{
    public DrawableCanvasViewModel()
    {
        Magnification = new Magnification(4);
        DrawStyle = new FreeCurve();
        ImageBlender = new DirectImageBlender();
        PenColor = new ArgbColor(255, 0, 0, 0);
        PenSize = 1;
        PenStyle = new(ImageBlender, PenColor, PenSize);
        ImageTransfer = new DirectImageTransfer();

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

        this.WhenAnyValue(x => x.ImageBlender, x => x.PenColor, x => x.PenSize)
            .Subscribe(x => PenStyle = new(ImageBlender, PenColor, PenSize));

        this.WhenAnyValue(x => x.Magnification)
            .Subscribe(x =>
            {
                DrawableArea = DrawableArea.UpdateMagnification(Magnification);
                UpdateImage();
            });

        this.WhenAnyValue(x => x.ImageTransfer)
            .Subscribe(x => UpdateImage());
    }

    [Reactive] public Magnification Magnification { get; set; }
    [Reactive] public IDrawStyle DrawStyle { get; set; }
    [Reactive] public IImageBlender ImageBlender { get; set; }
    [Reactive] public ArgbColor PenColor { get; set; }
    [Reactive] public int PenSize { get; set; }
    [Reactive] public PenStyle PenStyle { get; set; }
    [Reactive] public IImageTransfer ImageTransfer { get; set; }
    [Reactive] public bool IsShifted { get; set; }

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
            this.RaiseAndSetIfChanged(ref _bitmap, value);
        }
    }

    private DrawableArea DrawableArea;
    public DrawingBuffer PictureBuffer;

    private Picture Picture = null;

    public ReactiveCommand<ArgbColor, Unit> OnColorPicked { get; }
    public event EventHandler<ColorPickedEventArgs>? ColorPicked;
    private void ExecuteColorPicked(ArgbColor color)
    {
        ColorPicked?.Invoke(this, new ColorPickedEventArgs(color));
    }

    public ReactiveCommand<Picture, Unit> OnDrew { get; }
    public event Action<Picture, Picture>? Drew;
    private void ExecuteDrew(Picture previous)
    {
        Drew?.Invoke(previous, Picture);
    }

    public void SetPicture(Picture source)
    {
        PictureBuffer = new DrawingBuffer(source);
        UpdateImage();
    }

    private void UpdateImage()
    {
        Picture = DrawableArea.Painted(PictureBuffer, PenStyle, ImageTransfer);
        PictureBitmapAdapter adapter = new();
        MyBitmap = adapter.ConvertToBitmap(Picture);
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
        PictureBuffer = result.PictureBuffer.Clone();
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
            var newColor = DrawableArea.PickColor(PictureBuffer.Fetch(), pos);
            ExecuteColorPicked(newColor);
        }
    }
    private void ExecuteDrawCancelAction()
    {
        DrawingResult result = DrawableArea.DrawCancel(PictureBuffer);
        PictureBuffer = result.PictureBuffer.Clone();
        DrawableArea = result.DrawableArea;
        UpdateImage();
    }

    private void ExecuteDrawingAction(Position pos)
    {
        DrawingResult result = DrawableArea.Move(DrawStyle, PenStyle, PictureBuffer, pos, IsShifted);
        PictureBuffer = result.PictureBuffer.Clone();
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

        DrawingResult result = DrawableArea.DrawEnd(DrawStyle, PenStyle, PictureBuffer, new Position((int)pos.X, (int)pos.Y), IsShifted);
        PictureBuffer = result.PictureBuffer.Clone();
        DrawableArea = result.DrawableArea;

        Drew?.Invoke(previous, PictureBuffer.Previous);
        UpdateImage();
    }

    private void ExecuteCanvasLeaveAction()
    {
        DrawableArea = DrawableArea.Leave(PictureBuffer);
        UpdateImage();
    }

}
