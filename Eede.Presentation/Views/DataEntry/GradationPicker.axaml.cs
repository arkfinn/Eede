using Avalonia.Controls;
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

        public GradationPicker()
        {
            InitializeComponent();
            slider.ValueChanged += SliderValueChanged;
            textbox.TextChanging += OnTextChanging;
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
            set => slider.MaxValue = value;
        }

        private void IncrementValue(object? sender, RoutedEventArgs e)
        {
            slider.Value++;
        }

        private void DecrementValue(object? sender, RoutedEventArgs e)
        {
            slider.Value--;
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
