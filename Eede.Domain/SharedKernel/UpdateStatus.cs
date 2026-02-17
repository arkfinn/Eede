#nullable enable
namespace Eede.Domain.SharedKernel;

public enum UpdateStatus
{
    Idle,
    Checking,
    Downloading,
    ReadyToApply,
    Error
}
