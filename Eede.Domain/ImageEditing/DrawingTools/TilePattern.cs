#nullable enable
using System;

namespace Eede.Domain.ImageEditing.DrawingTools;

public enum TilePatternType
{
    Solid,
    Dense87,
    Dense75,
    Dense62,
    Checker50,
    Sparse37,
    Sparse25,
    Sparse12
}

/// <summary>
/// 描画時の座標ごとの書き込み可否（抜け＝下地残存）を定義する不変ドメイン値オブジェクト。
/// </summary>
public sealed record TilePattern
{
    public TilePatternType Type { get; }
    public string DisplayName { get; }
    public int Width { get; }
    public int Height { get; }
    public int OffsetX { get; }
    public bool IsSolid => Type == TilePatternType.Solid;

    private TilePattern(TilePatternType type, string displayName, int width, int height, int offsetX = 0)
    {
        Type = type;
        DisplayName = displayName;
        Width = width;
        Height = height;
        OffsetX = offsetX;
    }

    public static TilePattern Solid { get; } = new(TilePatternType.Solid, "100% ベタ塗り", 1, 1);
    public static TilePattern Dense87 { get; } = new(TilePatternType.Dense87, "87.5%", 4, 4);
    public static TilePattern Dense75 { get; } = new(TilePatternType.Dense75, "75%", 2, 2);
    public static TilePattern Dense62 { get; } = new(TilePatternType.Dense62, "62.5%", 4, 4);
    public static TilePattern Checker50 { get; } = new(TilePatternType.Checker50, "50% 市松", 2, 2);
    public static TilePattern Sparse37 { get; } = new(TilePatternType.Sparse37, "37.5%", 4, 4);
    public static TilePattern Sparse25 { get; } = new(TilePatternType.Sparse25, "25%", 2, 2);
    public static TilePattern Sparse12 { get; } = new(TilePatternType.Sparse12, "12.5%", 4, 4);

    public TilePattern WithOffsetX(int offsetX)
    {
        if (offsetX == OffsetX)
        {
            return this;
        }
        return offsetX == 0
            ? FromType(Type)
            : new(Type, DisplayName, Width, Height, offsetX);
    }

    public static TilePattern FromType(TilePatternType type) => type switch
    {
        TilePatternType.Solid => Solid,
        TilePatternType.Dense87 => Dense87,
        TilePatternType.Dense75 => Dense75,
        TilePatternType.Dense62 => Dense62,
        TilePatternType.Checker50 => Checker50,
        TilePatternType.Sparse37 => Sparse37,
        TilePatternType.Sparse25 => Sparse25,
        TilePatternType.Sparse12 => Sparse12,
        _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown TilePatternType: {type}")
    };

    /// <summary>
    /// キャンバス絶対座標 (x, y) が本パターンで描画対象になるか判定する（負座標・横オフセット対応）。
    /// </summary>
    public bool IsDrawable(int x, int y)
    {
        int shiftedX = x - OffsetX;
        return Type switch
        {
            TilePatternType.Solid => true,
            TilePatternType.Dense87 => !((y & 1) == 0 && (shiftedX & 3) == (y & 2)),
            TilePatternType.Dense75 => (y & 1) == 1 || (shiftedX & 1) == 1,
            TilePatternType.Dense62 => (y & 1) == 0 ? (shiftedX & 1) == 1 : ((shiftedX - y) & 3) != 2,
            TilePatternType.Checker50 => ((shiftedX + y) & 1) == 1,
            TilePatternType.Sparse37 => (y & 1) == 1 ? (shiftedX & 1) == 0 : ((shiftedX + y + 1) & 3) == 0,
            TilePatternType.Sparse25 => (y & 1) == 1 && (shiftedX & 1) == 0,
            TilePatternType.Sparse12 => (y & 1) == 1 && (shiftedX & 3) == (y & 2),
            _ => true
        };
    }
}
