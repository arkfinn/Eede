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
public class VelopackUpdateServiceTests
{
    private Mock<IUpdateManagerWrapper> _mockManager;
    private VelopackUpdateService _service;
    private List<UpdateStatus> _statusChanges;
    private IDisposable _subscription;

    [SetUp]
    public void SetUp()
    {
        _mockManager = new Mock<IUpdateManagerWrapper>();
        // Get the internal constructor using reflection
        var ctor = typeof(VelopackUpdateService).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(Func<IUpdateManagerWrapper>), typeof(string) },
            null
        );
        _service = (VelopackUpdateService)ctor.Invoke(new object[] { new Func<IUpdateManagerWrapper>(() => _mockManager.Object), "dummy" });

        _statusChanges = new List<UpdateStatus>();
        _subscription = _service.StatusChanged.Subscribe(s => _statusChanges.Add(s));
    }

    [TearDown]
    public void TearDown()
    {
        _subscription?.Dispose();
        _service?.Dispose();
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
        var updateInfoField = typeof(VelopackUpdateService).GetField("_updateInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        updateInfoField.SetValue(_service, dummyInfo);

        _mockManager.Setup(m => m.DownloadUpdatesAsync(It.IsAny<UpdateInfo>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        await _service.DownloadUpdateAsync();

        // Assert
        Assert.That(_service.Status, Is.EqualTo(UpdateStatus.Error));
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
        var result = await _service.CheckForUpdatesAsync();

        // Assert
        Assert.That(result, Is.False);
        Assert.That(_service.Status, Is.EqualTo(UpdateStatus.Idle));
    }

    [Test]
    public void ApplyAndRestart_WhenExceptionThrown_SetsStatusToError()
    {
        // Arrange
        // We must have ReadyToApply status, so let's set it via reflection
        var statusProperty = typeof(VelopackUpdateService).GetProperty("Status");
        statusProperty.SetValue(_service, UpdateStatus.ReadyToApply);

        #pragma warning disable SYSLIB0050 // FormatterServices is obsolete
        var dummyInfo = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(UpdateInfo)) as UpdateInfo;
        #pragma warning restore SYSLIB0050

        // We set _updateInfo via reflection so that it's not null
        var updateInfoField = typeof(VelopackUpdateService).GetField("_updateInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        updateInfoField.SetValue(_service, dummyInfo);

        _mockManager.Setup(m => m.IsInstalled).Returns(true);
        _mockManager.Setup(m => m.ApplyUpdatesAndRestart(It.IsAny<UpdateInfo>()))
            .Throws(new UnauthorizedAccessException("Permission denied"));

        // Act
        _service.ApplyAndRestart();

        // Assert
        Assert.That(_service.Status, Is.EqualTo(UpdateStatus.Error));
        Assert.That(_statusChanges, Contains.Item(UpdateStatus.Error));
    }
}
