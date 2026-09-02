#nullable enable
using System;
using Eede.Domain.SharedKernel;

namespace Eede.Application.Recovery;

public record PullContext
{
    public string SourceDocumentId { get; init; }
    public PictureArea SourceArea { get; init; }

    public PullContext(string sourceDocumentId, PictureArea sourceArea)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDocumentId);
        SourceDocumentId = sourceDocumentId;
        SourceArea = sourceArea;
    }
}

public interface IPullContextTracker
{
    PullContext? CurrentContext { get; }
    void SetPullContext(string sourceDocumentId, PictureArea sourceArea);
    void ClearPullContext();
}
