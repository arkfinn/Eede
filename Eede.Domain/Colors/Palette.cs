﻿using System;
using System.Collections.Immutable;
using System.Linq;

namespace Eede.Domain.Colors;

public class Palette
{
    private const int MAX_LENGTH = 256;
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
        ArgbColor[] colorArray = new ArgbColor[MAX_LENGTH];
        for (int i = 0; i < MAX_LENGTH; i++)
        {
            colorArray[i] = new ArgbColor(0, 0, 0, 0);
        }
        return new Palette(colorArray.ToImmutableList());
    }

    public ArgbColor Get(int index)
    {
        return Colors[index];
    }

    public Palette Set(int index, ArgbColor value)
    {
        return new Palette(Colors.SetItem(index, value));
    }

    public void ForEach(Action<ArgbColor, int> action)
    {
        foreach (var item in Colors.Select((value, index) => new { value, index }))
        {
            action.Invoke(item.value, item.index);
        }
    }
}
