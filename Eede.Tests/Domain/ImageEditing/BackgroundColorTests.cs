using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using NUnit.Framework;

namespace Eede.Tests.Domain.ImageEditing;

[TestFixture]
public class BackgroundColorTests
{
    [Test]
    public void Default_ReturnsTransparentColor()
    {
        var defaultColor = BackgroundColor.Default;

        // Transparent is ArgbColor(0, 0, 0, 0)
        Assert.That(defaultColor.Value.EqualsArgb(new ArgbColor(0, 0, 0, 0)), Is.True);
    }

    [Test]
    public void ImplicitConversionToArgbColor_ReturnsCorrectValue()
    {
        var argbColor = new ArgbColor(255, 100, 150, 200);
        var backgroundColor = new BackgroundColor(argbColor);

        ArgbColor result = backgroundColor;

        Assert.That(result.EqualsArgb(argbColor), Is.True);
    }

    [Test]
    public void RecordEquality_SameValues_AreEqual()
    {
        var argbColor = new ArgbColor(255, 255, 0, 0);
        var bg1 = new BackgroundColor(argbColor);
        var bg2 = new BackgroundColor(argbColor);

        Assert.That(bg1, Is.EqualTo(bg2));
    }

    [Test]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        var bg1 = new BackgroundColor(new ArgbColor(255, 255, 0, 0));
        var bg2 = new BackgroundColor(new ArgbColor(255, 0, 255, 0));

        Assert.That(bg1, Is.Not.EqualTo(bg2));
    }
}
