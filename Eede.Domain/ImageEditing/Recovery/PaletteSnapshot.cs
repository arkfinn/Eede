#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Eede.Domain.Palettes;

namespace Eede.Domain.ImageEditing.Recovery;

public sealed record PaletteSnapshot
{
    public ArgbColor SelectedColor { get; init; }
    public int ActiveTabIndex { get; init; }
    public IReadOnlyList<ArgbColor> PaletteColors { get; init; }

    public PaletteSnapshot(
        ArgbColor selectedColor,
        int activeTabIndex,
        IReadOnlyList<ArgbColor> paletteColors)
    {
        ArgumentNullException.ThrowIfNull(paletteColors);
        if (activeTabIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(activeTabIndex), "ActiveTabIndex must not be negative.");
        }

        SelectedColor = selectedColor;
        ActiveTabIndex = activeTabIndex;
        PaletteColors = paletteColors.ToArray();
    }
}
