using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools;

[TestFixture]
public class TilePatternTests
{
    [Test]
    public void IsDrawable_WorksCorrectlyIncludingNegativeCoordinates()
    {
        Assert.Multiple(() =>
        {
            Assert.That(TilePattern.Solid.IsDrawable(0, 0), Is.True);
            Assert.That(TilePattern.Solid.IsDrawable(-3, -5), Is.True);

            // 87.5% (14/16): 穴は (0,0) と (2,2)
            Assert.That(TilePattern.Dense87.IsDrawable(0, 0), Is.False);
            Assert.That(TilePattern.Dense87.IsDrawable(1, 0), Is.True);
            Assert.That(TilePattern.Dense87.IsDrawable(2, 2), Is.False);
            Assert.That(TilePattern.Dense87.IsDrawable(-2, -2), Is.False);

            // 75% (12/16): 偶数行の偶数列のみ抜き
            Assert.That(TilePattern.Dense75.IsDrawable(0, 0), Is.False);
            Assert.That(TilePattern.Dense75.IsDrawable(1, 0), Is.True);
            Assert.That(TilePattern.Dense75.IsDrawable(0, 1), Is.True);
            Assert.That(TilePattern.Dense75.IsDrawable(-2, -2), Is.False);

            // 62.5% (10/16)
            Assert.That(TilePattern.Dense62.IsDrawable(0, 0), Is.False);
            Assert.That(TilePattern.Dense62.IsDrawable(1, 0), Is.True);
            Assert.That(TilePattern.Dense62.IsDrawable(3, 1), Is.False);
            Assert.That(TilePattern.Dense62.IsDrawable(1, 3), Is.False);

            // 50% (8/16): 市松 (x+yが奇数)
            Assert.That(TilePattern.Checker50.IsDrawable(0, 0), Is.False);
            Assert.That(TilePattern.Checker50.IsDrawable(1, 0), Is.True);
            Assert.That(TilePattern.Checker50.IsDrawable(-1, -1), Is.False);
            Assert.That(TilePattern.Checker50.IsDrawable(-1, 0), Is.True);

            // 37.5% (6/16)
            Assert.That(TilePattern.Sparse37.IsDrawable(3, 0), Is.True);
            Assert.That(TilePattern.Sparse37.IsDrawable(0, 1), Is.True);
            Assert.That(TilePattern.Sparse37.IsDrawable(1, 2), Is.True);
            Assert.That(TilePattern.Sparse37.IsDrawable(0, 0), Is.False);

            // 25% (4/16): 奇数行の偶数列のみ描画
            Assert.That(TilePattern.Sparse25.IsDrawable(0, 0), Is.False);
            Assert.That(TilePattern.Sparse25.IsDrawable(0, 1), Is.True);
            Assert.That(TilePattern.Sparse25.IsDrawable(1, 1), Is.False);
            Assert.That(TilePattern.Sparse25.IsDrawable(-2, -1), Is.True);

            // 12.5% (2/16): (0,1) と (2,3) のみ描画
            Assert.That(TilePattern.Sparse12.IsDrawable(0, 1), Is.True);
            Assert.That(TilePattern.Sparse12.IsDrawable(2, 3), Is.True);
            Assert.That(TilePattern.Sparse12.IsDrawable(0, 0), Is.False);
            Assert.That(TilePattern.Sparse12.IsDrawable(2, 1), Is.False);
        });
    }

