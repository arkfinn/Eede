using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Presentation.Views.DataEntry;

public class GridOverlay : Control
{
    public static readonly StyledProperty<bool> IsShowPixelGridProperty =
        AvaloniaProperty.Register<GridOverlay, bool>(nameof(IsShowPixelGrid));

    public bool IsShowPixelGrid
    {
        get => GetValue(IsShowPixelGridProperty);
        set => SetValue(IsShowPixelGridProperty, value);
    }

    public static readonly StyledProperty<bool> IsShowCursorGridProperty =
        AvaloniaProperty.Register<GridOverlay, bool>(nameof(IsShowCursorGrid));

    public bool IsShowCursorGrid
    {
        get => GetValue(IsShowCursorGridProperty);
        set => SetValue(IsShowCursorGridProperty, value);
    }

    public static readonly StyledProperty<Magnification?> MagnificationProperty =
        AvaloniaProperty.Register<GridOverlay, Magnification?>(nameof(Magnification));

    public Magnification? Magnification
    {
        get => GetValue(MagnificationProperty);
        set => SetValue(MagnificationProperty, value);
    }

    public static readonly StyledProperty<PictureSize> CursorSizeProperty =
        AvaloniaProperty.Register<GridOverlay, PictureSize>(nameof(CursorSize), new PictureSize(32, 32));

    public PictureSize CursorSize
    {
        get => GetValue(CursorSizeProperty);
        set => SetValue(CursorSizeProperty, value);
    }

    public static readonly StyledProperty<ArgbColor> BackgroundColorProperty =
        AvaloniaProperty.Register<GridOverlay, ArgbColor>(nameof(BackgroundColor));

    public ArgbColor BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    static GridOverlay()
    {
        AffectsRender<GridOverlay>(IsShowPixelGridProperty, IsShowCursorGridProperty, MagnificationProperty, CursorSizeProperty, BackgroundColorProperty, BoundsProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var mag = Magnification;
        if (mag == null) return;

        double width = Bounds.Width;
        double height = Bounds.Height;

        // 背景が透明（Alphaが小さい）の場合は、チェッカーボードで見えやすい中間色を使用する
        Color gridBaseColor;
        if (BackgroundColor.Alpha < 10)
        {
            gridBaseColor = Colors.Gray;
        }
        else
        {
            bool isDarkBackground = BackgroundColor.Luminance < 0.5;
            gridBaseColor = isDarkBackground ? Colors.White : Colors.Black;
        }

        // 1px Pixel Grid
        if (IsShowPixelGrid)
        {
            // 不透明度をさらに上げて主張を強める (0.5)
            var pixelPen = new Pen(new SolidColorBrush(gridBaseColor, 0.5), 1);
            float magValue = mag.Value.Value;
            for (int x = 1; x < width / magValue; x++)
            {
                double px = x * magValue;
                context.DrawLine(pixelPen, new Point(px, 0), new Point(px, height));
            }
            for (int y = 1; y < height / magValue; y++)
            {
                double py = y * magValue;
                context.DrawLine(pixelPen, new Point(0, py), new Point(width, py));
            }
        }

        // Cursor Grid (Guide)
        if (IsShowCursorGrid)
        {
            // 不透明度をさらに上げてよりはっきりと表示 (0.8)
            var cursorPen = new Pen(new SolidColorBrush(gridBaseColor, 0.8), 2);
            int stepX = mag.Value.Magnify(CursorSize.Width);
            int stepY = mag.Value.Magnify(CursorSize.Height);

            if (stepX > 0)
            {
                for (double x = stepX; x < width; x += stepX)
                {
                    context.DrawLine(cursorPen, new Point(x, 0), new Point(x, height));
                }
            }
            if (stepY > 0)
            {
                for (double y = stepY; y < height; y += stepY)
                {
                    context.DrawLine(cursorPen, new Point(0, y), new Point(width, y));
                }
            }
        }
    }
}
