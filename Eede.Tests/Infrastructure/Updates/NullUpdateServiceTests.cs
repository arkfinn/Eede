using System;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Updates;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Updates;

[TestFixture]
public class NullUpdateServiceTests
{
    [Test]
    public async Task CheckForUpdatesAsync_AlwaysReturnsFalse()
    {
        var service = new NullUpdateService();
        var result = await service.CheckForUpdatesAsync();

        Assert.That(result, Is.False);
        Assert.That(service.Status, Is.EqualTo(UpdateStatus.Idle));
    }

    [Test]
    public async Task DownloadUpdateAsync_DoesNotThrow()
    {
        var service = new NullUpdateService();
        Assert.DoesNotThrowAsync(async () => await service.DownloadUpdateAsync());
        Assert.That(service.Status, Is.EqualTo(UpdateStatus.Idle));
    }

    [Test]
    public void ApplyAndRestart_DoesNotThrow()
    {
        var service = new NullUpdateService();
        Assert.DoesNotThrow(() => service.ApplyAndRestart());
        Assert.That(service.Status, Is.EqualTo(UpdateStatus.Idle));
    }
}
