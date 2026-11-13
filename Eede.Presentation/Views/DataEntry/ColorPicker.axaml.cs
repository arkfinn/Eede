using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Eede.Domain.Palettes;
using System;
using HsvColor = Avalonia.Media.HsvColor;

namespace Eede.Presentation.Views.DataEntry
{
#nullable enable
    public enum ColorDepthMode
    {
        Depth256,
        Depth16
    }

    public partial class ColorPicker : UserControl
    {
        private enum ColorPickerMode
        {
            RGB,
            HSV
        }

        private ColorPickerMode ColorMode = ColorPickerMode.RGB;
        public ColorDepthMode DepthMode { get; private set; } = ColorDepthMode.Depth256;

        private bool _isUpdatingFromUi;
        private bool _isSyncingSliders;

        public ColorPicker()
        {
            InitializeComponent();

            pickerA.ValueChanged += PickerValueChanged;
            pickerR.ValueChanged += PickerValueChanged;
            pickerG.ValueChanged += PickerValueChanged;
            pickerB.ValueChanged += PickerValueChanged;

            UpdateSliderConfiguration();
            UpdatePicker(NowColor);
            UpdateGradationColor();

            _ = this.GetObservable(NowColorProperty).Subscribe(_ =>
            {
                if (_isUpdatingFromUi)
                {
                    return;
                }
                UpdatePicker(NowColor);
                UpdateGradationColor();
            });
        }

        public static readonly StyledProperty<ArgbColor> NowColorProperty =
            AvaloniaProperty.Register<ColorPicker, ArgbColor>(nameof(NowColor), new ArgbColor(255, 0, 0, 0), defaultBindingMode: BindingMode.TwoWay);
        public ArgbColor NowColor
        {
            get => GetValue(NowColorProperty);
            set => SetValue(NowColorProperty, value);
        }

        private int ToDisplayValue(byte value255)
        {
            return DepthMode == ColorDepthMode.Depth16
                ? (int)Math.Round(value255 / 17.0)
                : value255;
        }

        private byte ToByte255(int displayValue)
        {
            return DepthMode == ColorDepthMode.Depth16
                ? (byte)Math.Clamp(displayValue * 17, 0, 255)
                : (byte)Math.Clamp(displayValue, 0, 255);
        }

        private void UpdateSliderConfiguration()
        {
            bool is16 = DepthMode == ColorDepthMode.Depth16;
            int maxChannel = is16 ? 15 : 255;
            int largeChange = is16 ? 1 : 16;

            pickerA.MaxValue = maxChannel;
            pickerA.LargeChange = largeChange;

            pickerG.MaxValue = maxChannel;
            pickerG.LargeChange = largeChange;

            pickerB.MaxValue = maxChannel;
            pickerB.LargeChange = largeChange;

            if (ColorMode == ColorPickerMode.HSV)
            {
                pickerR.MaxValue = 360;
                pickerR.LargeChange = 15;
            }
            else
            {
                pickerR.MaxValue = maxChannel;
                pickerR.LargeChange = largeChange;
            }
        }

        private void UpdateGradationColor()
        {
            pickerA.GradationColor = new Color[] { Color.FromRgb(255, 255, 255), Color.FromRgb(0, 0, 0) };
            switch (ColorMode)
            {
                case ColorPickerMode.RGB:
                    byte r = ToByte255(pickerR.Value);
                    byte g = ToByte255(pickerG.Value);
                    byte b = ToByte255(pickerB.Value);
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
                    double v = pickerB.MaxValue == 0 ? 0 : (double)pickerB.Value / pickerB.MaxValue;
                    pickerG.GradationColor = new Color[] {
                        HsvColor.FromHsv(pickerR.Value, 1, v).ToRgb(),
                        HsvColor.FromHsv(pickerR.Value, 0, v).ToRgb()
                    };
                    double s = pickerG.MaxValue == 0 ? 0 : (double)pickerG.Value / pickerG.MaxValue;
                    pickerB.GradationColor = new Color[] {
                        HsvColor.FromHsv(pickerR.Value, s, 1).ToRgb(),
                        HsvColor.FromHsv(pickerR.Value, s, 0).ToRgb()
                    };
                    break;
            }
        }

