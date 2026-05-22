using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System.Linq;

namespace Eede.Domain.Tests.ImageEditing;

[TestFixture]
public class BackgroundCalculatorTests
{
    [Test]
    public void CalculateVerticalGridLinesTest()
    {
        var calculator = new BackgroundCalculator();
        var pictureSize = new PictureSize(32, 32);
        var gridSize = new PictureSize(16, 16);
        var magnification = new Magnification(2);

        var lines = calculator.CalculateVerticalGridLines(pictureSize, gridSize, magnification).ToList();

        // 32/16 = 2 columns, so 1 vertical line between them
        Assert.That(lines.Count, Is.EqualTo(1));
        // 16 * 2 = 32 (display coordinate)
        Assert.That(lines[0].Start.X, Is.EqualTo(32));
        Assert.That(lines[0].Start.Y, Is.EqualTo(0));
        Assert.That(lines[0].End.X, Is.EqualTo(32));
        Assert.That(lines[0].End.Y, Is.EqualTo(64)); // 32 * 2 = 64
    }

    [Test]
    public void CalculateHorizontalGridLinesTest()
    {
        var calculator = new BackgroundCalculator();
        var pictureSize = new PictureSize(32, 32);
        var gridSize = new PictureSize(16, 16);
        var magnification = new Magnification(2);

        var lines = calculator.CalculateHorizontalGridLines(pictureSize, gridSize, magnification).ToList();

        Assert.That(lines.Count, Is.EqualTo(1));
        Assert.That(lines[0].Start.X, Is.EqualTo(0));
        Assert.That(lines[0].Start.Y, Is.EqualTo(32));
        Assert.That(lines[0].End.X, Is.EqualTo(64));
        Assert.That(lines[0].End.Y, Is.EqualTo(32));
    }
}
