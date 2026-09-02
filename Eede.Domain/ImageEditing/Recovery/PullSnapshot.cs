#nullable enable
using System;
using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.Recovery;

public sealed record PullSnapshot
{
    public string SourceDocumentId { get; init; }
    public PictureArea SourceArea { get; init; }
    public bool HasUnpushedChanges { get; init; }
    public string? CanvasImagePayloadRef { get; init; }

    public PullSnapshot(
        string sourceDocumentId,
        PictureArea sourceArea,
        bool hasUnpushedChanges,
        string? canvasImagePayloadRef)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDocumentId);

        SourceDocumentId = sourceDocumentId;
        SourceArea = sourceArea;
        HasUnpushedChanges = hasUnpushedChanges;
        CanvasImagePayloadRef = canvasImagePayloadRef;
    }
}