    [Test]
    public void EightStageDensityRamp_HasExactDotCountsPer4x4Tile()
    {
        var expectedCounts = new[]
        {
            (TilePattern.Solid, 16),
            (TilePattern.Dense87, 14),
            (TilePattern.Dense75, 12),
            (TilePattern.Dense62, 10),
            (TilePattern.Checker50, 8),
            (TilePattern.Sparse37, 6),
            (TilePattern.Sparse25, 4),
            (TilePattern.Sparse12, 2),
        };

        Assert.Multiple(() =>
        {
            foreach (var (pattern, expectedCount) in expectedCounts)
            {
                int count = 0;
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        if (pattern.IsDrawable(x, y)) count++;
                    }
                }
                Assert.That(count, Is.EqualTo(expectedCount), $"{pattern.Type} should fill {expectedCount}/16 pixels");
            }
        });
    }

    [Test]
    public void DrawFillRectangle_WithChecker50_PreservesBackgroundOnUnmarkedPixels()
    {
        var size = new PictureSize(4, 4);
        var bg = new ArgbColor(255, 10, 20, 30);
        var fg = new ArgbColor(255, 255, 0, 0);
        var picture = CreateMonoPicture(size, bg);

        var drawer = new Drawer(picture, new PenStyle(new DirectImageBlender(), fg, 1, TilePattern.Checker50));
        var result = drawer.DrawFillRectangle(new Position(0, 0), new Position(3, 3));

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                var expected = ((x + y) & 1) == 1 ? fg : bg;
                Assert.That(result.Picture.PickColor(new Position(x, y)), Is.EqualTo(expected), $"({x},{y})");
            }
        }
    }

    [Test]
    public void AlphaImageBlender_WithTilePattern_DoesNotDoubleBlendSkippedOrOutsideSemiTransparentPixels()
    {
        var size = new PictureSize(4, 4);
        var semiTransparent = new ArgbColor(128, 100, 150, 200);
        var redSemi = new ArgbColor(128, 255, 0, 0);
        var picture = CreateMonoPicture(size, semiTransparent);

        // (0,0)-(1,1) の範囲にのみ市松模様で半透明赤を描画
        var drawer = new Drawer(picture, new PenStyle(new AlphaImageBlender(), redSemi, 1, TilePattern.Checker50));
        var result = drawer.DrawFillRectangle(new Position(0, 0), new Position(1, 1));

        Assert.Multiple(() =>
        {
            // (1,0), (0,1) は描かれている（赤と半透明青が合成された色）
            Assert.That(result.Picture.PickColor(new Position(1, 0)), Is.Not.EqualTo(semiTransparent));
            Assert.That(result.Picture.PickColor(new Position(0, 1)), Is.Not.EqualTo(semiTransparent));

            // (0,0), (1,1) はパターン抜け座標 → 元色そのまま (濃化 A=192 にならない!)
            Assert.That(result.Picture.PickColor(new Position(0, 0)), Is.EqualTo(semiTransparent));
            Assert.That(result.Picture.PickColor(new Position(1, 1)), Is.EqualTo(semiTransparent));

            // (2,2), (3,3) は描画エリア外の半透明画素 → 元色そのまま
            Assert.That(result.Picture.PickColor(new Position(2, 2)), Is.EqualTo(semiTransparent));
            Assert.That(result.Picture.PickColor(new Position(3, 3)), Is.EqualTo(semiTransparent));
        });
    }

    [Test]
    public void WithOffsetX_ShiftsHorizontalPhaseByOnePixel()
    {
        var dense87Shifted = TilePattern.Dense87.WithOffsetX(1);
        var dense62Shifted = TilePattern.Dense62.WithOffsetX(1);
        var checkerShifted = TilePattern.Checker50.WithOffsetX(1);
        var sparse37Shifted = TilePattern.Sparse37.WithOffsetX(1);
        var sparse12Shifted = TilePattern.Sparse12.WithOffsetX(1);

        Assert.Multiple(() =>
        {
            Assert.That(checkerShifted.OffsetX, Is.EqualTo(1));
            Assert.That(checkerShifted.WithOffsetX(0), Is.SameAs(TilePattern.Checker50));
            Assert.That(checkerShifted, Is.Not.EqualTo(TilePattern.Checker50));

            for (int y = -2; y <= 4; y++)
            {
                for (int x = -2; x <= 4; x++)
                {
                    Assert.That(dense87Shifted.IsDrawable(x + 1, y), Is.EqualTo(TilePattern.Dense87.IsDrawable(x, y)));
                    Assert.That(dense62Shifted.IsDrawable(x + 1, y), Is.EqualTo(TilePattern.Dense62.IsDrawable(x, y)));
                    Assert.That(checkerShifted.IsDrawable(x + 1, y), Is.EqualTo(TilePattern.Checker50.IsDrawable(x, y)));
                    Assert.That(sparse37Shifted.IsDrawable(x + 1, y), Is.EqualTo(TilePattern.Sparse37.IsDrawable(x, y)));
                    Assert.That(sparse12Shifted.IsDrawable(x + 1, y), Is.EqualTo(TilePattern.Sparse12.IsDrawable(x, y)));
                }
            }
        });
    }

    private static Picture CreateMonoPicture(PictureSize size, ArgbColor color)
    {
        var data = new byte[size.Width * size.Height * 4];
        for (int i = 0; i < data.Length; i += 4)
        {
            data[i] = color.Blue;
            data[i + 1] = color.Green;
            data[i + 2] = color.Red;
            data[i + 3] = color.Alpha;
        }
        return Picture.Create(size, data);
    }
}
