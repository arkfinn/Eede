using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using System;
using System.Windows.Input;

namespace Eede.Presentation.Views.DataEntry;

public partial class PaletteForm : UserControl
{
    private const int CELL_WIDTH = 17;
    private const int CELL_HEIGHT = 11;

    public static readonly StyledProperty<Palette> PaletteProperty =
        AvaloniaProperty.Register<PaletteForm, Palette>(nameof(Palette), Palette.Create());
    public Palette Palette
    {
        get => GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public static readonly StyledProperty<ICommand?> PointerLeftButtonPressedCommandProperty =
        AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerLeftButtonPressedCommand));
    public ICommand? PointerLeftButtonPressedCommand
    {
        get => GetValue(PointerLeftButtonPressedCommandProperty);
        set => SetValue(PointerLeftButtonPressedCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> PointerRightButtonPressedCommandProperty =
        AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerRightButtonPressedCommand));
    public ICommand? PointerRightButtonPressedCommand
    {
        get => GetValue(PointerRightButtonPressedCommandProperty);
        set => SetValue(PointerRightButtonPressedCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> PointerColorReplacedCommandProperty =
        AvaloniaProperty.Register<PaletteForm, ICommand?>(nameof(PointerColorReplacedCommand));
    public ICommand? PointerColorReplacedCommand
    {
        get => GetValue(PointerColorReplacedCommandProperty);
        set => SetValue(PointerColorReplacedCommandProperty, value);
    }

    public static readonly StyledProperty<bool> IsRegisterModeProperty =
        AvaloniaProperty.Register<PaletteForm, bool>(nameof(IsRegisterMode), false);
    public bool IsRegisterMode { get => GetValue(IsRegisterModeProperty); set => SetValue(IsRegisterModeProperty, value); }

    private const double DRAG_THRESHOLD = 5.0;
    private const int TOUCH_LONG_PRESS_MS = 450;
    private Point? _dragStartPosition;
    private int _dragStartSlot = -1;
    private int _hoverSlot = -1;
    private bool _isDragging;
    private bool _touchLongPressHandled;
    private DispatcherTimer? _touchLongPressTimer;

    public PaletteForm()
    {
        InitializeComponent();
        int width = CELL_WIDTH * 16;
        int height = CELL_HEIGHT * 16;
        PaletteBuffer = Picture.CreateEmpty(new PictureSize(width, height));
        panel.Width = width;
        panel.Height = height;
        image.Width = width;
        image.Height = height;
        PaletteBitmap = AvaloniaBitmapAdapter.StaticConvertToBitmap(PaletteBuffer);
        image.Source = PaletteBitmap;
        Refresh();

        _ = this.GetObservable(PaletteProperty).Subscribe(_ =>
        {
            Refresh();
        });

        image.PointerPressed += OnCanvasPointerPressed;
        image.PointerMoved += OnCanvasPointerMoved;
        image.PointerReleased += OnCanvasPointerReleased;
        image.PointerCaptureLost += OnCanvasPointerCaptureLost;
    }

    private readonly Picture PaletteBuffer;
    public Bitmap PaletteBitmap;

    private void Refresh()
    {
        Picture Buffer = PaletteBuffer;
        var directBlender = new DirectImageBlender();
        Palette.ForEach((color, index) =>
        {
            Drawer drawer = new(Buffer, new PenStyle(directBlender, color, 1));
            int x = index % 16 * CELL_WIDTH;
            int y = index / 16 * CELL_HEIGHT;
            var result = drawer.DrawFillRectangle(new Position(x, y), new Position(x + CELL_WIDTH - 1, y + CELL_HEIGHT - 1));
            Buffer = result.Picture;
        });

        if (_isDragging)
        {
            if (_dragStartSlot >= 0 && _dragStartSlot < Palette.MAX_LENGTH)
            {
                Buffer = DrawSlotBorder(Buffer, _dragStartSlot, new ArgbColor(255, 255, 255, 0));
            }
            if (_hoverSlot >= 0 && _hoverSlot < Palette.MAX_LENGTH && _hoverSlot != _dragStartSlot)
            {
                Buffer = DrawSlotBorder(Buffer, _hoverSlot, new ArgbColor(255, 0, 255, 255));
            }
        }

        PaletteBitmap?.Dispose();
        PaletteBitmap = AvaloniaBitmapAdapter.StaticConvertToBitmap(Buffer);
        image.Source = PaletteBitmap;
    }

    private static Picture DrawSlotBorder(Picture buffer, int slot, ArgbColor borderColor)
    {
        int x = slot % 16 * CELL_WIDTH;
        int y = slot / 16 * CELL_HEIGHT;
        Drawer drawer = new(buffer, new PenStyle(new DirectImageBlender(), borderColor, 1));
        return drawer.DrawRectangle(new Position(x, y), new Position(x + CELL_WIDTH - 1, y + CELL_HEIGHT - 1)).Picture;
    }

    private static int GetSlotFromPosition(Point pos)
    {
        int col = (int)Math.Floor(pos.X / CELL_WIDTH);
        int row = (int)Math.Floor(pos.Y / CELL_HEIGHT);
        if (col < 0 || col >= 16 || row < 0 || row >= 16)
        {
            return -1;
        }
        return col + (row * 16);
    }

    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Point pos = e.GetPosition(image);
        int number = GetSlotFromPosition(pos);
        if (number < 0) return;

        if (e.Pointer.Type == PointerType.Touch)
        {
            HandleTouchPressSlot(number, pos, e.Pointer);
            e.Handled = true;
            return;
        }

        PointerPointProperties pointer = e.GetCurrentPoint(image).Properties;
        if (pointer.IsLeftButtonPressed)
        {
            _dragStartPosition = pos;
            _dragStartSlot = number;
            _hoverSlot = number;
            _isDragging = false;
            e.Pointer.Capture(image);
        }

        if (pointer.IsRightButtonPressed)
        {
            PointerRightButtonPressedCommand?.Execute(number);
        }
    }

    internal void HandleTouchPressSlot(int number, Point pos, IPointer? pointer = null)
    {
        _touchLongPressTimer?.Stop();
        _touchLongPressHandled = false;
        if (IsRegisterMode)
        {
            PointerRightButtonPressedCommand?.Execute(number);
            return;
        }

        _dragStartPosition = pos;
        _dragStartSlot = number;
        _hoverSlot = number;
        _isDragging = false;
        pointer?.Capture(image);
        _touchLongPressTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(TOUCH_LONG_PRESS_MS), DispatcherPriority.Input, (_, _) => TriggerTouchLongPress());
        _touchLongPressTimer.Start();
    }

    internal bool TriggerTouchLongPress()
    {
        _touchLongPressTimer?.Stop();
        if (_dragStartPosition == null || _isDragging || _dragStartSlot < 0) return false;
        int slot = _dragStartSlot;
        _touchLongPressHandled = true;
        ResetDragState();
        PointerRightButtonPressedCommand?.Execute(slot);
        return true;
    }

    private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragStartPosition == null || _dragStartSlot < 0) return;

        Point pos = e.GetPosition(image);
        if (!_isDragging)
        {
            double dx = pos.X - _dragStartPosition.Value.X;
            double dy = pos.Y - _dragStartPosition.Value.Y;
            if ((dx * dx) + (dy * dy) >= DRAG_THRESHOLD * DRAG_THRESHOLD)
            {
                _touchLongPressTimer?.Stop();
                _isDragging = true;
                _hoverSlot = GetSlotFromPosition(pos);
                Refresh();
                return;
            }
        }

        if (_isDragging)
        {
            int nextHover = GetSlotFromPosition(pos);
            if (_hoverSlot != nextHover)
            {
                _hoverSlot = nextHover;
                Refresh();
            }
        }
    }

    private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _touchLongPressTimer?.Stop();
        if (e.Pointer.Type == PointerType.Touch && _touchLongPressHandled)
        {
            _touchLongPressHandled = false;
            e.Pointer.Capture(null);
            e.Handled = true;
            return;
        }

        if (_dragStartPosition == null || _dragStartSlot < 0) return;

        if (e.InitialPressMouseButton == MouseButton.Left || e.Pointer.Type == PointerType.Touch)
        {
            Point pos = e.GetPosition(image);
            int dropSlot = GetSlotFromPosition(pos);
            bool wasDragging = _isDragging;
            int startSlot = _dragStartSlot;

            ResetDragState();
            e.Pointer.Capture(null);

            if (wasDragging)
            {
                Refresh();
                if (dropSlot >= 0 && dropSlot != startSlot)
                {
                    PointerColorReplacedCommand?.Execute((startSlot, dropSlot));
                }
            }
            else
            {
                PointerLeftButtonPressedCommand?.Execute(startSlot);
            }
        }
    }

    private void OnCanvasPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _touchLongPressTimer?.Stop();
        _touchLongPressHandled = false;
        bool wasDragging = _isDragging;
        ResetDragState();
        if (wasDragging)
        {
            Refresh();
        }
    }

    private void ResetDragState()
    {
        _dragStartPosition = null;
        _dragStartSlot = -1;
        _hoverSlot = -1;
        _isDragging = false;
    }
}
