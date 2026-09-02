#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Recovery;

namespace Eede.Application.Recovery;

public sealed record RestoredDocument
{
    public DocumentSnapshot Snapshot { get; }
    public Picture Picture { get; }

    public RestoredDocument(DocumentSnapshot snapshot, Picture picture)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Picture = picture ?? throw new ArgumentNullException(nameof(picture));
    }
}

public sealed record CorruptedDocumentInfo
{
    public DocumentSnapshot Snapshot { get; }
    public string ErrorMessage { get; }
    public Exception? Exception { get; }

    public CorruptedDocumentInfo(DocumentSnapshot snapshot, string errorMessage, Exception? exception = null)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        ErrorMessage = errorMessage;
        Exception = exception;
    }
}

public sealed record RestoredPullState
{
    public PullSnapshot Snapshot { get; }
    public Picture? CanvasPicture { get; }

    public RestoredPullState(PullSnapshot snapshot, Picture? canvasPicture)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        CanvasPicture = canvasPicture;
    }
}

public sealed record RestoredSessionData
{
    public SessionSnapshot Snapshot { get; }
    public IReadOnlyList<RestoredDocument> Documents { get; }
    public RestoredPullState? PullState { get; }
    public PaletteSnapshot PaletteState { get; }
    public IReadOnlyList<CorruptedDocumentInfo> CorruptedDocuments { get; }

    public bool HasCorruptedDocuments => CorruptedDocuments.Count > 0;

    public RestoredSessionData(
        SessionSnapshot snapshot,
        IReadOnlyList<RestoredDocument> documents,
        RestoredPullState? pullState,
        PaletteSnapshot paletteState,
        IReadOnlyList<CorruptedDocumentInfo>? corruptedDocuments = null)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(paletteState);

        if (documents.Any(d => d is null))
        {
            throw new ArgumentException("Documents cannot contain null items.", nameof(documents));
        }

        Documents = documents.ToArray();
        PullState = pullState;
        PaletteState = paletteState;
        CorruptedDocuments = (corruptedDocuments ?? Array.Empty<CorruptedDocumentInfo>()).ToArray();

        if (CorruptedDocuments.Any(c => c is null))
        {
            throw new ArgumentException("CorruptedDocuments cannot contain null items.", nameof(corruptedDocuments));
        }
    }
}
