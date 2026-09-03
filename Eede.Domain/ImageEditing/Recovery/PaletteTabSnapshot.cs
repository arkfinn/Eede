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
    public string CustomTitle { get; init; }
    public bool IsClosable { get; init; }
    public string? SourceIdentity { get; init; }

    public PaletteTabSnapshot(
        string? filePath,
        bool isDirty,
        IReadOnlyList<ArgbColor> colors,
        string? customTitle = null,
        bool? isClosable = null,
        string? sourceIdentity = null)
    {
        ArgumentNullException.ThrowIfNull(colors);
        FilePath = filePath;
        IsDirty = isDirty;
        Colors = colors.ToArray();
        CustomTitle = !string.IsNullOrEmpty(customTitle)
            ? customTitle
            : (filePath != null ? System.IO.Path.GetFileNameWithoutExtension(filePath) : "一時パレット");
        IsClosable = isClosable ?? (filePath != null);
        SourceIdentity = sourceIdentity ?? filePath;
    }

    public PaletteTabSnapshot() : this(null, false, Array.Empty<ArgbColor>())
    {
    }
}
