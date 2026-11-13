using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Eede.Domain.ImageEditing.DrawingTools;
using System;

namespace Eede.Presentation.Views.DataEntry;

#nullable enable

public class TilePatternSwatch : Control
{
    private static readonly IBrush BackgroundTileBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
    private static readonly IBrush ForegroundTileBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));

    public static readonly StyledProperty<TilePattern?> PatternProperty =
        AvaloniaProperty.Register<TilePatternSwatch, TilePattern?>(nameof(Pattern));

    public static readonly StyledProperty<bool> ShiftXProperty =
        AvaloniaProperty.Register<TilePatternSwatch, bool>(nameof(ShiftX));

    static TilePatternSwatch()
    {
        AffectsRender<TilePatternSwatch>(PatternProperty, ShiftXProperty);
    }

    public TilePattern? Pattern
    {
        get => GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    public bool ShiftX
    {
        get => GetValue(ShiftXProperty);
        set => SetValue(ShiftXProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var bounds = new Rect(Bounds.Size);
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        context.FillRectangle(BackgroundTileBrush, bounds);

        TilePattern pattern = (Pattern ?? TilePattern.Solid).WithOffsetX(ShiftX ? 1 : 0);
        const int cellSize = 2;
        int cols = (int)(bounds.Width / cellSize);
        int rows = (int)(bounds.Height / cellSize);

        for (int py = 0; py < rows; py++)
        {
            for (int px = 0; px < cols; px++)
            {
                if (pattern.IsDrawable(px, py))
                {
                    context.FillRectangle(ForegroundTileBrush, new Rect(px * cellSize, py * cellSize, cellSize, cellSize));
                }
            }
        }
    }
}

/// <summary>
/// 3x3 タイルパターングリッド右下（9マス目）の横1ドットずらしトグル用オリジナルアイコン（基準線＋実線矢印）。
/// </summary>
public class TilePatternShiftIcon : Control
{
    private static readonly IBrush NormalBgBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
    private static readonly IBrush FallbackActiveBgBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x00, 0xC8, 0x53));
    private static readonly IBrush ReferenceBarBrush = new SolidColorBrush(Color.FromRgb(136, 136, 136));
    private static readonly IBrush NormalArrowBrush = new SolidColorBrush(Color.FromRgb(34, 34, 34));
    private static readonly IBrush FallbackActiveArrowBrush = new SolidColorBrush(Color.FromRgb(0, 126, 52));

    // 'R' = 左端の基準位置縦線, '#' = シンプルな実線右向き矢印
    private static readonly string[] IconBitmap =
    [
        ".R..........",
        ".R....#.....",
        ".R....##....",
        ".R.#######..",
        ".R.########.",
        ".R.#######..",
        ".R....##....",
        ".R....#.....",
        ".R.........."
    ];

    public static readonly StyledProperty<bool> ShiftXProperty =
        AvaloniaProperty.Register<TilePatternShiftIcon, bool>(nameof(ShiftX));

    public static readonly StyledProperty<IBrush?> ActiveBackgroundProperty =
        AvaloniaProperty.Register<TilePatternShiftIcon, IBrush?>(nameof(ActiveBackground));

    public static readonly StyledProperty<IBrush?> ActiveArrowBrushProperty =
        AvaloniaProperty.Register<TilePatternShiftIcon, IBrush?>(nameof(ActiveArrowBrush));

    static TilePatternShiftIcon()
    {
        AffectsRender<TilePatternShiftIcon>(ShiftXProperty, ActiveBackgroundProperty, ActiveArrowBrushProperty);
    }

    public bool ShiftX
    {
        get => GetValue(ShiftXProperty);
        set => SetValue(ShiftXProperty, value);
    }

    public IBrush? ActiveBackground
    {
        get => GetValue(ActiveBackgroundProperty);
        set => SetValue(ActiveBackgroundProperty, value);
    }

    public IBrush? ActiveArrowBrush
    {
        get => GetValue(ActiveArrowBrushProperty);
        set => SetValue(ActiveArrowBrushProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var bounds = new Rect(Bounds.Size);
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        context.FillRectangle(NormalBgBrush, bounds);
        if (ShiftX)
        {
            context.FillRectangle(ActiveBackground ?? FallbackActiveBgBrush, bounds);
        }

        const int cellSize = 2;
        IBrush arrowBrush = ShiftX ? (ActiveArrowBrush ?? FallbackActiveArrowBrush) : NormalArrowBrush;
        int offsetY = Math.Max(0, ((int)bounds.Height - IconBitmap.Length * cellSize) / 2);
        int arrowShiftPx = ShiftX ? cellSize : 0;

        for (int r = 0; r < IconBitmap.Length; r++)
        {
            string row = IconBitmap[r];
            for (int c = 0; c < row.Length; c++)
            {
                char ch = row[c];
                if (ch == 'R')
                {
                    context.FillRectangle(ReferenceBarBrush, new Rect(c * cellSize, offsetY + r * cellSize, cellSize, cellSize));
                }
                else if (ch == '#')
                {
                    int drawX = c * cellSize + arrowShiftPx;
                    if (drawX + cellSize <= bounds.Width)
                    {
                        context.FillRectangle(arrowBrush, new Rect(drawX, offsetY + r * cellSize, cellSize, cellSize));
                    }
                }
            }
        }
    }
}
