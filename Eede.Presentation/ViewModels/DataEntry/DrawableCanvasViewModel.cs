using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Eede.Application.Colors;
using Eede.Application.Pictures;
using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Threading.Tasks;
using Eede.Application.Services;
using Eede.Application.UseCase.Pictures;

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
    [Reactive] public PictureArea? SelectingArea { get; set; }
    [Reactive] public Thickness SelectingThickness { get; set; }
    [Reactive] public PictureSize SelectingSize { get; set; }
    [Reactive] public Position PreviewPosition { get; set; }
    [Reactive] public Picture? PreviewPixels { get; set; }
    [Reactive] public Thickness PreviewThickness { get; set; }
    [Reactive] public PictureSize PreviewSize { get; set; }
    [Reactive] public Thickness RawPreviewThickness { get; set; }
    [Reactive] public PictureSize RawPreviewSize { get; set; }
    [Reactive] public Bitmap? MagnifiedPreviewBitmap { get; set; }
    [Reactive] public bool IsAnimationMode { get; set; }
    [Reactive] public GridSettings GridSettings { get; set; }
    [Reactive] public Cursor ActiveCursor { get; set; }

    private readonly GlobalState _globalState;
    private readonly IAddFrameProvider _addFrameProvider;
    private readonly IClipboardService _clipboardService;
    private readonly IBitmapAdapter<Bitmap> _bitmapAdapter;
    private readonly IDrawingSessionProvider _drawingSessionProvider;
    private readonly CopySelectionUseCase _copySelectionUseCase;
    private readonly CutSelectionUseCase _cutSelectionUseCase;
    private readonly PasteFromClipboardUseCase _pasteFromClipboardUseCase;
    private readonly IInteractionCoordinator _coordinator;
    private readonly PictureSize _gridSize = new(16, 16);

    public ReactiveCommand<Unit, Unit> CopyCommand { get; }
    public ReactiveCommand<Unit, Unit> CutCommand { get; }
    public ReactiveCommand<Unit, Unit> PasteCommand { get; }
    public ReactiveCommand<Picture, Unit> InternalUpdateCommand { get; }

    public DrawableCanvasViewModel(
        GlobalState globalState,
        IAddFrameProvider addFrameProvider,
        IClipboardService clipboardService,
        IBitmapAdapter<Bitmap> bitmapAdapter,
        IDrawingSessionProvider drawingSessionProvider,
        CopySelectionUseCase copySelectionUseCase,
        CutSelectionUseCase cutSelectionUseCase,
        PasteFromClipboardUseCase pasteFromClipboardUseCase,
        IInteractionCoordinator coordinator)
    {
        _globalState = globalState;
        _addFrameProvider = addFrameProvider;
        _clipboardService = clipboardService;
        _bitmapAdapter = bitmapAdapter;
        _drawingSessionProvider = drawingSessionProvider;
        _copySelectionUseCase = copySelectionUseCase;
        _cutSelectionUseCase = cutSelectionUseCase;
        _pasteFromClipboardUseCase = pasteFromClipboardUseCase;
        _coordinator = coordinator;

        _coordinator.StateChanged += () =>
        {
            PictureBuffer = _coordinator.CurrentBuffer;
            SelectingArea = _coordinator.SelectingArea;
            IsRegionSelecting = _coordinator.IsRegionSelecting;
            PreviewPixels = _coordinator.PreviewPixels;
            PreviewPosition = _coordinator.PreviewPosition;
            ActiveCursor = _coordinator.ActiveCursor;
            UpdateImage();
        };

        _coordinator.Drew += (prev, current, area1, area2) => Drew?.Invoke(prev, current, area1, area2);

        this.WhenAnyValue(x => x.SelectingArea)
            .Subscribe(area =>
            {
                if (_drawingSessionProvider.CurrentSession != null && _drawingSessionProvider.CurrentSession.CurrentSelectingArea != area)
                {
                    _drawingSessionProvider.Update(_drawingSessionProvider.CurrentSession.UpdateSelectingArea(area));
                }
            });

        _drawingSessionProvider.SessionChanged += (session) =>
        {
            if (SelectingArea != session.CurrentSelectingArea)
            {
                SelectingArea = session.CurrentSelectingArea;
                UpdateImage();
            }
        };

        GridSettings = new GridSettings(new(32, 32), new(0, 0), 0);
        InternalUpdateCommand = ReactiveCommand.Create<Picture>(ExecuteInternalUpdate);
        
        Picture initialPicture = Picture.CreateEmpty(_globalState.BoxSize);
        SetPicture(initialPicture);

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

        OnColorPicked = ReactiveCommand.Create<ArgbColor>(ExecuteColorPicked);
        OnDrew = ReactiveCommand.Create<Picture>(ExecuteDrew);
        DrawBeginCommand = ReactiveCommand.Create<Position>(ExecuteDrawBeginAction);
        PointerRightButtonPressedCommand = ReactiveCommand.Create<Position>(ExecutePonterRightButtonPressedAction);
        DrawingCommand = ReactiveCommand.Create<Position>(ExecuteDrawingAction);
        DrawEndCommand = ReactiveCommand.Create<Position>(ExecuteDrawEndAction);
        CanvasLeaveCommand = ReactiveCommand.Create(ExecuteCanvasLeaveAction);

        CopyCommand = ReactiveCommand.CreateFromTask(ExecuteCopyAction);
        CutCommand = ReactiveCommand.CreateFromTask(ExecuteCutAction);
        PasteCommand = ReactiveCommand.CreateFromTask(ExecutePasteAction);

        _ = this.WhenAnyValue(x => x.DrawStyle)
            .Subscribe(x =>
            {
                if (x is RegionSelector selector)
                {
                    SetupRegionSelector(selector);
                }
            });

        _ = this.WhenAnyValue(x => x.ImageBlender, x => x.PenColor, x => x.PenSize)
            .Subscribe(x => PenStyle = new(ImageBlender, PenColor, PenSize));

        _ = this.WhenAnyValue(x => x.Magnification)
            .Subscribe(x =>
            {
                _coordinator.UpdateMagnification(Magnification);
                UpdateImage();
            });

        _ = this.WhenAnyValue(x => x.ImageTransfer)
            .Subscribe(x => UpdateImage());

        _ = this.WhenAnyValue(x => x.SelectingArea, x => x.Magnification, (area, mag) => (area, mag))
            .Subscribe(x =>
            {
                if (x.area.HasValue)
                {
                    var displayPos = new CanvasCoordinate(x.area.Value.X, x.area.Value.Y).ToDisplay(x.mag);
                    var displaySize = new CanvasCoordinate(x.area.Value.Width, x.area.Value.Height).ToDisplay(x.mag);
                    SelectingThickness = new Thickness(displayPos.X, displayPos.Y, 0, 0);
                    SelectingSize = new PictureSize(displaySize.X, displaySize.Y);
                }
                else
                {
                    SelectingThickness = new Thickness(0, 0, 0, 0);
                    SelectingSize = new PictureSize(0, 0);
                }
            });

        _ = this.WhenAnyValue(x => x.PreviewPosition, x => x.PreviewPixels, x => x.Magnification, (pos, pix, mag) => (pos, pix, mag))
            .Subscribe(x =>
            {
                if (x.pix != null)
                {
                    var displayPos = new CanvasCoordinate(x.pos.X, x.pos.Y).ToDisplay(x.mag);
                    var displaySize = new CanvasCoordinate(x.pix.Size.Width, x.pix.Size.Height).ToDisplay(x.mag);

                    PreviewThickness = new Thickness(displayPos.X, displayPos.Y, 0, 0);
                    PreviewSize = new PictureSize(displaySize.X, displaySize.Y);
                    RawPreviewThickness = new Thickness(x.pos.X, x.pos.Y, 0, 0);
                    RawPreviewSize = x.pix.Size;

                    var magnifiedPicture = ImageTransfer.Transfer(x.pix, x.mag);
                    var previewBase = Picture.CreateEmpty(magnifiedPicture.Size);
                    var simulatedPreview = previewBase.Blend(new DirectImageBlender(), magnifiedPicture, new Position(0, 0));
                    MagnifiedPreviewBitmap = _bitmapAdapter.ConvertToPremultipliedBitmap(simulatedPreview);
                }
                else
                {
                    MagnifiedPreviewBitmap = null;
                }
            });

        Picture picture = Picture.CreateEmpty(_globalState.BoxSize);
        SetPicture(picture);
    }

    public void SetupRegionSelector(RegionSelector selector)
    {
        _coordinator.SetupRegionSelector(selector, PictureBuffer, IsAnimationMode, IsAnimationMode ? GridSettings.CellSize : _gridSize);
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
        if (PictureBuffer == null || PenStyle == null || ImageTransfer == null)
        {
            return;
        }

        Picture = _coordinator.Painted(PictureBuffer, PenStyle, ImageTransfer);
        if (Picture == null) return;

        MyBitmap = _bitmapAdapter.ConvertToPremultipliedBitmap(Picture);
    }

    public ReactiveCommand<Position, Unit> DrawBeginCommand { get; }
    public ReactiveCommand<Position, Unit> PointerRightButtonPressedCommand { get; }
    public ReactiveCommand<Position, Unit> DrawingCommand { get; }
    public ReactiveCommand<Position, Unit> DrawEndCommand { get; }
    public ReactiveCommand<Unit, Unit> CanvasLeaveCommand { get; }

    private void ExecuteDrawBeginAction(Position pos)
    {
        _coordinator.PointerBegin(pos, PictureBuffer, DrawStyle, PenStyle, IsShifted, IsAnimationMode, IsAnimationMode ? GridSettings.CellSize : _gridSize, InternalUpdateCommand);
    }

    private void ExecutePonterRightButtonPressedAction(Position pos)
    {
        _coordinator.PointerRightButtonPressed(pos, PictureBuffer, DrawStyle, IsAnimationMode, IsAnimationMode ? GridSettings.CellSize : _gridSize, (color) => ExecuteColorPicked(color), InternalUpdateCommand);
    }

    private void ExecuteDrawingAction(Position pos)
    {
        _coordinator.PointerMoved(pos, PictureBuffer, DrawStyle, PenStyle, IsShifted, IsAnimationMode, IsAnimationMode ? GridSettings.CellSize : _gridSize);
    }

    private void ExecuteDrawEndAction(Position pos)
    {
        _coordinator.PointerLeftButtonReleased(pos, PictureBuffer, DrawStyle, IsAnimationMode, IsAnimationMode ? GridSettings.CellSize : _gridSize, PenStyle, IsShifted, InternalUpdateCommand);
    }

    private void ExecuteCanvasLeaveAction()
    {
        _coordinator.CanvasLeave(PictureBuffer);
    }

    private async Task ExecuteCopyAction()
    {
        if (PictureBuffer == null) return;

        try
        {
            await _copySelectionUseCase.Execute(PictureBuffer.Previous, IsRegionSelecting ? SelectingArea : null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Copy failed: {ex}");
        }
    }

    private async Task ExecuteCutAction()
    {
        if (PictureBuffer == null) return;

        try
        {
            Picture previous = PictureBuffer.Previous;
            PictureArea? previousArea = IsRegionSelecting ? SelectingArea : null;
            Picture cleared = await _cutSelectionUseCase.Execute(previous, previousArea);
            ExecuteInternalUpdate(cleared);
            // TODO: Reset selection in coordinator if needed
            Drew?.Invoke(previous, cleared, previousArea, null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cut failed: {ex}");
        }
    }

    private async Task ExecutePasteAction()
    {
        if (PictureBuffer == null) return;

        try
        {
            Picture? pasted = await _pasteFromClipboardUseCase.Execute();
            if (pasted == null) return;
            // TODO: delegate to coordinator
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Paste failed: {ex}");
        }
    }
}