using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Eede.Application.Settings;
using Eede.Infrastructure.Settings;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Settings;

[TestFixture]
public class LocalStorageSettingsRepositoryTests
{
    private Dictionary<string, string> _storage;
    private LocalStorageSettingsRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _storage = new Dictionary<string, string>();
        _repository = new LocalStorageSettingsRepository(
            key => _storage.TryGetValue(key, out var val) ? val : null,
            (key, val) => _storage[key] = val,
            "test_settings_key"
        );
    }

    [Test]
    public async Task LoadAsync_WhenNoDataExists_ReturnsDefaultSettings()
    {
        var settings = await _repository.LoadAsync();

        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.GridWidth, Is.EqualTo(32));
        Assert.That(settings.GridHeight, Is.EqualTo(32));
    }

    [Test]
    public async Task SaveAsync_And_LoadAsync_RoundtripsSettingsSuccessfully()
    {
        var original = new AppSettings
        {
            GridWidth = 48,
            GridHeight = 48
        };
        original.AddRecentFile("sample.png", new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc));

        var saved = await _repository.SaveAsync(original);
        Assert.That(saved, Is.True);

        var loaded = await _repository.LoadAsync();
        Assert.That(loaded.GridWidth, Is.EqualTo(48));
        Assert.That(loaded.GridHeight, Is.EqualTo(48));
        Assert.That(loaded.RecentFiles, Has.Count.EqualTo(1));
        Assert.That(loaded.RecentFiles[0].Path, Is.EqualTo("sample.png"));
    }

    [Test]
    public async Task LoadAsync_WhenJsonIsCorrupted_ReturnsDefaultSettingsWithoutThrowing()
    {
        _storage["test_settings_key"] = "{ corrupted invalid json }";

        var loaded = await _repository.LoadAsync();

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded.GridWidth, Is.EqualTo(32));
        Assert.That(loaded.GridHeight, Is.EqualTo(32));
    }

    [Test]
    public async Task SaveAsync_WhenSetterThrows_ReturnsFalseWithoutCrashing()
    {
        var faultyRepo = new LocalStorageSettingsRepository(
            key => null,
            (key, val) => throw new InvalidOperationException("QuotaExceeded"),
            "test_settings_key"
        );

        var result = await faultyRepo.SaveAsync(new AppSettings());

        Assert.That(result, Is.False);
    }
}
