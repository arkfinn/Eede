#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eede.Domain.ImageEditing.Recovery;

public sealed record SessionSnapshot
{
    public Guid SessionId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string ActiveDocumentId { get; init; }
    public IReadOnlyList<DocumentSnapshot> Documents { get; init; }
    public PullSnapshot? PullState { get; init; }
    public PaletteSnapshot PaletteState { get; init; }

    public SessionSnapshot(
        Guid sessionId,
        DateTimeOffset createdAt,
        string activeDocumentId,
        IReadOnlyList<DocumentSnapshot> documents,
        PullSnapshot? pullState,
        PaletteSnapshot paletteState)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("SessionId cannot be empty.", nameof(sessionId));
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(activeDocumentId);
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(paletteState);

        if (documents.Any(d => d is null))
        {
            throw new ArgumentException("Documents collection cannot contain null elements.", nameof(documents));
        }

        SessionId = sessionId;
        CreatedAt = createdAt;
        ActiveDocumentId = activeDocumentId;
        Documents = documents.ToArray();
        PullState = pullState;
        PaletteState = paletteState;
    }
}
