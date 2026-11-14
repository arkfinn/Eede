using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Eede.Presentation.Views.DataEntry;
using System;

namespace Eede.Presentation.Views.DataEntry
{
#nullable enable

    public partial class GradationPicker : UserControl
    {
        internal enum ColorPickerMode
        {
            Empty,
            RGB,
            HSV
        }

        private bool _isShiftPressed;

        public GradationPicker()
        {
            InitializeComponent();
            slider.ValueChanged += SliderValueChanged;
            textbox.TextChanging += OnTextChanging;
            AddHandler(PointerPressedEvent, (_, e) => _isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift), RoutingStrategies.Tunnel);
            PointerWheelChanged += OnPointerWheelChanged;
        }

        public int Value
        {
            get => slider.Value;
            set => slider.Value = value;
        }

        public Color[]? GradationColor
        {
            get => slider.GradationColor;
            set => slider.GradationColor = value;
        }

        public event EventHandler<GradationSlider.ValueChangedEventArgs> ValueChanged
        {
            add => slider.ValueChanged += value;
            remove => slider.ValueChanged -= value;
        }

        public int MaxValue
        {
            get => slider.MaxValue;
            set
            {
                slider.MaxValue = value;
                textbox.Text = slider.Value.ToString();
            }
        }

        public int LargeChange
        {
            get => slider.LargeChange;
            set => slider.LargeChange = value;
        }

        private void IncrementValue(object? sender, RoutedEventArgs e)
        {
            slider.Value += _isShiftPressed ? slider.LargeChange : 1;
        }

        private void DecrementValue(object? sender, RoutedEventArgs e)
        {
            slider.Value -= _isShiftPressed ? slider.LargeChange : 1;
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            int step = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? slider.LargeChange : 1;
            if (e.Delta.Y > 0)
            {
                slider.Value += step;
            }
            else if (e.Delta.Y < 0)
            {
                slider.Value -= step;
            }
            e.Handled = true;
        }

        private void SliderValueChanged(object? sender, GradationSlider.ValueChangedEventArgs e)
        {
            textbox.Text = e.Value.ToString();
        }

        private void OnTextChanging(object? sender, TextChangingEventArgs e)
        {
            if (textbox.Text == Value.ToString())
            {
                e.Handled = true;
                return;
            }
            if (int.TryParse(textbox.Text, out int value))
            {
                Value = value;
            }

        }
    }
}