        private void UpdateNowColor()
        {
            if (_isSyncingSliders)
            {
                return;
            }

            _isUpdatingFromUi = true;
            try
            {
                byte a = ToByte255(pickerA.Value);
                switch (ColorMode)
                {
                    case ColorPickerMode.RGB:
                        byte r = ToByte255(pickerR.Value);
                        byte g = ToByte255(pickerG.Value);
                        byte b = ToByte255(pickerB.Value);
                        NowColor = new ArgbColor(a, r, g, b);
                        break;

                    case ColorPickerMode.HSV:
                        double s = pickerG.MaxValue == 0 ? 0 : (double)pickerG.Value / pickerG.MaxValue;
                        double v = pickerB.MaxValue == 0 ? 0 : (double)pickerB.Value / pickerB.MaxValue;
                        Color color = HsvColor.FromHsv(pickerR.Value, s, v).ToRgb();
                        NowColor = new ArgbColor(a, color.R, color.G, color.B);
                        break;
                }
            }
            finally
            {
                _isUpdatingFromUi = false;
            }
        }

        private void UpdatePicker(ArgbColor newColor)
        {
            _isSyncingSliders = true;
            try
            {
                pickerA.Value = ToDisplayValue(newColor.Alpha);
                switch (ColorMode)
                {
                    case ColorPickerMode.RGB:
                        pickerR.Value = ToDisplayValue(newColor.Red);
                        pickerG.Value = ToDisplayValue(newColor.Green);
                        pickerB.Value = ToDisplayValue(newColor.Blue);
                        break;

                    case ColorPickerMode.HSV:
                        HsvColor hsv = Color.FromRgb(newColor.Red, newColor.Green, newColor.Blue).ToHsv();
                        pickerR.Value = (int)Math.Round(hsv.H);
                        pickerG.Value = (int)Math.Round(hsv.S * pickerG.MaxValue);
                        pickerB.Value = (int)Math.Round(hsv.V * pickerB.MaxValue);
                        break;
                }
            }
            finally
            {
                _isSyncingSliders = false;
            }
        }

        private void PickerValueChanged(object? sender, GradationSlider.ValueChangedEventArgs e)
        {
            if (_isSyncingSliders)
            {
                return;
            }
            UpdateNowColor();
            UpdateGradationColor();
        }

        private void ToggleColorMode(object? sender, RoutedEventArgs e)
        {
            if (ColorMode == ColorPickerMode.RGB)
            {
                ColorMode = ColorPickerMode.HSV;
                text1.Text = "H";
                text2.Text = "S";
                text3.Text = "V";
            }
            else
            {
                ColorMode = ColorPickerMode.RGB;
                text1.Text = "R";
                text2.Text = "G";
                text3.Text = "B";
            }

            _isSyncingSliders = true;
            try
            {
                UpdateSliderConfiguration();
                UpdatePicker(NowColor);
            }
            finally
            {
                _isSyncingSliders = false;
            }
            UpdateGradationColor();
        }

        private void ToggleDepthMode(object? sender, RoutedEventArgs e)
        {
            DepthMode = DepthMode == ColorDepthMode.Depth256
                ? ColorDepthMode.Depth16
                : ColorDepthMode.Depth256;
            depthModeText.Text = DepthMode == ColorDepthMode.Depth16 ? "16" : "256";

            _isSyncingSliders = true;
            try
            {
                UpdateSliderConfiguration();
                UpdatePicker(NowColor);
            }
            finally
            {
                _isSyncingSliders = false;
            }
            UpdateGradationColor();
        }

        private void StepDownTone(object? sender, RoutedEventArgs e)
        {
            ShiftTone(-1);
        }

        private void StepUpTone(object? sender, RoutedEventArgs e)
        {
            ShiftTone(1);
        }

        public void ShiftTone(int direction)
        {
            int step = (DepthMode == ColorDepthMode.Depth16 ? 1 : 16) * direction;
            _isSyncingSliders = true;
            try
            {
                if (ColorMode == ColorPickerMode.RGB)
                {
                    pickerR.Value = Math.Clamp(pickerR.Value + step, 0, pickerR.MaxValue);
                    pickerG.Value = Math.Clamp(pickerG.Value + step, 0, pickerG.MaxValue);
                    pickerB.Value = Math.Clamp(pickerB.Value + step, 0, pickerB.MaxValue);
                }
                else if (ColorMode == ColorPickerMode.HSV)
                {
                    pickerB.Value = Math.Clamp(pickerB.Value + step, 0, pickerB.MaxValue);
                }
            }
            finally
            {
                _isSyncingSliders = false;
            }
            UpdateNowColor();
            UpdateGradationColor();
        }
    }
}
