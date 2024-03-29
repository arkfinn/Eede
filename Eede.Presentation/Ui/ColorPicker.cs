﻿using Eede.Domain.Colors;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Eede.Ui
{
    internal enum ColorPickerMode
    {
        Empty,
        RGB,
        HSV
    }

    //struct Hsv
    //{
    //    public int H;
    //    public int S;
    //    public int V;
    //}

    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
        {
            InitializeComponent();
            UpdateGradationColor();
        }

        public event EventHandler ColorChanged;

        private void fireColorChanged()
        {
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private ColorPickerMode mode = ColorPickerMode.RGB;

        private void ColorPicker_Load(object sender, EventArgs e)
        {
        }

        private int r = 0;
        private int g = 0;
        private int b = 0;

        private void UpdateColor()
        {
            switch (mode)
            {
                case ColorPickerMode.RGB:
                    r = pickerR.Value;
                    g = pickerG.Value;
                    b = pickerB.Value;
                    break;

                case ColorPickerMode.HSV:
                    Color tmp = ConvertHsvColor(HsvColor.FromHsv(pickerR.Value, pickerG.Value, pickerB.Value));
                    r = tmp.R;
                    g = tmp.G;
                    b = tmp.B;
                    break;
                default:
                    break;
            }
        }

        private void UpdateGradationColor()
        {
            UpdateColor();
            pickerA.GradationColor = new Color[] { Color.FromArgb(255, 255, 255), Color.FromArgb(0, 0, 0) };
            switch (mode)
            {
                case ColorPickerMode.RGB:
                    pickerR.GradationColor = new Color[] { Color.FromArgb(255, g, b), Color.FromArgb(0, g, b) };
                    pickerG.GradationColor = new Color[] { Color.FromArgb(r, 255, b), Color.FromArgb(r, 0, b) };
                    pickerB.GradationColor = new Color[] { Color.FromArgb(r, g, 255), Color.FromArgb(r, g, 0) };
                    break;

                case ColorPickerMode.HSV:
                    pickerR.GradationColor = new Color[] {
                        Color.FromArgb(255, 0, 255),
                        Color.FromArgb(0, 0, 255),
                        Color.FromArgb(0, 255, 255),
                        Color.FromArgb(0, 255, 0),
                        Color.FromArgb(255, 255, 0),
                        Color.FromArgb(255, 0, 0),
                    };
                    pickerG.GradationColor = new Color[] {
                        ConvertHsvColor(HsvColor.FromHsv(pickerR.Value, 255, pickerB.Value)),
                        ConvertHsvColor(HsvColor.FromHsv(pickerR.Value, 0, pickerB.Value))
                    };
                    pickerB.GradationColor = new Color[] {
                        ConvertHsvColor(HsvColor.FromHsv(pickerR.Value, pickerG.Value, 255)),
                        ConvertHsvColor(HsvColor.FromHsv(pickerR.Value, pickerG.Value, 0))
                    };
                    break;

                default:
                    break;
            }
        }

        private Color ConvertHsvColor(HsvColor hsvColor)
        {
            ArgbColor color = hsvColor.ToArgbColor();
            return Color.FromArgb(color.Red, color.Green, color.Blue);
        }

        private void picker_ValueChanged(object sender, EventArgs e)
        {
            UpdateGradationColor();
            fireColorChanged();
        }

        private ArgbColor _nowColor = new(255, 0, 0, 0);
        public ArgbColor NowColor
        {
            get =>  new ArgbColor((byte)pickerA.Value, (byte)r, (byte)g, (byte)b);
            set
            {
                SetColor(value);
            }
        }

        private void SetColor(ArgbColor c)
        {
            pickerA.Value = c.Alpha;
            pickerR.Value = c.Red;
            pickerG.Value = c.Green;
            pickerB.Value = c.Blue;
        }

        //private Color GetArgb()
        //{
        //    return Color.FromArgb(pickerA.Value, r, g, b);
        //}

        //public Color GetRgb()
        //{
        //    return Color.FromArgb(255, r, g, b);
        //}

        //public Color GetA()
        //{
        //    return Color.FromArgb(pickerA.Value, pickerA.Value, pickerA.Value, pickerA.Value);
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            switch (mode)
            {
                case ColorPickerMode.RGB:
                    button1.Text = "A   H   S   V";
                    mode = ColorPickerMode.Empty;
                    HsvColor hsv = HsvColor.FromRgb(r, g, b);
                    pickerR.Maximum = 360;
                    pickerR.Value = hsv.Hue;
                    pickerG.Value = hsv.Saturation;
                    pickerB.Value = hsv.Value;
                    mode = ColorPickerMode.HSV;
                    break;

                case ColorPickerMode.HSV:
                    button1.Text = "A   R   G   B";
                    mode = ColorPickerMode.Empty;
                    pickerR.Maximum = 255;
                    pickerR.Value = r;
                    pickerG.Value = g;
                    pickerB.Value = b;
                    mode = ColorPickerMode.RGB;
                    break;

                default:
                    break;
            }
            UpdateGradationColor();
        }
        //private Hsv GetHsvFromRgb(int R, int G, int B)
        //{
        //    Hsv hsv = new Hsv();

        //    double max = Math.Max(Math.Max(R, G), B);
        //    double min = Math.Min(Math.Min(R, G), B);

        //    //Vを求める
        //    hsv.V = (int)max;

        //    //RGBすべてが同じ場合　HとSは0
        //    if (max == min)
        //    {
        //        hsv.H = 0;
        //        hsv.S = 0;
        //    }
        //    else
        //    {
        //        //Sを求める
        //        hsv.S = (int)(((max - min) * 255) / max);

        //        //Hを求める
        //        hsv.H = 0;

        //        if (max == R)
        //        {
        //            hsv.H = (int)(60 * (G - B) / (max - min));
        //            if (hsv.H < 0) hsv.H += 360;
        //        }
        //        else if (max == G)
        //        {
        //            hsv.H = (int)(60 * (B - R) / (max - min))+120;
        //            if (hsv.H < 0) hsv.H += 360;
        //        }
        //        else if (max == B)
        //        {
        //            hsv.H = (int)(60 * (R - G) / (max - min))+240;
        //            if (hsv.H < 0) hsv.H += 360;
        //        }
        //    }

        //    return hsv;
        //}

        //private Color GetColorFromHSV(int H, int S, int V)
        //{
        //    if (H == 360) H = 0;
        //    int Hi = (int)Math.Floor((double)H / 60) % 6;

        //    float f = ((float)H / 60) - Hi;
        //    float p = ((float)V / 255) * (1 - ((float)S / 255));
        //    float q = ((float)V / 255) * (1 - f * ((float)S / 255));
        //    float t = ((float)V / 255) * (1 - (1 - f) * ((float)S / 255));

        //    p *= 255;
        //    q *= 255;
        //    t *= 255;

        //    Color rgb = Color.Empty;

        //    switch (Hi)
        //    {
        //        case 0:
        //            rgb = Color.FromArgb(V, (int)t, (int)p);
        //            break;
        //        case 1:
        //            rgb = Color.FromArgb((int)q, V, (int)p);
        //            break;
        //        case 2:
        //            rgb = Color.FromArgb((int)p, V, (int)t);
        //            break;
        //        case 3:
        //            rgb = Color.FromArgb((int)p, (int)q, V);
        //            break;
        //        case 4:
        //            rgb = Color.FromArgb((int)t, (int)p, V);
        //            break;
        //        case 5:
        //            rgb = Color.FromArgb(V, (int)p, (int)q);
        //            break;
        //    }

        //    return rgb;
        //}

    }
}