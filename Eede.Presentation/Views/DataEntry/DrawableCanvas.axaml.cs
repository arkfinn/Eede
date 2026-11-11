using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common;
using Eede.Presentation.ViewModels.DataEntry;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Eede.Presentation.Views.DataEntry
{
    public partial class DrawableCanvas : UserControl
    {
        private readonly ZoomAndPanController _zoomAndPanController = new();
        private Vector _currentMargin = new(0, 0);
        private bool _isInitialCentered = false;
        private readonly List<IDisposable> _dataContextDisposables = new();

        public DrawableCanvas()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;

            this.PointerWheelChanged += OnPointerWheelChanged;
            this.PointerEntered += OnPointerEntered;
            this.PointerCaptureLost += OnPointerCaptureLost;
            _zoomAndPanController.SpaceStateChanged += OnSpaceStateChanged;

            scrollViewer.AddHandler(InputElement.PointerPressedEvent, OnScrollViewerPointerPressed, RoutingStrategies.Tunnel);
            scrollViewer.AddHandler(InputElement.PointerMovedEvent, OnScrollViewerPointerMoved, RoutingStrategies.Tunnel);
            scrollViewer.AddHandler(InputElement.PointerReleasedEvent, OnScrollViewerPointerReleased, RoutingStrategies.Tunnel);
            scrollViewer.PointerCaptureLost += (s, e) => _zoomAndPanController.HandlePointerCaptureLost(e);
            scrollViewer.GetObservable(ScrollViewer.ViewportProperty).Subscribe(OnViewportChanged);

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

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            foreach (var d in _dataContextDisposables)
            {
                d.Dispose();
            }
            _dataContextDisposables.Clear();

            if (DataContext is DrawableCanvasViewModel vm)
            {
                var disposable = vm.WhenAnyValue(x => x.DisplayWidth, x => x.DisplayHeight)
                    .Where(t => t.Item1 > 0 && t.Item2 > 0)
                    .DistinctUntilChanged()
                    .Subscribe(_ =>
                    {
                        global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            CenterContent();
                        });
                    });
                _dataContextDisposables.Add(disposable);
            }
        }

        private void OnViewportChanged(Size viewport)
        {
            if (viewport.Width <= 0 || viewport.Height <= 0) return;

            var newMargin = ZoomHelper.CalculateOverscrollMargin(viewport);
            if (_currentMargin != newMargin)
            {
                var oldMargin = _currentMargin;
                _currentMargin = newMargin;
                canvasGrid.Margin = new Thickness(newMargin.X, newMargin.Y, newMargin.X, newMargin.Y);

                if (_isInitialCentered && oldMargin.X > 0 && oldMargin.Y > 0)
                {
                    var delta = newMargin - oldMargin;
                    scrollViewer.Offset = new Vector(
                        Math.Max(0, scrollViewer.Offset.X + delta.X),
                        Math.Max(0, scrollViewer.Offset.Y + delta.Y));
                }
            }

            if (!_isInitialCentered && DataContext is DrawableCanvasViewModel vm && vm.DisplayWidth > 0)
            {
                CenterContent();
            }
        }

        public void CenterContent()
        {
            if (DataContext is not DrawableCanvasViewModel vm || scrollViewer == null) return;
            var viewport = scrollViewer.Viewport;
            if (viewport.Width <= 0 || viewport.Height <= 0) return;

            var displaySize = new Size(vm.DisplayWidth, vm.DisplayHeight);
            if (displaySize.Width <= 0 || displaySize.Height <= 0) return;

            scrollViewer.Offset = ZoomHelper.CalculateInitialOffset(_currentMargin, displaySize, viewport);
            _isInitialCentered = true;
        }

        private void OnScrollViewerPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(scrollViewer).Properties;
            bool isMiddle = props.IsMiddleButtonPressed;
            bool isSpaceLeft = _zoomAndPanController.IsSpacePressed && props.IsLeftButtonPressed;

            if (isMiddle || isSpaceLeft)
            {
                canvas.Focus();
                if (_zoomAndPanController.HandlePointerPressed(scrollViewer, scrollViewer, e))
                {
                    e.Handled = true;
                }
            }
        }

        private void OnScrollViewerPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_zoomAndPanController.IsPanning)
            {
                _zoomAndPanController.HandlePointerMoved(scrollViewer, scrollViewer, e);
                e.Handled = true;
            }
        }

        private void OnScrollViewerPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_zoomAndPanController.IsPanning)
            {
                _zoomAndPanController.HandlePointerReleased(scrollViewer, e);
                e.Handled = true;
            }
        }

        private void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            canvas.Focus();
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (DataContext is DrawableCanvasViewModel vm && scrollViewer != null)
            {
                _zoomAndPanController.HandleWheel(
                    scrollViewer,
                    e,
                    vm.Magnification,
                    mag => vm.Magnification = mag,
                    () => this.UpdateLayout(),
                    null,
                    _currentMargin);
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

            if (_zoomAndPanController.HandlePointerPressed(scrollViewer, canvas, e))
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
            if (_zoomAndPanController.HandlePointerMoved(scrollViewer, canvas, e))
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
            if (_zoomAndPanController.IsPanning)
            {
                return;
            }
            _zoomAndPanController.Reset();
            PointerLeaveCommand?.Execute(null);
        }
    }
}
