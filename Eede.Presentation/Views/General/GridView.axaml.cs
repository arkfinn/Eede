using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using System;
using System.Globalization;

namespace Eede.Presentation.Views.General
{
    public partial class GridView : UserControl
    {
        public static readonly StyledProperty<GridSettings?> GridSettingsProperty =
            AvaloniaProperty.Register<GridView, GridSettings?>(nameof(GridSettings));

        public GridSettings? GridSettings
        {
            get => GetValue(GridSettingsProperty);
            set => SetValue(GridSettingsProperty, value);
        }

        public static readonly StyledProperty<Magnification?> MagnificationProperty =
            AvaloniaProperty.Register<GridView, Magnification?>(nameof(Magnification));

        public Magnification? Magnification
        {
            get => GetValue(MagnificationProperty);
            set => SetValue(MagnificationProperty, value);
        }

        public GridView()
        {
            InitializeComponent();
        }

        static GridView()
        {
            AffectsRender<GridView>(GridSettingsProperty, MagnificationProperty, BoundsProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var gridSettings = GridSettings;
            var mag = Magnification;
            if (gridSettings == null || mag == null) return;

            var cellSize = gridSettings.CellSize;
            var offset = gridSettings.Offset;
            var padding = gridSettings.Padding;

            var pen = new Pen(Brushes.White, 1);
            var dashPen = new Pen(Brushes.Black, 1, new DashStyle(new[] { 2.0, 2.0 }, 0));

            double width = Bounds.Width;
            double height = Bounds.Height;

            int mCellW = mag.Value.Magnify(cellSize.Width);
            int mCellH = mag.Value.Magnify(cellSize.Height);
            int mOffsetX = mag.Value.Magnify(offset.X);
            int mOffsetY = mag.Value.Magnify(offset.Y);
            int mPadding = mag.Value.Magnify(padding);

            int stepX = mCellW + mPadding;
            int stepY = mCellH + mPadding;

            if (stepX <= 0 || stepY <= 0) return;

            // Draw vertical lines
            for (double x = mOffsetX; x <= width; x += stepX)
            {
                context.DrawLine(pen, new Point(x, 0), new Point(x, height));
                context.DrawLine(dashPen, new Point(x, 0), new Point(x, height));
                
                if (mPadding > 0 && x + mCellW <= width)
                {
                    context.DrawLine(pen, new Point(x + mCellW, 0), new Point(x + mCellW, height));
                    context.DrawLine(dashPen, new Point(x + mCellW, 0), new Point(x + mCellW, height));
                }
            }

            // Draw horizontal lines
            for (double y = mOffsetY; y <= height; y += stepY)
            {
                context.DrawLine(pen, new Point(0, y), new Point(width, y));
                context.DrawLine(dashPen, new Point(0, y), new Point(width, y));
                
                if (mPadding > 0 && y + mCellH <= height)
                {
                    context.DrawLine(pen, new Point(0, y + mCellH), new Point(width, y + mCellH));
                    context.DrawLine(dashPen, new Point(0, y + mCellH), new Point(width, y + mCellH));
                }
            }

            // Draw cell indices
            var typeFace = new Typeface(FontFamily.Default);
            int columns = Math.Max(1, ((int)width - mOffsetX + mPadding) / stepX);
            int rows = Math.Max(1, ((int)height - mOffsetY + mPadding) / stepY);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int index = r * columns + c;
                    var text = new FormattedText(
                        index.ToString(),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeFace,
                        10,
                        Brushes.White);

                    double tx = mOffsetX + c * stepX + 2;
                    double ty = mOffsetY + r * stepY + 2;
                    
                    if (tx + text.Width <= width && ty + text.Height <= height)
                    {
                        // Background for text visibility
                        context.DrawRectangle(Brushes.Black, null, new Rect(tx, ty, text.Width, text.Height));
                        context.DrawText(text, new Point(tx, ty));
                    }
                }
            }
        }
    }
}
