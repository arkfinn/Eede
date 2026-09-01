#nullable enable
using System;
using System.Collections.Generic;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Recovery;

namespace Eede.Application.Recovery;

public sealed record SessionCapture
{
    public SessionSnapshot Snapshot { get; }
    public IReadOnlyDictionary<string, Picture> Pictures { get; }

    public SessionCapture(
        SessionSnapshot snapshot,
        IReadOnlyDictionary<string, Picture> pictures)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Pictures = pictures ?? throw new ArgumentNullException(nameof(pictures));

        foreach (var (key, value) in pictures)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            if (value is null)
            {
                throw new ArgumentException($"Picture for key '{key}' cannot be null.", nameof(pictures));
            }
        }
    }
}
