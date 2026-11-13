using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace Eede.Presentation.Views.DataEntry
{
#nullable enable

    public class GradationSlider : TemplatedControl
    {
        public class ValueChangedEventArgs : EventArgs
        {
            public int Value { get; }

            public ValueChangedEventArgs(int value)
            {
                Value = value;
            }
        }

        private readonly Polygon TickPolygon;
        private Size SliderSize = new(0, 0);

        public event EventHandler<ValueChangedEventArgs>? ValueChanged;

        private int _maxValue = 255;
        public int MaxValue
        {
            get => _maxValue;
            set
            {
                if (_maxValue == value) return;
                _maxValue = value;
                Value = _value;
                InvalidateVisual();
            }
        }

        private int _minValue = 0;
        public int MinValue
        {
            get => _minValue;
            set
            {
                if (_minValue == value) return;
                _minValue = value;
                Value = _value;
                InvalidateVisual();
            }
        }

        public int LargeChange { get; set; } = 16;

        private int _value = 0;
        public int Value
        {
            get => _value;
            set
            {
                int newValue = Math.Min(Math.Max(MinValue, value), MaxValue);
                if (newValue == _value)
                {
                    return;
                }
                _value = newValue;
                ValueChanged?.Invoke(this, new ValueChangedEventArgs(newValue));
                InvalidateVisual();
            }
        }

        private Color[]? _gradationColor = null;
        public Color[]? GradationColor
        {
            set
            {
                _gradationColor = value;
                InvalidateVisual();
            }
            get => _gradationColor;
        }

        public GradationSlider()
        {
            LayoutUpdated += new EventHandler(OnLayoutUpdated);
            PointerPressed += new EventHandler<PointerPressedEventArgs>(OnPointerPressed);
            PointerMoved += new EventHandler<PointerEventArgs>(OnPointerMoved);
            PointerReleased += new EventHandler<PointerReleasedEventArgs>(OnPointerReleased);
            PointerCaptureLost += new EventHandler<PointerCaptureLostEventArgs>(OnPointerCaptureLost);
            PointerWheelChanged += new EventHandler<PointerWheelEventArgs>(OnPointerWheelChanged);
            PointerExited += new EventHandler<PointerEventArgs>(OnPointerExited);
            PointerEntered += new EventHandler<PointerEventArgs>(OnPointerEntered);

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            TickPolygon = CreateTickPolygon();
            TickPolygon.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
            LogicalChildren.Add(TickPolygon);
            VisualChildren.Add(TickPolygon);

            MaxHeight = MaxValue + TickPolygon.Height;
        }

        private Polygon CreateTickPolygon()
        {
            Polygon polygon = new();
            polygon.Points.Add(new Point(1, 5));
            polygon.Points.Add(new Point(5, 9));
            polygon.Points.Add(new Point(6, 9));
            polygon.Points.Add(new Point(6, 1));
            polygon.Points.Add(new Point(5, 1));
            polygon.Points.Add(new Point(1, 5));
            polygon.Width = 8;
            polygon.Height = 10;
            polygon.Fill = new SolidColorBrush(Colors.White);
            polygon.Stroke = new SolidColorBrush(Colors.Black);
            polygon.StrokeThickness = 1;
            polygon.Stretch = Stretch.None;
            polygon.UseLayoutRounding = false;
            RenderOptions.SetEdgeMode(polygon, EdgeMode.Aliased);
            return polygon;
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (IsDrawableSlider())
            {
                UpdateSliderSize();
                DrawGradation(context);
                DrawBorder(context);
                UpdateTick();
            }
        }

        private void UpdateSliderSize()
        {
            SliderSize = new Size(Width - TickPolygon.Width, Height - TickPolygon.Height);
        }

        private bool IsDrawableSlider()
        {
            return Width > TickPolygon.Width && Height > TickPolygon.Height;
        }

        private void DrawGradation(DrawingContext context)
        {
            IBrush brush = CreateBrush();
            context.FillRectangle(brush, new Rect(TickPolygon.Width / 2, TickPolygon.Height / 2, SliderSize.Width, SliderSize.Height));
        }

        private IBrush CreateBrush()
        {
            GradientStops gradientStops = [];

            if (GradationColor == null || GradationColor.Length < 2)
            {
                gradientStops.Add(new GradientStop { Color = Colors.Black, Offset = 0 });
                gradientStops.Add(new GradientStop { Color = Colors.White, Offset = 1 });
            }
            else
            {
                float offset = 1.0f / (GradationColor.Length - 1);
                for (int i = 0; i < GradationColor.Length; i++)
                {
                    gradientStops.Add(new GradientStop
                    {
                        Color = GradationColor[i],
                        Offset = offset * i
                    });
                }
            }
            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops = gradientStops
            }.ToImmutable();
        }

        private void DrawBorder(DrawingContext context)
        {
            Pen p = IsActive() ? new(Brushes.Brown, 1) : new(Brushes.Black, 1);

            context.DrawRectangle(p, new Rect((TickPolygon.Width / 2) + 0.5, TickPolygon.Height / 2, SliderSize.Width, SliderSize.Height));
        }

        private bool IsActive()
        {
            return activeFlag;
        }

        private void UpdateTick()
        {
            Dispatcher.UIThread.Post(() =>
            {
                double yPosition = MaxValue == MinValue
                    ? 0
                    : (MaxValue - (Value - MinValue)) / (double)(MaxValue - MinValue) * SliderSize.Height;
                TickPolygon.Margin = new Thickness(TickPolygon.Width, yPosition, 0, 0);
            });
        }

        private double GetTickCenterY()
        {
            double sliderHeight = Math.Max(1.0, Height - TickPolygon.Height);
            double yRatio = MaxValue == MinValue
                ? 0.0
                : (double)(MaxValue - (Value - MinValue)) / (MaxValue - MinValue);
            return (yRatio * sliderHeight) + (TickPolygon.Height / 2.0);
        }

        ////リサイズ時、グラデーションの再描画
        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            // これいらないかもしれない
            InvalidateVisual();
        }

        private bool _isThumbDragging = false;
        private bool _isTrackClickPending = false;
        private Point _pressPoint;
        private double _pressTickCenterY;

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            Point pos = e.GetPosition(this);
            double tickCenterY = GetTickCenterY();
            e.Pointer.Capture(this);

            if (Math.Abs(pos.Y - tickCenterY) <= 3.5)
            {
                _isThumbDragging = true;
                _isTrackClickPending = false;
                UpdateValueByPositionY(pos.Y);
            }
            else
            {
                _isTrackClickPending = true;
                _isThumbDragging = false;
                _pressPoint = pos;
                _pressTickCenterY = tickCenterY;
            }
            e.Handled = true;
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            Point pos = e.GetPosition(this);
            if (_isTrackClickPending)
            {
                if (Math.Abs(pos.Y - _pressPoint.Y) > 4.0)
                {
                    _isTrackClickPending = false;
                    _isThumbDragging = true;
                    UpdateValueByPositionY(pos.Y);
                }
                return;
            }

            if (_isThumbDragging)
            {
                UpdateValueByPositionY(pos.Y);
            }
        }

        private void UpdateValueByPositionY(double value)
        {
            double effectiveHeight = Math.Max(1.0, Height - TickPolygon.Height);
            Value = (int)Math.Round(MaxValue - ((value - (TickPolygon.Height / 2.0)) / effectiveHeight * (MaxValue - MinValue)));
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_isTrackClickPending)
            {
                Value += _pressPoint.Y < _pressTickCenterY ? LargeChange : -LargeChange;
            }
            _isTrackClickPending = false;
            _isThumbDragging = false;
            if (e.Pointer.Captured == this)
            {
                e.Pointer.Capture(null);
            }
            e.Handled = true;
        }

        private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            _isTrackClickPending = false;
            _isThumbDragging = false;
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            int step = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? LargeChange : 1;
            if (e.Delta.Y > 0)
            {
                Value += step;
            }
            else if (e.Delta.Y < 0)
            {
                Value -= step;
            }
            e.Handled = true;
        }

        private bool activeFlag = false;
        private void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            activeFlag = true;
            InvalidateVisual();
        }

        private void OnPointerExited(object? sender, PointerEventArgs e)
        {
            activeFlag = false;
            InvalidateVisual();
        }
    }

}
