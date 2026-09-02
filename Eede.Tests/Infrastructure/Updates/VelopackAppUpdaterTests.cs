using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Eede.Infrastructure.Updates;
using Eede.Domain.SharedKernel;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Collections.Generic;
using Velopack;
using System.Reactive.Linq;

namespace Eede.Tests.Infrastructure.Updates;

[TestFixture]
public class VelopackAppUpdaterTests
{
    private Mock<IUpdateManagerWrapper> _mockManager;
    private VelopackAppUpdater _updater;
    private List<UpdateStatus> _statusChanges;
    private IDisposable _subscription;

    [SetUp]
    public void SetUp()
    {
        _mockManager = new Mock<IUpdateManagerWrapper>();
        // Get the internal constructor using reflection
        var ctor = typeof(VelopackAppUpdater).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(Func<IUpdateManagerWrapper>), typeof(string) },
            null
        );
        _updater = (VelopackAppUpdater)ctor!.Invoke(new object[] { new Func<IUpdateManagerWrapper>(() => _mockManager.Object), "dummy" });

        _statusChanges = new List<UpdateStatus>();
        _subscription = _updater.StatusChanged.Subscribe(s => _statusChanges.Add(s));
    }

    [TearDown]
    public void TearDown()
    {
        _subscription?.Dispose();
        _updater?.Dispose();
    }

    [Test]
    public void IsSupported_ReturnsTrue()
    {
        Assert.That(_updater.IsSupported, Is.True);
    }

    [Test]
    public async Task DownloadUpdateAsync_WhenExceptionThrown_SetsStatusToError()
    {
        // Arrange
        _mockManager.Setup(m => m.IsInstalled).Returns(true);

        #pragma warning disable SYSLIB0050 // FormatterServices is obsolete
        var dummyInfo = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(UpdateInfo)) as UpdateInfo;
        #pragma warning restore SYSLIB0050

        // We set _updateInfo via reflection so that it's not null and we can trigger the download path
        var updateInfoField = typeof(VelopackAppUpdater).GetField("_updateInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        updateInfoField!.SetValue(_updater, dummyInfo);

        _mockManager.Setup(m => m.DownloadUpdatesAsync(It.IsAny<UpdateInfo>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        await _updater.DownloadUpdateAsync();

        // Assert
        Assert.That(_updater.Status, Is.EqualTo(UpdateStatus.Error));
        Assert.That(_statusChanges, Contains.Item(UpdateStatus.Error));
    }

    [Test]
    public async Task CheckForUpdatesAsync_WhenExceptionThrown_SetsStatusToIdle()
    {
        // Arrange
        _mockManager.Setup(m => m.IsInstalled).Returns(true);
        _mockManager.Setup(m => m.CheckForUpdatesAsync())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _updater.CheckForUpdatesAsync();

        // Assert
        Assert.That(result, Is.False);
        Assert.That(_updater.Status, Is.EqualTo(UpdateStatus.Idle));
    }

    [Test]
    public void ApplyAndRestart_WhenExceptionThrown_SetsStatusToError()
    {
        // Arrange
        // We must have ReadyToApply status, so let's set it via reflection
        var statusProperty = typeof(VelopackAppUpdater).GetProperty("Status");
        statusProperty!.SetValue(_updater, UpdateStatus.ReadyToApply);

        #pragma warning disable SYSLIB0050 // FormatterServices is obsolete
        var dummyInfo = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(UpdateInfo)) as UpdateInfo;
        #pragma warning restore SYSLIB0050

        // We set _updateInfo via reflection so that it's not null
        var updateInfoField = typeof(VelopackAppUpdater).GetField("_updateInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        updateInfoField!.SetValue(_updater, dummyInfo);

        _mockManager.Setup(m => m.IsInstalled).Returns(true);
        _mockManager.Setup(m => m.ApplyUpdatesAndRestart(It.IsAny<UpdateInfo>()))
            .Throws(new UnauthorizedAccessException("Permission denied"));

        // Act
        _updater.ApplyAndRestart();

        // Assert
        Assert.That(_updater.Status, Is.EqualTo(UpdateStatus.Error));
        Assert.That(_statusChanges, Contains.Item(UpdateStatus.Error));
    }
}
