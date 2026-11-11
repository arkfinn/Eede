using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.Animations;

[TestFixture]
public class GridSettingsTests
{
    [Test]
    public void CalculateCellIndex_ReturnsCorrectIndex()
    {
        var settings = new GridSettings(new PictureSize(10, 10), new Position(0, 0), 0);
        var imageSize = new PictureSize(30, 30); // 3 columns, 3 rows

        var index = settings.CalculateCellIndex(new Position(15, 5), imageSize);
        // col = 15/10 = 1, row = 5/10 = 0
        // index = 0 * 3 + 1 = 1
        Assert.That(index, Is.EqualTo(1));
    }

    [Test]
    public void CalculateCellIndex_WithOffset_ReturnsCorrectIndex()
    {
        var settings = new GridSettings(new PictureSize(10, 10), new Position(5, 5), 0);
        var imageSize = new PictureSize(35, 35); // columns = (35-5)/10 = 3

        var index = settings.CalculateCellIndex(new Position(16, 16), imageSize);
        // col = (16-5)/10 = 1, row = (16-5)/10 = 1
        // index = 1 * 3 + 1 = 4
        Assert.That(index, Is.EqualTo(4));
    }

    [Test]
    public void CalculateCellIndex_WithPadding_ReturnsCorrectIndex()
    {
        var settings = new GridSettings(new PictureSize(10, 10), new Position(0, 0), 2);
        var imageSize = new PictureSize(34, 34); // columns = (34+2)/12 = 3

        var index = settings.CalculateCellIndex(new Position(15, 15), imageSize);
        // col = 15/12 = 1, row = 15/12 = 1
        // index = 1 * 3 + 1 = 4
        Assert.That(index, Is.EqualTo(4));
    }

    [Test]
    public void CalculateCellIndex_OutOfBoundsX_ReturnsMinusOne()
    {
        var settings = new GridSettings(new PictureSize(10, 10), new Position(0, 0), 0);
        var imageSize = new PictureSize(30, 30); // 3 columns

        // col >= columns
        var indexRight = settings.CalculateCellIndex(new Position(35, 5), imageSize);
        // col = 35/10 = 3 >= 3
        Assert.That(indexRight, Is.EqualTo(-1));

        // col < 0
        var indexLeft = settings.CalculateCellIndex(new Position(-15, 5), imageSize);
        // col = -15/10 = -1 < 0
        Assert.That(indexLeft, Is.EqualTo(-1));

        // col < 0, near zero (was erroneously returning 0 due to integer truncation toward zero)
        var indexLeftNearZero = settings.CalculateCellIndex(new Position(-5, 5), imageSize);
        Assert.That(indexLeftNearZero, Is.EqualTo(-1));
    }

    [Test]
    public void CalculateCellIndex_OutOfBoundsY_ReturnsMinusOne()
    {
        var settings = new GridSettings(new PictureSize(10, 10), new Position(0, 0), 0);
        var imageSize = new PictureSize(30, 30); // 3 columns, 3 rows

        // index < 0 (row < 0)
        var indexTop = settings.CalculateCellIndex(new Position(5, -15), imageSize);
        Assert.That(indexTop, Is.EqualTo(-1));

        // row < 0, near zero (was erroneously returning 0)
        var indexTopNearZero = settings.CalculateCellIndex(new Position(5, -5), imageSize);
        Assert.That(indexTopNearZero, Is.EqualTo(-1));

        // row >= rows (bottom overflow)
        var indexBottom = settings.CalculateCellIndex(new Position(5, 35), imageSize);
        Assert.That(indexBottom, Is.EqualTo(-1));
    }

    [Test]
    public void CalculateCellIndex_PositionBeforeOffset_ReturnsMinusOne()
    {
        var settings = new GridSettings(new PictureSize(10, 10), new Position(5, 5), 0);
        var imageSize = new PictureSize(35, 35); // 3 columns

        // Position X before offset (1 < 5) should be out of bounds (-1)
        var indexBeforeOffsetX = settings.CalculateCellIndex(new Position(1, 5), imageSize);
        Assert.That(indexBeforeOffsetX, Is.EqualTo(-1));

        // Position Y before offset (2 < 5) should be out of bounds (-1)
        var indexBeforeOffsetY = settings.CalculateCellIndex(new Position(5, 2), imageSize);
        Assert.That(indexBeforeOffsetY, Is.EqualTo(-1));
    }

