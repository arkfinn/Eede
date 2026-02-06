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
using System.Reactive.Linq;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;
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
    [Reactive] public bool IsShowHandles { get; set; }
    [Reactive] public PictureArea? SelectingArea { get; set; }
    [Reactive] public Thickness SelectingThickness { get; set; }
    [Reactive] public PictureSize SelectingSize { get; set; }
    [Reactive] public Position PreviewPosition { get; set; }
    [Reactive] public Picture? PreviewPixels { get; set; }
    [Reactive] public Thickness PreviewThickness { get; set; }
    [Reactive] public PictureSize PreviewSize { get; set; }
    [Reactive] public Thickness RawPreviewThickness { get; set; }
    [Reactive] public PictureSize RawPreviewSize { get; set; }
    private Bitmap? _magnifiedPreviewBitmap;
    public Bitmap? MagnifiedPreviewBitmap
    {
        get => _magnifiedPreviewBitmap;
        set
        {
            if (_magnifiedPreviewBitmap != value)
            {
                _magnifiedPreviewBitmap?.Dispose();
            }
            _ = this.RaiseAndSetIfChanged(ref _magnifiedPreviewBitmap, value);
        }
    }
    [Reactive] public bool IsAnimationMode { get; set; }
    [Reactive] public GridSettings GridSettings { get; set; }
    [Reactive] public Cursor ActiveCursor { get; set; }
    [Reactive] public double HandleSize { get; set; }
    [Reactive] public Thickness HandleMargin { get; set; }

    [Reactive] public double DisplayWidth { get; set; }
    [Reactive] public double DisplayHeight { get; set; }

    [Reactive] public bool IsShowPixelGrid { get; set; }
    [Reactive] public bool IsShowCursorGrid { get; set; }
    [Reactive] public PictureSize CursorSize { get; set; }

    private readonly ObservableAsPropertyHelper<bool> _isPixelGridEffectivelyVisible;
    public bool IsPixelGridEffectivelyVisible => _isPixelGridEffectivelyVisible.Value;

    private readonly ObservableAsPropertyHelper<bool> _isCursorGridEffectivelyVisible;
    public bool IsCursorGridEffectivelyVisible => _isCursorGridEffectivelyVisible.Value;

    private readonly GlobalState _globalState;
    private readonly IAddFrameProvider _addFrameProvider;
    private readonly IClipboard _clipboard;
    private readonly IBitmapAdapter<Bitmap> _bitmapAdapter;
    private readonly IDrawingSessionProvider _drawingSessionProvider;
    private readonly ISelectionService _selectionService;
    private readonly IInteractionCoordinator _coordinator;
    private readonly PictureSize _gridSize = new(16, 16);
    private readonly IImageTransfer _identityTransfer = new IdentityImageTransfer();

    public ReactiveCommand<Unit, Unit> CopyCommand { get; }
    public ReactiveCommand<Unit, Unit> CutCommand { get; }
    public ReactiveCommand<Unit, Unit> PasteCommand { get; }
    public ReactiveCommand<Picture, Unit> InternalUpdateCommand { get; }

    public DrawableCanvasViewModel(
        GlobalState globalState,
        IAddFrameProvider addFrameProvider,
        IClipboard clipboard,
        IBitmapAdapter<Bitmap> bitmapAdapter,
        IDrawingSessionProvider drawingSessionProvider,
        ISelectionService selectionService,
        IInteractionCoordinator coordinator)
    {
        _globalState = globalState;
        _addFrameProvider = addFrameProvider;
        _clipboard = clipboard;

        _bitmapAdapter = bitmapAdapter;
        _drawingSessionProvider = drawingSessionProvider;
        _selectionService = selectionService;
        _coordinator = coordinator;

        _coordinator.StateChanged += () =>
        {
            PictureBuffer = _coordinator.CurrentBuffer;
            SelectingArea = _coordinator.SelectingArea;
            IsRegionSelecting = _coordinator.IsRegionSelecting;
            IsShowHandles = _coordinator.IsShowHandles;
            PreviewPixels = _coordinator.PreviewPixels;
            PreviewPosition = _coordinator.PreviewPosition;
            ActiveCursor = _coordinator.ActiveCursor;
            UpdateImage();
        };

        _coordinator.Drew += (prev, current, area1, area2) => Drew?.Invoke(prev, current, area1, area2);

        _drawingSessionProvider.SessionChanged += (session) =>
        {
            _coordinator.SyncWithSession();
            PictureBuffer = session.Buffer;
            SelectingArea = session.CurrentSelectingArea;
            PreviewPixels = session.CurrentPreviewContent?.Pixels;
            PreviewPosition = session.CurrentPreviewContent?.Position ?? new Position(0, 0);
            UpdateImage();
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
        HandleSize = 4.0;
        HandleMargin = new Thickness(-2, -2, 0, 0);
        CursorSize = new PictureSize(32, 32);

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

        _isPixelGridEffectivelyVisible = this.WhenAnyValue(
            x => x.IsShowPixelGrid,
            x => x.Magnification,
            (isShow, mag) => isShow && (mag != null && mag.Value >= 4))
            .ToProperty(this, x => x.IsPixelGridEffectivelyVisible);

        _isCursorGridEffectivelyVisible = this.WhenAnyValue(
            x => x.IsShowCursorGrid)
            .ToProperty(this, x => x.IsCursorGridEffectivelyVisible);

        _ = this.WhenAnyValue(x => x.DrawStyle)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                _coordinator.CommitSelection(x is not RegionSelector);
                _coordinator.ChangeDrawStyle(x);
                if (x is RegionSelector selector)
                {
                    SetupRegionSelector(selector);
                }
            });

        _ = this.WhenAnyValue(x => x.ImageBlender, x => x.PenColor, x => x.PenSize)
            .Subscribe(x => PenStyle = new(ImageBlender, PenColor, PenSize));

        _ = this.WhenAnyValue(x => x.ImageBlender)
            .Subscribe(x => _coordinator.ImageBlender = x);

        _ = this.WhenAnyValue(x => x.BackgroundColor)
            .Subscribe(x => _coordinator.BackgroundColor = x.Value);

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

        _ = this.WhenAnyValue(x => x.Magnification, x => x.PictureBuffer)
            .Subscribe(x =>
            {
                if (x.Item2 != null)
                {
                    DisplayWidth = x.Item1.Magnify(x.Item2.Previous.Width);
                    DisplayHeight = x.Item1.Magnify(x.Item2.Previous.Height);
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

                    // プレビューもIdentityTransferを使用し、View側で拡大させる
                    var previewBase = Picture.CreateEmpty(x.pix.Size);
                    var simulatedPreview = previewBase.Blend(ImageBlender, x.pix, new Position(0, 0));
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

    private DrawingBuffer _pictureBuffer;
    public DrawingBuffer PictureBuffer
    {
        get => _pictureBuffer;
        set => this.RaiseAndSetIfChanged(ref _pictureBuffer, value);
    }
    private Picture Picture = null;

    public ReactiveCommand<ArgbColor, Unit> OnColorPicked { get; }
    public event EventHandler<ColorPickedEventArgs> ColorPicked;
    private void ExecuteColorPicked(ArgbColor color)
    {
        PenColor = color;
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

        // 表示用には拡大を行わない _identityTransfer を使用する
        Picture = _coordinator.Painted(PictureBuffer, PenStyle, _identityTransfer);
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

    public void CommitSelection(bool forceClearSelection = false)
    {
        _coordinator.CommitSelection(forceClearSelection);
    }

    public void SyncWithSession(bool forceReset = false)
    {
        _coordinator.SyncWithSession(forceReset);
    }

    private async Task ExecuteCopyAction()
    {
        if (PictureBuffer == null) return;

        try
        {
            CommitSelection();
            await _selectionService.CopyAsync(PictureBuffer.Previous, IsRegionSelecting ? SelectingArea : null);
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
            CommitSelection();
            Picture previous = PictureBuffer.Previous;
            PictureArea? previousArea = IsRegionSelecting ? SelectingArea : null;
            Picture cleared = await _selectionService.CutAsync(previous, previousArea);
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
            CommitSelection();
            await _selectionService.PasteAsync();
            _coordinator.SyncWithSession();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Paste failed: {ex}");
        }
    }
}