#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Eede.Domain.ImageEditing.Recovery;

namespace Eede.Application.Recovery;

public interface ISessionRecoveryService
{
    Task<bool> HasPendingRecoveryAsync(CancellationToken ct = default);
    Task<bool> IsCrashRecoveryAsync(CancellationToken ct = default);
    Task<SessionSnapshot?> GetRecoveryMetadataAsync(CancellationToken ct = default);
    Task<RestoredSessionData> RestoreSessionAsync(CancellationToken ct = default);
    Task DiscardSessionAsync(CancellationToken ct = default);
}
