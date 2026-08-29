using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.VisualTree;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common;
using Eede.Presentation.ViewModels.DataEntry;
using System;
using System.Windows.Input;

namespace Eede.Presentation.Views.DataEntry
{
    public partial class DrawableCanvas : UserControl
    {
        private readonly ZoomAndPanController _zoomAndPanController = new();
        private ScrollViewer? _scrollViewer;
        private ScrollViewer? ScrollViewer => _scrollViewer ??= this.FindAncestorOfType<ScrollViewer>();

        public DrawableCanvas()
        {
            InitializeComponent();

            this.PointerWheelChanged += OnPointerWheelChanged;
            this.PointerEntered += OnPointerEntered;
            this.PointerCaptureLost += OnPointerCaptureLost;
            _zoomAndPanController.SpaceStateChanged += OnSpaceStateChanged;

            canvas.PointerPressed += OnCanvasPointerPressed;
            canvas.PointerMoved += OnCanvasPointerMoved;
            canvas.PointerReleased += OnCanvasPointerReleased;
            canvas.PointerExited += OnCanvasPointerExited;

            // UserControl自身でRequestBringIntoViewイベントをインターセプトする
            this.AddHandler(RequestBringIntoViewEvent, (s, e) => e.Handled = true);

            canvas.KeyDown += OnKeyDown;
            canvas.KeyUp += OnKeyUp;
            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
        }

        private void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            canvas.Focus();
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (DataContext is DrawableCanvasViewModel vm)
            {
                _zoomAndPanController.HandleWheel(
                    ScrollViewer,
                    e,
                    vm.Magnification,
                    mag => vm.Magnification = mag,
                    () => this.UpdateLayout());
            }
        }

        public static readonly StyledProperty<ICommand?> PointerLeftButtonPressedCommandProperty =
            AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerLeftButtonPressedCommand));
        public ICommand? PointerLeftButtonPressedCommand
        {
            get => GetValue(PointerLeftButtonPressedCommandProperty);
            set => SetValue(PointerLeftButtonPressedCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand?> PointerMovedCommandProperty =
            AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerMovedCommand));
        public ICommand? PointerMovedCommand
        {
            get => GetValue(PointerMovedCommandProperty);
            set => SetValue(PointerMovedCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand?> PointerLeftButtonReleasedCommandProperty =
            AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerLeftButtonReleasedCommand));
        public ICommand? PointerLeftButtonReleasedCommand
        {
            get => GetValue(PointerLeftButtonReleasedCommandProperty);
            set => SetValue(PointerLeftButtonReleasedCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand?> PointerRightButtonPressedCommandProperty =
            AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerRightButtonPressedCommand));
        public ICommand? PointerRightButtonPressedCommand
        {
            get => GetValue(PointerRightButtonPressedCommandProperty);
            set => SetValue(PointerRightButtonPressedCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand?> PointerLeaveCommandProperty =
            AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerLeaveCommand));
        public ICommand? PointerLeaveCommand
        {
            get => GetValue(PointerLeaveCommandProperty);
            set => SetValue(PointerLeaveCommandProperty, value);
        }

        public static readonly StyledProperty<bool> IsShiftedProperty =
            AvaloniaProperty.Register<DrawableCanvas, bool>(nameof(IsShifted), false, defaultBindingMode: BindingMode.TwoWay);
        public bool IsShifted
        {
            get => GetValue(IsShiftedProperty);
            set => SetValue(IsShiftedProperty, value);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            _zoomAndPanController.HandleKeyDown(e);
            if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
            {
                IsShifted = true;
            }
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            _zoomAndPanController.HandleKeyUp(e);
            if ((e.KeyModifiers & KeyModifiers.Shift) == 0)
            {
                IsShifted = false;
            }
        }

        private void OnSpaceStateChanged(bool isSpacePressed)
        {
            if (isSpacePressed)
            {
                Cursor = new Cursor(StandardCursorType.Hand);
            }
            else
            {
                if (DataContext is DrawableCanvasViewModel vm)
                {
                    Cursor = vm.ActiveCursor;
                }
                else
                {
                    Cursor = Cursor.Default;
                }
            }
        }

        private bool IsLeftButtonPressing = false;
        private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            canvas.Focus();

            if (_zoomAndPanController.HandlePointerPressed(ScrollViewer, canvas, e))
            {
                return;
            }

            Point pos = e.GetPosition(canvas);
            PointerPointProperties pointer = e.GetCurrentPoint(canvas).Properties;
            if (pointer.IsLeftButtonPressed)
            {
                PointerLeftButtonPressedCommand?.Execute(new Position((int)pos.X, (int)pos.Y));
                IsLeftButtonPressing = true;
            }

            if (pointer.IsRightButtonPressed)
            {
                PointerRightButtonPressedCommand?.Execute(new Position((int)pos.X, (int)pos.Y));
            }
        }

        private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_zoomAndPanController.HandlePointerMoved(ScrollViewer, canvas, e))
            {
                return;
            }

            Point pos = e.GetPosition(canvas);
            if (IsLeftButtonPressing && e.GetCurrentPoint(canvas).Properties.IsRightButtonPressed)
            {
                PointerRightButtonPressedCommand?.Execute(new Position((int)pos.X, (int)pos.Y));
                IsLeftButtonPressing = false;
            }
            PointerMovedCommand?.Execute(new Position((int)pos.X, (int)pos.Y));
        }

        private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_zoomAndPanController.HandlePointerReleased(canvas, e))
            {
                return;
            }

            if (IsLeftButtonPressing)
            {
                Point pos = e.GetPosition(canvas);
                PointerLeftButtonReleasedCommand?.Execute(new Position((int)pos.X, (int)pos.Y));
            }
            IsLeftButtonPressing = false;
        }

        private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            _zoomAndPanController.HandlePointerCaptureLost(e);
            IsLeftButtonPressing = false;
        }

        private void OnCanvasPointerExited(object? sender, EventArgs e)
        {
            _zoomAndPanController.Reset();
            PointerLeaveCommand?.Execute(null);
        }
    }
}
