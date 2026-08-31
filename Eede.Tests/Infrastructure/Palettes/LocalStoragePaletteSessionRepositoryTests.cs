using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eede.Infrastructure.Palettes.Persistence;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Palettes;

[TestFixture]
public class LocalStoragePaletteSessionRepositoryTests
{
    private Dictionary<string, string> _storage;
    private LocalStoragePaletteSessionRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _storage = new Dictionary<string, string>();
        _repository = new LocalStoragePaletteSessionRepository(
            key => _storage.TryGetValue(key, out var val) ? val : null,
            (key, val) => _storage[key] = val,
            "test_palette_key"
        );
    }

    [Test]
    public async Task LoadAsync_WhenNoData_ReturnsEmptyArray()
    {
        var result = await _repository.LoadAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task SaveAsync_And_LoadAsync_RoundtripsSuccessfully()
    {
        var paths = new[] { "pal1.act", "pal2.act" };
        await _repository.SaveAsync(paths);

        var loaded = await _repository.LoadAsync();
        Assert.That(loaded, Is.EquivalentTo(paths));
    }

    [Test]
    public async Task LoadAsync_WhenCorrupted_ReturnsEmptyArrayWithoutThrowing()
    {
        _storage["test_palette_key"] = "not json";

        var loaded = await _repository.LoadAsync();
        Assert.That(loaded, Is.Empty);
    }
}
