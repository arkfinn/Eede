#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Eede.Domain.Palettes;

namespace Eede.Domain.ImageEditing.Recovery;

public sealed record PaletteTabSnapshot
{
    public string? FilePath { get; init; }
    public bool IsDirty { get; init; }
    public IReadOnlyList<ArgbColor> Colors { get; init; }

    public PaletteTabSnapshot(
        string? filePath,
        bool isDirty,
        IReadOnlyList<ArgbColor> colors)
    {
        ArgumentNullException.ThrowIfNull(colors);
        FilePath = filePath;
        IsDirty = isDirty;
        Colors = colors.ToArray();
    }

    public PaletteTabSnapshot() : this(null, false, Array.Empty<ArgbColor>())
    {
    }
}
