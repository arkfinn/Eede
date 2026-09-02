#nullable enable
using System;
using Eede.Domain.SharedKernel;

namespace Eede.Application.Recovery;

public sealed class PullContextTracker : IPullContextTracker
{
    private readonly object _lock = new();
    private PullContext? _currentContext;

    public PullContext? CurrentContext
    {
        get
        {
            lock (_lock)
            {
                return _currentContext;
            }
        }
    }

    public void SetPullContext(string sourceDocumentId, PictureArea sourceArea)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDocumentId);
        lock (_lock)
        {
            _currentContext = new PullContext(sourceDocumentId, sourceArea);
        }
    }

    public void ClearPullContext()
    {
        lock (_lock)
        {
            _currentContext = null;
        }
    }
}
