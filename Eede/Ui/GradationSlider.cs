using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Eede.Ui
{
    //http://komozo.blogspot.com/2010/01/c_04.html
    class GradationSlider : Control
    {

        // イベントの宣言  
        [Description("コントロールの値が変更するとき発生します。")]
        [Category("アクション")]
        public event EventHandler ValueChanged;

        // プロパティ  
        private int _maximum = 255;
        public int Maximum
        {
            set
            {
                this._maximum = value;
            }
            get
            {
                return this._maximum;
            }
        }

        private int minimum;

        public int Minimum
        {
            get { return minimum; }
            set { minimum = value; }
        }


        private int _value;
        public int Value
        {
            set
            {
                int old_val = this._value;
                this._value = Math.Min(Math.Max(Minimum, value), Maximum);
                if (ValueChanged != null && this._value != old_val) ValueChanged(this, new EventArgs());
                this.Refresh();
            }
            get
            {
                return this._value;
            }
        }

        private Color[] _gradationColor;
        public Color[] GradationColor
        {
            set
            {
                this._gradationColor = value;
                drawGradation();
                this.Refresh();
            }
            get
            {

                return this._gradationColor;
            }
        }

        public GradationSlider()
            : base()
        {

            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.Resize += new System.EventHandler(this.CSlider_Resize);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CSlider_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CSlider_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CSlider_MouseUp);
            this.MouseLeave += new System.EventHandler(this.CSlider_MouseLeave);
            this.MouseEnter += new System.EventHandler(this.CSlider_MouseEnter);

            drawTick();
        }

        // 描画  
        //針の描画  
        private Bitmap tick;
        private void drawTick()
        {
            Point[] point = null;
            switch (GradientMode)
            {
                case LinearGradientMode.BackwardDiagonal:
                    break;
                case LinearGradientMode.ForwardDiagonal:
                    break;
                case LinearGradientMode.Horizontal:
                    tick = new Bitmap(10, 8);
                    point = new Point[]{   new Point(5,1), new Point(9,5),  
                                 new Point(9,6), new Point(1,6),  
                                 new Point(1,5), new Point(5,1) };
                    break;
                case LinearGradientMode.Vertical:
                    tick = new Bitmap(8, 10);
                    point = new Point[] {   new Point(1,5), new Point(5,9),  
                                 new Point(6,9), new Point(6,1),  
                                 new Point(5,1), new Point(1,5) };
                    break;
                default:
                    break;
            }

            using (Graphics g = Graphics.FromImage(tick))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;

                Brush b = new SolidBrush(Color.White);
                g.FillPolygon(b, point);
                Pen p = new Pen(Color.Black, 1);
                if (activeFlag == true)
                {
                    p = new Pen(Color.Brown, 1);
                }
                g.DrawLines(p, point);
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            drawGradation();
            switch (gradientMode)
            {
                case LinearGradientMode.BackwardDiagonal:
                    break;
                case LinearGradientMode.ForwardDiagonal:
                    break;
                case LinearGradientMode.Horizontal:
                    pe.Graphics.DrawImage(gradation, tick.Width / 2, 0, gradation.Width, gradation.Height);
                    pe.Graphics.DrawImage(tick, (Value * gradation.Width-2) / (Maximum - Minimum), gradation.Height);
                    break;
                case LinearGradientMode.Vertical:
                    pe.Graphics.DrawImage(gradation, 0, tick.Height / 2, gradation.Width, gradation.Height);
                    pe.Graphics.DrawImage(tick, gradation.Width, (Value * gradation.Height-2) / (Maximum - Minimum));
                    break;
                default:
                    break;
            }
        }

        private LinearGradientMode gradientMode = LinearGradientMode.Horizontal;

        public LinearGradientMode GradientMode
        {
            get { return gradientMode; }
            set
            {
                gradientMode = value;
                drawGradation();
                drawTick();
            }
        }


        //グラデーションの描画  
        private Bitmap gradation;
        private void drawGradation()
        {
            if (this.Width > tick.Width && this.Height > tick.Height)
            {
                Size gradationSize = new Size(this.Width - tick.Width, this.Height - tick.Height);

                gradation = new Bitmap(gradationSize.Width, gradationSize.Height);
                Graphics g = Graphics.FromImage(gradation);
                LinearGradientBrush lgb =
                    new LinearGradientBrush(new Rectangle(0, 0, gradationSize.Width, gradationSize.Height),
                        Color.Black, Color.White, gradientMode);

                //多色用  


                if (GradationColor != null && GradationColor.Length >= 2)
                {
                    // ColorBlendクラスを生成   
                    ColorBlend cb = new ColorBlend();
                    cb.Colors = GradationColor;
                    float[] Position = new float[GradationColor.Length];
                    for (int i = 0; i < GradationColor.Length; i++)
                    {
                        Position[i] = (1.0f / (GradationColor.Length - 1)) * (i);
                    }

                    cb.Positions = Position;
                    // ブラシのInterpolationColorsに設定   
                    lgb.InterpolationColors = cb;
                }


                g.FillRectangle(lgb, new Rectangle(0, 0, gradationSize.Width, gradationSize.Height));

                //枠線  
                Pen p = new Pen(Color.Black, 1);
                if (activeFlag == true)
                {
                    p = new Pen(Color.Brown, 1);
                }

                g.DrawRectangle(p, 0, 0, gradationSize.Width - 1, gradationSize.Height - 1);
                lgb.Dispose();
                g.Dispose();
            }

        }


        //リサイズ時、グラデーションの再描画  
        private void CSlider_Resize(object sender, EventArgs e)
        {
            drawGradation();
            this.Refresh();
        }


        //プロパティ  
        private bool mouseDown = false;
        private void CSlider_MouseDown(object sender, MouseEventArgs e)
        {
            switch (GradientMode)
            {
                case LinearGradientMode.BackwardDiagonal:
                    break;
                case LinearGradientMode.ForwardDiagonal:
                    break;
                case LinearGradientMode.Horizontal:
                    Value = (int)(((float)(e.X - (tick.Width / 2)) / (gradation.Width)) * (Maximum - Minimum));
                    break;
                case LinearGradientMode.Vertical:
                    Value = (int)(((float)(e.Y - (tick.Height / 2)) / (gradation.Height)) * (Maximum - Minimum));
                    break;
                default:
                    break;
            }
            mouseDown = true;
        }

        private void CSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown == true)
            {
                switch (GradientMode)
                {
                    case LinearGradientMode.BackwardDiagonal:
                        break;
                    case LinearGradientMode.ForwardDiagonal:
                        break;
                    case LinearGradientMode.Horizontal:
                        Value = (int)(((float)(e.X - (tick.Width / 2)) / (gradation.Width)) * (Maximum - Minimum));
                        break;
                    case LinearGradientMode.Vertical:
                        Value = (int)(((float)(e.Y - (tick.Height / 2)) / (gradation.Height)) * (Maximum - Minimum));
                        break;
                    default:
                        break;
                }
            }
        }

        private void CSlider_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }


        private bool activeFlag = false;
        private void CSlider_MouseEnter(object sender, EventArgs e)
        {
            activeFlag = true;

            drawGradation();
            drawTick();
            this.Refresh();
        }

        private void CSlider_MouseLeave(object sender, EventArgs e)
        {
            activeFlag = false;

            drawGradation();
            drawTick();
            this.Refresh();
        }
    }
}
