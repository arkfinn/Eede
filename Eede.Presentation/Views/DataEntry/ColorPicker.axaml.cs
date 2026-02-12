using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Eede.Domain.Palettes;
using Eede.Presentation.Views.DataEntry;
using System;
using HsvColor = Avalonia.Media.HsvColor;

namespace Eede.Presentation.Views.DataEntry
{
    public partial class ColorPicker : UserControl
    {
        private enum ColorPickerMode
        {
            Empty,
            RGB,
            HSV
        }

        public ColorPicker()
        {
            InitializeComponent();

            pickerA.ValueChanged += PickerValueChanged;
            pickerR.ValueChanged += PickerValueChanged;
            pickerG.ValueChanged += PickerValueChanged;
            pickerB.ValueChanged += PickerValueChanged;

            _ = this.GetObservable(NowColorProperty).Subscribe(_ =>
            {
                UpdateGradationColor();
                UpdatePicker(NowColor);
            });
        }

        public static readonly StyledProperty<ArgbColor> NowColorProperty =
            AvaloniaProperty.Register<ColorPicker, ArgbColor>(nameof(NowColor), new ArgbColor(255, 0, 0, 0), defaultBindingMode: BindingMode.TwoWay);
        public ArgbColor NowColor
        {
            get => GetValue(NowColorProperty);
            set => SetValue(NowColorProperty, value);
        }

        private ColorPickerMode ColorMode = ColorPickerMode.RGB;

        private void UpdateGradationColor()
        {
            pickerA.GradationColor = new Color[] { Color.FromRgb(255, 255, 255), Color.FromRgb(0, 0, 0) };
            switch (ColorMode)
            {
                case ColorPickerMode.RGB:
                    byte r = NowColor.Red;
                    byte g = NowColor.Green;
                    byte b = NowColor.Blue;
                    pickerR.GradationColor = new Color[] { Color.FromRgb(255, g, b), Color.FromRgb(0, g, b) };
                    pickerG.GradationColor = new Color[] { Color.FromRgb(r, 255, b), Color.FromRgb(r, 0, b) };
                    pickerB.GradationColor = new Color[] { Color.FromRgb(r, g, 255), Color.FromRgb(r, g, 0) };
                    break;

                case ColorPickerMode.HSV:
                    pickerR.GradationColor = new Color[] {
                        Color.FromRgb(255, 0, 255),
                        Color.FromRgb(0, 0, 255),
                        Color.FromRgb(0, 255, 255),
                        Color.FromRgb(0, 255, 0),
                        Color.FromRgb(255, 255, 0),
                        Color.FromRgb(255, 0, 0),
                    };
                    double v = (double)pickerB.Value / pickerB.MaxValue;
                    pickerG.GradationColor = new Color[] {

                        HsvColor.FromHsv(pickerR.Value, 1, v).ToRgb(),
                        HsvColor.FromHsv(pickerR.Value, 0, v).ToRgb()
                    };
                    double s = (double)pickerG.Value / pickerG.MaxValue;
                    pickerB.GradationColor = new Color[] {
                        HsvColor.FromHsv(pickerR.Value, s, 1).ToRgb(),
                        HsvColor.FromHsv(pickerR.Value, s, 0).ToRgb()
                    };
                    break;

                default:
                    break;
            }
        }

        private void UpdateNowColor()
        {
            switch (ColorMode)
            {
                case ColorPickerMode.RGB:
                    byte r = (byte)pickerR.Value;
                    byte g = (byte)pickerG.Value;
                    byte b = (byte)pickerB.Value;
                    byte a = (byte)pickerA.Value;
                    NowColor = new ArgbColor(a, r, g, b);
                    break;

                case ColorPickerMode.HSV:
                    Color color = HsvColor.FromHsv(pickerR.Value, (double)pickerG.Value / pickerG.MaxValue, (double)pickerB.Value / pickerB.MaxValue).ToRgb();
                    NowColor = new ArgbColor((byte)pickerA.Value, color.R, color.G, color.B);
                    break;

                case ColorPickerMode.Empty:
                    // Empty���͉����s��Ȃ��i���������p�j
                    break;
            }
        }

        private void UpdatePicker(ArgbColor newColor)
        {
            switch (ColorMode)
            {
                case ColorPickerMode.RGB:
                    ColorMode = ColorPickerMode.Empty;
                    pickerA.Value = newColor.Alpha;
                    pickerR.Value = newColor.Red;
                    pickerG.Value = newColor.Green;
                    pickerB.Value = newColor.Blue;
                    ColorMode = ColorPickerMode.RGB;
                    break;

                case ColorPickerMode.HSV:
                    ColorMode = ColorPickerMode.Empty;
                    HsvColor hsv = Color.FromRgb(newColor.Red, newColor.Green, newColor.Blue).ToHsv();
                    pickerA.Value = newColor.Alpha;
                    pickerR.Value = (int)hsv.H;
                    pickerG.Value = (int)(hsv.S * pickerG.MaxValue);
                    pickerB.Value = (int)(hsv.V * pickerB.MaxValue);
                    ColorMode = ColorPickerMode.HSV;
                    break;

                case ColorPickerMode.Empty:
                    // Empty���͉����s��Ȃ��i���������p�j
                    break;
            }
        }

        private void PickerValueChanged(object? sender, GradationSlider.ValueChangedEventArgs e)
        {
            UpdateNowColor();
        }

        private void ToggleColorMode(object? sender, RoutedEventArgs e)
        {
            switch (ColorMode)
            {
                case ColorPickerMode.RGB:
                    text1.Text = "H";
                    text2.Text = "S";
                    text3.Text = "V";

                    pickerR.MaxValue = 360;
                    ColorMode = ColorPickerMode.Empty;
                    HsvColor hsv = Color.FromRgb(NowColor.Red, NowColor.Green, NowColor.Blue).ToHsv();
                    pickerR.Value = (int)hsv.H;
                    pickerG.Value = (int)(hsv.S * pickerG.MaxValue);
                    pickerB.Value = (int)(hsv.V * pickerB.MaxValue);
                    ColorMode = ColorPickerMode.HSV;
                    break;

                case ColorPickerMode.HSV:
                    text1.Text = "R";
                    text2.Text = "G";
                    text3.Text = "B";
                    ColorMode = ColorPickerMode.Empty;
                    pickerR.MaxValue = 255;
                    pickerR.Value = NowColor.Red;
                    pickerG.Value = NowColor.Green;
                    pickerB.Value = NowColor.Blue;
                    ColorMode = ColorPickerMode.RGB;
                    break;

                default:
                    break;
            }
            UpdateGradationColor();
        }
    }
}
