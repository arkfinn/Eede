using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Eede.Application.Infrastructure;
using Eede.Domain.Palettes;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.Views.DataEntry;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace Eede.Tests.Presentation.Views.DataEntry;

#nullable enable

[TestFixture]
public class PaletteFormSelectionTests
{
    [Test]
    public void IsSlotSelected_WhenColorMatchesMultipleSlots_ShouldReturnTrueForAllMatchingSlots()
    {
        // Arrange
        var red = new ArgbColor(255, 255, 0, 0);
        var blue = new ArgbColor(255, 0, 0, 255);
        var palette = Palette.Create()
            .Apply(0, red)
            .Apply(1, blue)
            .Apply(5, red)
            .Apply(255, red);

        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That(PaletteSelectionOverlay.IsSlotSelected(palette, 0, red, false, -1, -1), Is.True);
            Assert.That(PaletteSelectionOverlay.IsSlotSelected(palette, 1, red, false, -1, -1), Is.False);
            Assert.That(PaletteSelectionOverlay.IsSlotSelected(palette, 2, red, false, -1, -1), Is.False);
            Assert.That(PaletteSelectionOverlay.IsSlotSelected(palette, 5, red, false, -1, -1), Is.True);
            Assert.That(PaletteSelectionOverlay.IsSlotSelected(palette, 255, red, false, -1, -1), Is.True);
        });
    }

    [TestCase((byte)0, (byte)0, (byte)0, (byte)0)]
    [TestCase((byte)0, (byte)255, (byte)0, (byte)0)]
    public void IsSlotSelected_WhenSelectedColorAlphaIsZero_ShouldReturnFalse(byte a, byte r, byte g, byte b)
    {
        // Arrange
        var selected = new ArgbColor(a, r, g, b);
        var palette = Palette.Create().Apply(0, selected);

        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That(PaletteSelectionOverlay.IsSlotSelected(palette, 0, selected, false, -1, -1), Is.False);
            Assert.That(PaletteSelectionOverlay.IsSlotSelected(palette, 1, selected, false, -1, -1), Is.False);
        });
    }

    [Test]
    public void IsSlotSelected_WhenAlphaDiffersWithSameRgb_ShouldReturnFalse()
    {
        // Arrange
        var slotColor = new ArgbColor(128, 255, 0, 0);
        var selectedColor = new ArgbColor(255, 255, 0, 0);
        var palette = Palette.Create().Apply(0, slotColor);

        // Act
        bool result = PaletteSelectionOverlay.IsSlotSelected(palette, 0, selectedColor, false, -1, -1);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsSlotSelected_WhenDragging_ShouldSkipDragStartAndHoverSlots()
    {
        // Arrange
        var red = new ArgbColor(255, 255, 0, 0);
        var palette = Palette.Create()
            .Apply(0, red)
            .Apply(5, red)
            .Apply(10, red);

        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That(PaletteSelectionOverlay.IsSlotSelected(palette, 0, red, true, 0, 5), Is.False);
            Assert.That(PaletteSelectionOverlay.IsSlotSelected(palette, 5, red, true, 0, 5), Is.False);
            Assert.That(PaletteSelectionOverlay.IsSlotSelected(palette, 10, red, true, 0, 5), Is.True);
        });
    }

    [AvaloniaTest]
    public void PaletteForm_SelectedColorChange_ShouldUpdateOverlayWithoutRecreatingBitmap()
    {
        // Arrange
        var form = new PaletteForm();
        var window = new Window { Content = form };
        window.Show();
        var initialBitmap = form.PaletteBitmap;
        var targetColor = new ArgbColor(255, 10, 20, 30);

        // Act
        form.SelectedColor = targetColor;
        var overlay = form.GetVisualDescendants().OfType<PaletteSelectionOverlay>().FirstOrDefault();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(form.PaletteBitmap, Is.SameAs(initialBitmap));
            Assert.That(overlay, Is.Not.Null);
            Assert.That(overlay!.IsHitTestVisible, Is.False);
            Assert.That(overlay.SelectedColor, Is.EqualTo(targetColor));
        });
    }

    [AvaloniaTest]
    public void PaletteContainer_SelectedColorBinding_ShouldPropagateAndRenderWithoutException()
    {
        // Arrange
        var repoMock = new Mock<IPaletteRepository>();
        var sessionMock = new Mock<IPaletteSessionRepository>();
        sessionMock.Setup(s => s.LoadAsync()).ReturnsAsync(System.Array.Empty<string>());
        var vm = new PaletteContainerViewModel(repoMock.Object, sessionMock.Object);

        var white = new ArgbColor(255, 255, 255, 255);
        vm.SelectedTab!.Palette = Palette.Create()
            .Apply(0, white)
            .Apply(3, white);

        var container = new PaletteContainer { DataContext = vm };
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = container,
            RequestedThemeVariant = ThemeVariant.Light
        };
        window.Show();

        // Act
        vm.SelectedColor = white;

        var form = container.GetVisualDescendants().OfType<PaletteForm>().FirstOrDefault();
        var overlay = container.GetVisualDescendants().OfType<PaletteSelectionOverlay>().FirstOrDefault();

        // Render to RenderTargetBitmap to verify DrawingContext rendering succeeds
        using var rtb = new RenderTargetBitmap(new PixelSize(272, 176), new Vector(96, 96));
        form!.Measure(new Size(272, 176));
        form.Arrange(new Rect(0, 0, 272, 176));
        rtb.Render(form);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(form, Is.Not.Null);
            Assert.That(overlay, Is.Not.Null);
            Assert.That(form.SelectedColor, Is.EqualTo(white));
            Assert.That(overlay!.SelectedColor, Is.EqualTo(white));
            Assert.That(overlay.Palette, Is.EqualTo(vm.SelectedTab.Palette));
        });
    }

    [AvaloniaTest]
    public void PaletteSelectionOverlay_ThemeVariantChange_ShouldRenderWithoutException()
    {
        // Arrange
        var red = new ArgbColor(255, 255, 0, 0);
        var overlay = new PaletteSelectionOverlay
        {
            Width = 272,
            Height = 176,
            Palette = Palette.Create().Apply(0, red),
            SelectedColor = red
        };
        var window = new Window
        {
            Width = 300,
            Height = 200,
            Content = overlay,
            RequestedThemeVariant = ThemeVariant.Light
        };
        window.Show();

        using var rtb = new RenderTargetBitmap(new PixelSize(272, 176), new Vector(96, 96));

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            rtb.Render(overlay);
            window.RequestedThemeVariant = ThemeVariant.Dark;
            rtb.Render(overlay);
        });
    }
}
