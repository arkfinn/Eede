using System;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Updates;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Updates;

[TestFixture]
public class NullAppUpdaterTests
{
    [Test]
    public void IsSupported_ReturnsFalse()
    {
        var updater = new NullAppUpdater();
        Assert.That(updater.IsSupported, Is.False);
    }

    [Test]
    public async Task CheckForUpdatesAsync_AlwaysReturnsFalse()
    {
        var updater = new NullAppUpdater();
        var result = await updater.CheckForUpdatesAsync();

        Assert.That(result, Is.False);
        Assert.That(updater.Status, Is.EqualTo(UpdateStatus.Idle));
    }

    [Test]
    public async Task DownloadUpdateAsync_DoesNotThrow()
    {
        var updater = new NullAppUpdater();
        Assert.DoesNotThrowAsync(async () => await updater.DownloadUpdateAsync());
        Assert.That(updater.Status, Is.EqualTo(UpdateStatus.Idle));
    }

    [Test]
    public void ApplyAndRestart_DoesNotThrow()
    {
        var updater = new NullAppUpdater();
        Assert.DoesNotThrow(() => updater.ApplyAndRestart());
        Assert.That(updater.Status, Is.EqualTo(UpdateStatus.Idle));
    }
}