    [Test]
    public void Equality_RecordsWithSameValuesAreEqual()
    {
        var settings1 = new GridSettings(new PictureSize(10, 10), new Position(5, 5), 2);
        var settings2 = new GridSettings(new PictureSize(10, 10), new Position(5, 5), 2);

        Assert.That(settings1, Is.EqualTo(settings2));
        Assert.That(settings1 == settings2, Is.True);
    }

    [Test]
    public void Inequality_RecordsWithDifferentValuesAreNotEqual()
    {
        var settings1 = new GridSettings(new PictureSize(10, 10), new Position(5, 5), 2);
        var settings2 = new GridSettings(new PictureSize(20, 10), new Position(5, 5), 2);

        Assert.That(settings1, Is.Not.EqualTo(settings2));
        Assert.That(settings1 != settings2, Is.True);
    }

    [Test]
    public void Constructor_WithInvalidValues_ThrowsArgumentOutOfRangeException()
    {
        // cellSize Width <= 0
        var exW = Assert.Throws<System.ArgumentOutOfRangeException>(() =>
            new GridSettings(new PictureSize(0, 10), new Position(0, 0), 0));
        Assert.That(exW.ParamName, Is.EqualTo("cellSize"));
        Assert.That(exW.Message, Does.Contain("Cell dimensions must be positive."));

        // cellSize Height <= 0
        var exH = Assert.Throws<System.ArgumentOutOfRangeException>(() =>
            new GridSettings(new PictureSize(10, 0), new Position(0, 0), 0));
        Assert.That(exH.ParamName, Is.EqualTo("cellSize"));
        Assert.That(exH.Message, Does.Contain("Cell dimensions must be positive."));

        // offset X < 0
        var exX = Assert.Throws<System.ArgumentOutOfRangeException>(() =>
            new GridSettings(new PictureSize(10, 10), new Position(-1, 0), 0));
        Assert.That(exX.ParamName, Is.EqualTo("offset"));
        Assert.That(exX.Message, Does.Contain("Offset coordinates must be non-negative."));

        // offset Y < 0
        var exY = Assert.Throws<System.ArgumentOutOfRangeException>(() =>
            new GridSettings(new PictureSize(10, 10), new Position(0, -1), 0));
        Assert.That(exY.ParamName, Is.EqualTo("offset"));
        Assert.That(exY.Message, Does.Contain("Offset coordinates must be non-negative."));

        // padding < 0
        var exP = Assert.Throws<System.ArgumentOutOfRangeException>(() =>
            new GridSettings(new PictureSize(10, 10), new Position(0, 0), -1));
        Assert.That(exP.ParamName, Is.EqualTo("padding"));
        Assert.That(exP.Message, Does.Contain("Padding must be non-negative."));
    }

    [Test]
    public void CalculateCellIndex_WithOffsetAndPadding_CalculatesCorrectRowAndColumn()
    {
        // CellSize: 10x10, Offset: (5, 5), Padding: 2 -> step = 12
        var settings = new GridSettings(new PictureSize(10, 10), new Position(5, 5), 2);
        // imageSize: Width=41, Height=41 -> (41 - 5 + 2) / 12 = 38 / 12 = 3 columns, 3 rows
        var imageSize = new PictureSize(41, 41);

        // Position: (5 + 12, 5 + 12) = (17, 17) -> col = 1, row = 1 -> index = 1 * 3 + 1 = 4
        var index = settings.CalculateCellIndex(new Position(17, 17), imageSize);
        Assert.That(index, Is.EqualTo(4));

        // Position: (5, 5 + 24) = (5, 29) -> col = 0, row = 2 -> index = 2 * 3 + 0 = 6
        var indexRow2 = settings.CalculateCellIndex(new Position(5, 29), imageSize);
        Assert.That(indexRow2, Is.EqualTo(6));

        // Out of bounds row: row >= 3 (Y >= 5 + 36 = 41)
        var outOfBoundsY = settings.CalculateCellIndex(new Position(17, 41), imageSize);
        Assert.That(outOfBoundsY, Is.EqualTo(-1));
    }

    [Test]
    public void Validate_ReturnsTrueForValidSettings()
    {
        var settings = new GridSettings(new PictureSize(10, 10), new Position(0, 0), 0);
        Assert.That(settings.Validate(), Is.True);

        var uninitialized = (GridSettings)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(GridSettings));
        Assert.That(uninitialized.Validate(), Is.False);
    }
}
