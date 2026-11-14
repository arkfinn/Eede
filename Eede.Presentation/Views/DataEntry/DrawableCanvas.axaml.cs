using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
        private readonly TouchGestureTracker _touchTracker = new();
        private DispatcherTimer? _touchLongPressTimer;
        private bool _isTouchDrawing, _isTouchSingleFingerPanning;
        private Point _touchPanLastViewportPos, _touchLastCanvasPos;
        private Vector _currentMargin = new(0, 0);
        private bool _isInitialCentered = false;
        private bool _hasPendingCenter = false;
        private bool _isCentering = false;
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
            scrollViewer.GetObservable(ScrollViewer.ExtentProperty).Subscribe(OnExtentChanged);

            canvas.PointerPressed += OnCanvasPointerPressed;
            canvas.PointerMoved += OnCanvasPointerMoved;
            canvas.PointerReleased += OnCanvasPointerReleased;
            canvas.PointerCaptureLost += OnPointerCaptureLost;
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

                Action onCenterRequest = () =>
                {
                    global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        CenterContent();
                    });
                };
                vm.RequestCenterContent += onCenterRequest;
                _dataContextDisposables.Add(System.Reactive.Disposables.Disposable.Create(() => vm.RequestCenterContent -= onCenterRequest));
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

            if ((!_isInitialCentered || _hasPendingCenter) && DataContext is DrawableCanvasViewModel vm && vm.DisplayWidth > 0)
            {
                CenterContent();
            }
        }

        private void OnExtentChanged(Size extent)
        {
            if (extent.Width <= 0 || extent.Height <= 0) return;
            if (!_isInitialCentered || _hasPendingCenter)
            {
                CenterContent();
            }
        }

        public void CenterContent()
        {
            if (_isCentering) return;
            if (DataContext is not DrawableCanvasViewModel vm || scrollViewer == null) return;

            var displaySize = new Size(vm.DisplayWidth, vm.DisplayHeight);
            if (displaySize.Width <= 0 || displaySize.Height <= 0) return;

            var viewport = scrollViewer.Viewport;
            if (viewport.Width <= 0 || viewport.Height <= 0)
            {
                _hasPendingCenter = true;
                return;
            }

            _isCentering = true;
            try
            {
                var newMargin = ZoomHelper.CalculateOverscrollMargin(viewport);
                if (_currentMargin != newMargin)
                {
                    _currentMargin = newMargin;
                    canvasGrid.Margin = new Thickness(newMargin.X, newMargin.Y, newMargin.X, newMargin.Y);
                }

                double requiredWidth = displaySize.Width + (_currentMargin.X * 2);
                double requiredHeight = displaySize.Height + (_currentMargin.Y * 2);

                if (scrollViewer.Extent.Width < requiredWidth - 0.5 ||
                    scrollViewer.Extent.Height < requiredHeight - 0.5)
                {
                    this.UpdateLayout();
                    viewport = scrollViewer.Viewport;
                }

                scrollViewer.Offset = ZoomHelper.CalculateInitialOffset(_currentMargin, displaySize, viewport);

                if (scrollViewer.Extent.Width >= requiredWidth - 0.5 &&
                    scrollViewer.Extent.Height >= requiredHeight - 0.5)
                {
                    _isInitialCentered = true;
                    _hasPendingCenter = false;
                }
                else
                {
                    _hasPendingCenter = true;
                }
            }
            finally
            {
                _isCentering = false;
            }
        }

        private void OnScrollViewerPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Pointer.Type == PointerType.Touch) { if (e.Source != canvas && (TouchCanvasMode == TouchCanvasMode.Pan || _touchTracker.ActivePointerCount >= 1)) HandleTouchPointerPressed(e); return; }
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
            if (e.Pointer.Type == PointerType.Touch) { if (e.Source != canvas && _touchTracker.ActivePointerCount >= 1) HandleTouchPointerMoved(e); return; }
            if (_zoomAndPanController.IsPanning)
            {
                _zoomAndPanController.HandlePointerMoved(scrollViewer, scrollViewer, e);
                e.Handled = true;
            }
        }

        private void OnScrollViewerPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (e.Pointer.Type == PointerType.Touch) { if (e.Source != canvas && _touchTracker.ActivePointerCount >= 1) HandleTouchPointerReleased(e); return; }
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

        public static readonly StyledProperty<TouchCanvasMode> TouchCanvasModeProperty = AvaloniaProperty.Register<DrawableCanvas, TouchCanvasMode>(nameof(TouchCanvasMode), TouchCanvasMode.Draw);
        public TouchCanvasMode TouchCanvasMode { get => GetValue(TouchCanvasModeProperty); set => SetValue(TouchCanvasModeProperty, value); }
        public static readonly StyledProperty<ICommand?> CancelDrawingCommandProperty = AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(CancelDrawingCommand));
        public ICommand? CancelDrawingCommand { get => GetValue(CancelDrawingCommandProperty); set => SetValue(CancelDrawingCommandProperty, value); }
        public static readonly StyledProperty<ICommand?> UndoCommandProperty = AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(UndoCommand));
        public ICommand? UndoCommand { get => GetValue(UndoCommandProperty); set => SetValue(UndoCommandProperty, value); }
        public static readonly StyledProperty<ICommand?> RedoCommandProperty = AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(RedoCommand));
        public ICommand? RedoCommand { get => GetValue(RedoCommandProperty); set => SetValue(RedoCommandProperty, value); }

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
            if (e.Pointer.Type == PointerType.Touch) { HandleTouchPointerPressed(e); return; }

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
            if (e.Pointer.Type == PointerType.Touch) { HandleTouchPointerMoved(e); return; }

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
            if (e.Pointer.Type == PointerType.Touch) { HandleTouchPointerReleased(e); return; }

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
            if (e.Pointer.Type == PointerType.Touch)
            {
                _touchLongPressTimer?.Stop();
                if (_touchTracker.OnPointerCaptureLost(e.Pointer.Id) && _isTouchDrawing) { _isTouchDrawing = false; ExecuteCancelDrawing(); }
                if (_touchTracker.ActivePointerCount == 0) _isTouchSingleFingerPanning = false;
                return;
            }
            _zoomAndPanController.HandlePointerCaptureLost(e);
            IsLeftButtonPressing = false;
        }

        private void OnCanvasPointerExited(object? sender, EventArgs e)
        {
            if (_zoomAndPanController.IsPanning || _touchTracker.ActivePointerCount > 0) return;
            _zoomAndPanController.Reset();
            PointerLeaveCommand?.Execute(null);
        }

        private void HandleTouchPointerPressed(PointerPressedEventArgs e)
        {
            Point viewportPos = e.GetPosition(scrollViewer), canvasPos = e.GetPosition(canvas);
            var down = _touchTracker.OnPointerDown(e.Pointer.Id, viewportPos, Environment.TickCount64);
            e.Pointer.Capture(canvas);
            if (down.ShouldCancelSingleFingerAction)
            {
                _touchLongPressTimer?.Stop();
                _isTouchSingleFingerPanning = false;
                if (_isTouchDrawing) { _isTouchDrawing = false; ExecuteCancelDrawing(); }
            }
            else if (down.CanStartSingleFingerAction)
            {
                var pos = new Position((int)canvasPos.X, (int)canvasPos.Y);
                if (TouchCanvasMode == TouchCanvasMode.Pan) { _isTouchSingleFingerPanning = true; _touchPanLastViewportPos = viewportPos; }
                else if (TouchCanvasMode == TouchCanvasMode.Eyedropper) PointerRightButtonPressedCommand?.Execute(pos);
                else
                {
                    PointerLeftButtonPressedCommand?.Execute(pos);
                    _isTouchDrawing = true;
                    _touchLastCanvasPos = canvasPos;
                    _touchLongPressTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(TouchGestureTracker.LongPressDelayMs), DispatcherPriority.Input, OnTouchLongPressTick);
                    _touchLongPressTimer.Stop();
                    _touchLongPressTimer.Start();
                }
            }
            e.Handled = true;
        }

        private void OnTouchLongPressTick(object? sender, EventArgs e)
        {
            _touchLongPressTimer?.Stop();
            if (!_touchTracker.TryTriggerLongPress(Environment.TickCount64, out _)) return;
            if (_isTouchDrawing) { _isTouchDrawing = false; ExecuteCancelDrawing(); }
            PointerRightButtonPressedCommand?.Execute(new Position((int)_touchLastCanvasPos.X, (int)_touchLastCanvasPos.Y));
        }

        private void HandleTouchPointerMoved(PointerEventArgs e)
        {
            Point viewportPos = e.GetPosition(scrollViewer), canvasPos = e.GetPosition(canvas);
            var move = _touchTracker.OnPointerMoved(e.Pointer.Id, viewportPos, Environment.TickCount64);
            if (!_touchTracker.IsLongPressEligible) _touchLongPressTimer?.Stop();
            if (move.IsMultiTouch)
            {
                _touchLongPressTimer?.Stop();
                if (DataContext is DrawableCanvasViewModel vm)
                    TouchGestureTracker.ApplyMultiTouchPanAndZoom(move, scrollViewer, vm.Magnification, mag => vm.Magnification = mag, () => UpdateLayout(), _currentMargin);
            }
            else if (move.CanContinueSingleFingerAction)
            {
                var pos = new Position((int)canvasPos.X, (int)canvasPos.Y);
                if (TouchCanvasMode == TouchCanvasMode.Pan && _isTouchSingleFingerPanning && scrollViewer != null)
                {
                    scrollViewer.Offset = ZoomHelper.CalculatePanOffset(scrollViewer.Offset, viewportPos - _touchPanLastViewportPos);
                    _touchPanLastViewportPos = viewportPos;
                }
                else if (TouchCanvasMode == TouchCanvasMode.Eyedropper) PointerRightButtonPressedCommand?.Execute(pos);
                else { _touchLastCanvasPos = canvasPos; if (_isTouchDrawing) PointerMovedCommand?.Execute(pos); }
            }
            e.Handled = true;
        }

        private void HandleTouchPointerReleased(PointerReleasedEventArgs e)
        {
            _touchLongPressTimer?.Stop();
            Point viewportPos = e.GetPosition(scrollViewer), canvasPos = e.GetPosition(canvas);
            var up = _touchTracker.OnPointerUp(e.Pointer.Id, viewportPos, Environment.TickCount64);
            e.Pointer.Capture(null);
            if (up.ShouldEndSingleFingerAction && _isTouchDrawing)
            {
                _isTouchDrawing = false;
                PointerLeftButtonReleasedCommand?.Execute(new Position((int)canvasPos.X, (int)canvasPos.Y));
            }
            if (up.ActivePointerCount == 0) _isTouchDrawing = _isTouchSingleFingerPanning = false;
            if (up.TapAction == TouchTapAction.TwoFingerUndo) UndoCommand?.Execute(null);
            else if (up.TapAction == TouchTapAction.ThreeFingerRedo) RedoCommand?.Execute(null);
            e.Handled = true;
        }

        private void ExecuteCancelDrawing()
        {
            if (CancelDrawingCommand != null) CancelDrawingCommand.Execute(null);
            else (DataContext as DrawableCanvasViewModel)?.CancelDrawing();
        }
    }
}
