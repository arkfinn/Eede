#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Eede.Domain.Palettes;

public class Palette : IEquatable<Palette>
{
    public const int MAX_LENGTH = 256;
    private readonly ImmutableList<ArgbColor> Colors;

    private Palette(ImmutableList<ArgbColor> colors)
    {
        if (colors.Count != MAX_LENGTH)
        {
            throw new InvalidOperationException(colors.Count.ToString());
        }
        Colors = colors;
    }

    public static Palette Create()
    {
        return FromColors(new ArgbColor[MAX_LENGTH]);
    }

    public static Palette FromColors(ArgbColor[] colors)
    {
        return new Palette([.. colors]);
    }

    public ArgbColor Fetch(int index)
    {
        return Colors[index];
    }

    public Palette Apply(int index, ArgbColor value)
    {
        return new Palette(Colors.SetItem(index, value));
    }

    public void ForEach(Action<ArgbColor, int> action)
    {
        for (int i = 0; i < Colors.Count; i++)
        {
            action.Invoke(Colors[i], i);
        }
    }

    /// <summary>
    /// 空きスロット／完全透明 (Alpha == 0) を除いたユニークな有効色の集合を取得する。
    /// </summary>
    public HashSet<ArgbColor> GetEffectiveColorSet()
    {
        var set = new HashSet<ArgbColor>();
        foreach (var c in Colors)
        {
            if (c.Alpha != 0)
            {
                set.Add(c);
            }
        }
        return set;
    }

    /// <summary>
    /// 並び順や空きスロットの位置を問わず、有効色の集合が完全に一致するかを判定する（空パレット同士は false）。
    /// </summary>
    public bool HasSameColorSet(Palette other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var selfSet = GetEffectiveColorSet();
        var otherSet = other.GetEffectiveColorSet();
        return selfSet.Count > 0 && selfSet.SetEquals(otherSet);
    }

    /// <summary>
    /// 並び順を問わず、本パレットが相手の全有効色を完全に内包しているかを判定する（相手が空パレットの場合は false）。
    /// </summary>
    public bool ContainsAllColorsOf(Palette other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var otherSet = other.GetEffectiveColorSet();
        if (otherSet.Count == 0) return false;
        var selfSet = GetEffectiveColorSet();
        return selfSet.IsSupersetOf(otherSet);
    }

    /// <summary>
    /// 並び順を問わず、本パレットが相手の全有効色を真に内包（自分の方が色数が多い上位パレット）しているかを判定する。
    /// </summary>
    public bool IsProperSupersetColorSetOf(Palette other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var otherSet = other.GetEffectiveColorSet();
        if (otherSet.Count == 0) return false;
        var selfSet = GetEffectiveColorSet();
        return selfSet.IsProperSupersetOf(otherSet);
    }

    public bool Equals(Palette? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Colors.SequenceEqual(other.Colors);
    }

    public override bool Equals(object? obj) => Equals(obj as Palette);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var c in Colors)
        {
            hash.Add(c);
        }
        return hash.ToHashCode();
    }
}
