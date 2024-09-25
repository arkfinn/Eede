using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace Eede.Presentation.Views.DataEntry
{
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

        public event EventHandler<ValueChangedEventArgs> ValueChanged;
        public int MaxValue { set; get; } = 255;
        public int MinValue { get; set; } = 0;

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

        private Color[] _gradationColor = null;
        public Color[] GradationColor
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
                double yPosition = (MaxValue - (Value - MinValue)) / (double)(MaxValue - MinValue) * SliderSize.Height;
                TickPolygon.Margin = new Thickness(TickPolygon.Width, yPosition, 0, 0);
            });

        }

        ////リサイズ時、グラデーションの再描画
        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            // これいらないかもしれない
            InvalidateVisual();
        }

        private bool mouseDown = false;
        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            UpdateValueByPositionY(e.GetPosition(this).Y);
            mouseDown = true;
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (mouseDown != true)
            {
                return;
            }
            UpdateValueByPositionY(e.GetPosition(this).Y);
        }
        private void UpdateValueByPositionY(double value)
        {
            Console.WriteLine($"Height: {Height}, TickPolygon.Height: {TickPolygon.Height}, Mouse Y: {value}");
            Value = (int)(MaxValue - ((value - (TickPolygon.Height / 2)) / (Height - TickPolygon.Height) * (MaxValue - MinValue)));
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            mouseDown = false;
        }

        private bool activeFlag = false;
        private void OnPointerEntered(object sender, PointerEventArgs e)
        {
            activeFlag = true;
            InvalidateVisual();
        }

        private void OnPointerExited(object sender, PointerEventArgs e)
        {
            activeFlag = false;
            InvalidateVisual();
        }
    }

}
