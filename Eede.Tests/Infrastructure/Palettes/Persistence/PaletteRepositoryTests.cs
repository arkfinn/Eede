#nullable enable
using System;
using System.IO;
using Eede.Domain.Palettes;
using Eede.Infrastructure.Palettes.Persistence;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Palettes.Persistence;

[TestFixture]
public class PaletteRepositoryTests
{
    private PaletteRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new PaletteRepository();
    }

    [Test]
    public void Find_WithStream_LoadsActPalette()
    {
        byte[] mockAct = new byte[768];
        mockAct[0] = 255; // Color 0 Red
        using MemoryStream ms = new(mockAct);

        var palette = _repository.Find(ms, ".act");

        Assert.That(palette, Is.Not.Null);
        Assert.That(palette.Fetch(0).Red, Is.EqualTo(255));
    }

    [Test]
    public void Find_WithStream_LoadsAlphaActPalette()
    {
        byte[] mockAact = new byte[1024];
        mockAact[0] = 255; // Color 0 Red
        mockAact[3] = 128; // Color 0 Alpha
        using MemoryStream ms = new(mockAact);

        var palette = _repository.Find(ms, ".aact");

        Assert.That(palette, Is.Not.Null);
        Assert.That(palette.Fetch(0).Red, Is.EqualTo(255));
        Assert.That(palette.Fetch(0).Alpha, Is.EqualTo(128));
    }

    [Test]
    public void Find_WithUnsupportedExtension_ThrowsNotSupportedException()
    {
        using MemoryStream ms = new();
        Assert.Throws<NotSupportedException>(() => _repository.Find(ms, ".xyz"));
    }

    [Test]
    public void Save_WithStream_WritesActPalette()
    {
        var colors = new ArgbColor[256];
        colors[0] = new ArgbColor(255, 100, 150, 200);
        for (int i = 1; i < 256; i++) colors[i] = new ArgbColor(255, 0, 0, 0);
        var palette = Palette.FromColors(colors);

        using MemoryStream ms = new();
        _repository.Save(palette, ms, ".act");

        Assert.That(ms.Length, Is.EqualTo(768));
        ms.Position = 0;
        Assert.That(ms.ReadByte(), Is.EqualTo(100));
        Assert.That(ms.ReadByte(), Is.EqualTo(150));
        Assert.That(ms.ReadByte(), Is.EqualTo(200));
    }

    [Test]
    public void Save_WithStream_WritesAlphaActPalette()
    {
        var colors = new ArgbColor[256];
        colors[0] = new ArgbColor(200, 10, 20, 30);
        for (int i = 1; i < 256; i++) colors[i] = new ArgbColor(0, 0, 0, 0);
        var palette = Palette.FromColors(colors);

        using MemoryStream ms = new();
        _repository.Save(palette, ms, ".aact");

        Assert.That(ms.Length, Is.EqualTo(1024));
        ms.Position = 0;
        Assert.That(ms.ReadByte(), Is.EqualTo(10));
        Assert.That(ms.ReadByte(), Is.EqualTo(20));
        Assert.That(ms.ReadByte(), Is.EqualTo(30));
        Assert.That(ms.ReadByte(), Is.EqualTo(200));
    }

    [Test]
    public void Save_WithUnsupportedExtension_ThrowsNotSupportedException()
    {
        var palette = Palette.FromColors(new ArgbColor[256]);
        using MemoryStream ms = new();
        Assert.Throws<NotSupportedException>(() => _repository.Save(palette, ms, ".invalid"));
    }
}
