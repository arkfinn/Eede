using Eede.Domain.Palettes;
using Eede.Presentation.ViewModels.DataEntry;
using NUnit.Framework;
using Avalonia.Headless.NUnit;

namespace Eede.Presentation.Tests.ViewModels.DataEntry;

[TestFixture]
public class PaletteTabViewModelTests
{
    [AvaloniaTest]
    public void Title_WhenFilePathIsNull_ShouldBeTemporaryPalette()
    {
        var palette = Palette.Create();
        var sut = new PaletteTabViewModel(palette, null);

        Assert.That(sut.Title, Is.EqualTo("一時パレット"));
    }

    [AvaloniaTest]
    public void Title_WhenFilePathIsProvided_ShouldBeFileNameWithoutExtension()
    {
        var palette = Palette.Create();
        var sut = new PaletteTabViewModel(palette, @"C:\path\to\test.aact");

        Assert.That(sut.Title, Is.EqualTo("test"));
    }

    [AvaloniaTest]
    public void IsDirty_ShouldBeFalseInitially()
    {
        var palette = Palette.Create();
        var sut = new PaletteTabViewModel(palette, @"C:\path\to\test.aact");

        Assert.That(sut.IsDirty, Is.False);
    }

    [AvaloniaTest]
    public void IsDirty_WhenPaletteChangedAndFilePathIsNotNull_ShouldBeTrue()
    {
        var palette = Palette.Create();
        var sut = new PaletteTabViewModel(palette, @"C:\path\to\test.aact");

        sut.Palette = palette.Apply(0, new ArgbColor(255, 255, 0, 0));

        Assert.That(sut.IsDirty, Is.True);
        Assert.That(sut.Title, Is.EqualTo("test*"));
    }

    [AvaloniaTest]
    public void IsDirty_WhenPaletteChangedAndFilePathIsNull_ShouldStayFalse()
    {
        var palette = Palette.Create();
        var sut = new PaletteTabViewModel(palette, null);

        sut.Palette = palette.Apply(0, new ArgbColor(255, 255, 0, 0));

        Assert.That(sut.IsDirty, Is.False);
        Assert.That(sut.Title, Is.EqualTo("一時パレット"));
    }
}
