using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Eede.Application.Colors;
using Eede.Application.Common.SelectionStates;
using Eede.Application.Drawings;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
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
    [Reactive] public Picture PreviewPixels { get; set; }
    [Reactive] public Position PreviewPosition { get; set; }
    [Reactive] public Thickness PreviewThickness { get; set; }
    [Reactive] public PictureSize PreviewSize { get; set; }
    [Reactive] public Thickness RawPreviewThickness { get; set; }
    [Reactive] public PictureSize RawPreviewSize { get; set; }
    [Reactive] public Bitmap MagnifiedPreviewBitmap { get; set; }
    [Reactive] public Cursor ActiveCursor { get; set; }
    [Reactive] public bool IsAnimationMode { get; set; }
    [Reactive] public GridSettings GridSettings { get; set; }

    private ISelectionState _selectionState;
    private readonly ReactiveCommand<Picture, Unit> InternalUpdateCommand;
    private readonly PictureSize _gridSize;

    private readonly GlobalState _globalState;

    public DrawableCanvasViewModel(GlobalState globalState)
    {
        _globalState = globalState;
        _gridSize = new(16, 16);
        InternalUpdateCommand = ReactiveCommand.Create<Picture>(ExecuteInternalUpdate);
        _selectionState = new NormalCursorState(HalfBoxArea.Create(new Position(0, 0), _gridSize));

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
        ActiveCursor = Cursor.Default;
        //UpdatePicture(Picture.CreateEmpty(new PictureSize(32, 32)));


        OnColorPicked = ReactiveCommand.Create<ArgbColor>(ExecuteColorPicked);
        OnDrew = ReactiveCommand.Create<Picture>(ExecuteDrew);
        DrawBeginCommand = ReactiveCommand.Create<Position>(ExecuteDrawBeginAction);
        PointerRightButtonPressedCommand = ReactiveCommand.Create<Position>(ExecutePonterRightButtonPressedAction);
        DrawingCommand = ReactiveCommand.Create<Position>(ExecuteDrawingAction);
        DrawEndCommand = ReactiveCommand.Create<Position>(ExecuteDrawEndAction);
        CanvasLeaveCommand = ReactiveCommand.Create(ExecuteCanvasLeaveAction);

        // Size defaultBoxSize = new(32, 32); //GlobalSetting.Instance().BoxSize;
        DrawableArea = new(CanvasBackgroundService.Instance, new Magnification(1), _gridSize, null);

        _ = this.WhenAnyValue(x => x.DrawStyle)
            .Subscribe(x =>
            {
                if (x is not RegionSelector)
                {
                    _selectionState = new NormalCursorState(HalfBoxArea.Create(new Position(0, 0), _gridSize));
                    IsRegionSelecting = false;
                    PreviewPixels = null;
                    UpdateImage();
                }
            });

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
        _ = this.WhenAnyValue(x => x.SelectingArea, x => x.Magnification, (area, mag) => (area, mag))
            .Subscribe(x =>
            {
                if (x.area != null)
                {
                    SelectingThickness = new Thickness(x.mag.Magnify(x.area.X), x.mag.Magnify(x.area.Y), 0, 0);
                    SelectingSize = new PictureSize(x.mag.Magnify(x.area.Width), x.mag.Magnify(x.area.Height));
                }
            });

        _ = this.WhenAnyValue(x => x.PreviewPosition, x => x.PreviewPixels, x => x.Magnification, (pos, pix, mag) => (pos, pix, mag))
            .Subscribe(x =>
            {
                if (x.pix != null)
                {
                    PreviewThickness = new Thickness(x.mag.Magnify(x.pos.X), x.mag.Magnify(x.pos.Y), 0, 0);
                    PreviewSize = new PictureSize(x.mag.Magnify(x.pix.Width), x.mag.Magnify(x.pix.Height));
                    RawPreviewThickness = new Thickness(x.pos.X, x.pos.Y, 0, 0);
                    RawPreviewSize = new PictureSize(x.pix.Width, x.pix.Height);

                    // 既存のMagnificationプロセスを使用
                    var magnifiedPicture = ImageTransfer.Transfer(x.pix, x.mag);
                    
                    // プレビュー表示を「確定後」の状態に近づけるため、
                    // 透明な部分が下の画像を透過させるのではなく、
                    // 確定時と同じく「背景が見える状態」をシミュレートする
                    var previewBase = Picture.CreateEmpty(magnifiedPicture.Size);
                    var simulatedPreview = previewBase.Blend(new DirectImageBlender(), magnifiedPicture, new Position(0, 0));
                    
                    _ = new PictureBitmapAdapter();
                    MagnifiedPreviewBitmap = PictureBitmapAdapter.ConvertToPremultipliedBitmap(simulatedPreview);
                }
                else
                {
                    MagnifiedPreviewBitmap = null;
                }
            });

        Picture picture = Picture.CreateEmpty(_globalState.BoxSize);
        SetPicture(picture);
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

    private void ExecuteInternalUpdate(Picture picture)
    {
        if (picture != null && PictureBuffer != null)
        {
            PictureBuffer = PictureBuffer.Reset(picture);
            UpdateImage();
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
    public event Action<Picture, Picture, PictureArea?, PictureArea?> Drew;
    private void ExecuteDrew(Picture previous)
    {
        Drew?.Invoke(previous, Picture, null, null);
    }

    public void SetPicture(Picture source)
    {
        if (source == null) return;
        PictureBuffer = ContextFactory.Create(source);
        UpdateImage();
    }

    private void UpdateImage()
    {
        Picture = DrawableArea.Painted(PictureBuffer, PenStyle, ImageTransfer);
        if (Picture == null)
        {
            return;
        }

        var info = _selectionState?.GetSelectionPreviewInfo();
        if (info != null)
        {
            PreviewPixels = info.Pixels;
            PreviewPosition = info.Position;
            
            // メイン画像にプレビューを直接合成して表示する
            var magnifiedPreview = ImageTransfer.Transfer(info.Pixels, Magnification);
            var magnifiedPosition = new Position(Magnification.Magnify(info.Position.X), Magnification.Magnify(info.Position.Y));
            Picture = Picture.Blend(new DirectImageBlender(), magnifiedPreview, magnifiedPosition);
        }
        else
        {
            PreviewPixels = null;
        }

        _ = new PictureBitmapAdapter();
        MyBitmap = PictureBitmapAdapter.ConvertToPremultipliedBitmap(Picture);
    }

    public ReactiveCommand<Position, Unit> DrawBeginCommand { get; }
    public ReactiveCommand<Position, Unit> PointerRightButtonPressedCommand { get; }
    public ReactiveCommand<Position, Unit> DrawingCommand { get; }
    public ReactiveCommand<Position, Unit> DrawEndCommand { get; }
    public ReactiveCommand<Unit, Unit> CanvasLeaveCommand { get; }

    private void UpdateCursorSize(Position pos)
    {
        if (_selectionState == null) return;
        var selectionCursor = _selectionState.GetCursor(Minify(pos));
        ActiveCursor = selectionCursor switch
        {
            SelectionCursor.Move => new Cursor(StandardCursorType.SizeAll),
            _ => Cursor.Default
        };
    }

    private void ExecuteDrawBeginAction(Position pos)
    {
        if (PictureBuffer == null || PictureBuffer.IsDrawing())
        {
            return;
        }

        UpdateCursorSize(pos);

        ISelectionState nextState = _selectionState.HandlePointerLeftButtonPressed(
            GetCurrentHalfBoxArea(pos),
            Minify(pos),
            null, // pullAction はここでは不要（またはダミー）
            () => PictureBuffer.Previous,
            InternalUpdateCommand);

        if (nextState is DraggingState)
        {
            _selectionState = nextState;
            DrawableArea = DrawableArea.Leave(PictureBuffer);
            UpdateImage();
            return;
        }
        _selectionState = nextState;

        DrawingResult result = DrawableArea.DrawStart(DrawStyle, PenStyle, PictureBuffer, pos, IsShifted);
        PictureBuffer = result.PictureBuffer;
        DrawableArea = result.DrawableArea;
        UpdateImage();
    }

    private HalfBoxArea GetCurrentHalfBoxArea(Position pos)
    {
        return HalfBoxArea.Create(Minify(pos), _gridSize);
    }

    private Position Minify(Position pos)
    {
        return new Position(Magnification.Minify(pos.X), Magnification.Minify(pos.Y));
    }

    private void ExecutePonterRightButtonPressedAction(Position pos)
    {
        (ISelectionState nextState, HalfBoxArea _) = _selectionState.HandlePointerRightButtonPressed(GetCurrentHalfBoxArea(pos), Minify(pos), _gridSize, InternalUpdateCommand);
        if (_selectionState is DraggingState && nextState is SelectedState selected)
        {
            SelectingArea = selected.Selection.Area;
            IsRegionSelecting = true;
            _selectionState = nextState;
            UpdateImage();
            return;
        }
        _selectionState = nextState;

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
        if (_selectionState == null || PictureBuffer == null)
        {
            return;
        }

        UpdateCursorSize(pos);

        (_, _) = _selectionState.HandlePointerMoved(GetCurrentHalfBoxArea(pos), true, Minify(pos), PictureBuffer.Previous.Size);
        if (_selectionState is DraggingState)
        {
            DrawableArea = DrawableArea.Leave(PictureBuffer);
            UpdateImage();
            return;
        }

        DrawingResult result = DrawableArea.Move(DrawStyle, PenStyle, PictureBuffer, pos, IsShifted);
        PictureBuffer = result.PictureBuffer;
        DrawableArea = result.DrawableArea;
        UpdateImage();
    }

    private void ExecuteDrawEndAction(Position pos)
    {
        if (_selectionState == null || PictureBuffer == null)
        {
            return;
        }
        if (_selectionState is DraggingState dragging)
        {
            var previous = dragging.OriginalPicture;
            var previousArea = SelectingArea;
            var nextState = _selectionState.HandlePointerLeftButtonReleased(GetCurrentHalfBoxArea(pos), Minify(pos), null, InternalUpdateCommand);
            if (nextState is SelectedState selected)
            {
                SelectingArea = selected.Selection.Area;
                IsRegionSelecting = true;
                _selectionState = nextState;
                UpdateImage();
                Drew?.Invoke(previous, PictureBuffer.Previous, previousArea, SelectingArea);
            }
            else
            {
                _selectionState = nextState;
                UpdateImage();
            }
            return;
        }

        if (!PictureBuffer.IsDrawing())
        {
            return;
        }
        Picture previousImage = PictureBuffer.Previous;

        DrawingResult result = DrawableArea.DrawEnd(DrawStyle, PenStyle, PictureBuffer, new Position(pos.X, pos.Y), IsShifted);
        PictureBuffer = result.PictureBuffer;
        DrawableArea = result.DrawableArea;

        Drew?.Invoke(previousImage, PictureBuffer.Previous, null, null);
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
            _selectionState = new NormalCursorState(GetCurrentHalfBoxArea(args.Start));
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
            _selectionState = new SelectedState(new Selection(SelectingArea));
        };
        return tool;
    }
}
