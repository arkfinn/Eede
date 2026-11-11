using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools
{
    [TestFixture()]
    public class DrawerTests
    {
        [Test()]
        public void DrawPointTest()
        {
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1));
            var result = drawer.DrawPoint(new Position(5, 5));
            Picture dst = result.Picture;
            //dstBmp.Save(@"ImageEditing\DrawingTools\test\dest.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"ImageEditing\DrawingTools\test\point1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [TestCase(10, 10, 14, 14, @"ImageEditing\DrawingTools\test\ellipse1.png")]
        [TestCase(10, 10, 15, 15, @"ImageEditing\DrawingTools\test\ellipse2.png")]
        public void DrawEllipseTest(int fx, int fy, int tx, int ty, string expectedImagePath)
        {
            // Arrange
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            var result = drawer.DrawEllipse(new Position(fx, fy), new Position(tx, ty));
            Picture dst = result.Picture;
            //PictureHelper.WriteBitmap(@"ImageEditing\DrawingTools\test\ellipse_test.png", dst);

            // Assert
            Picture expected = ReadPicture(expectedImagePath);
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [TestCase(5, 5, 9, 9, @"ImageEditing\DrawingTools\test\fillEllipse1.png")]
        [TestCase(5, 5, 10, 10, @"ImageEditing\DrawingTools\test\fillEllipse2.png")]
        public void DrawFillEllipseTest(int fx, int fy, int tx, int ty, string expectedImagePath)
        {
            // Arrange
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            var result = drawer.DrawFillEllipse(new Position(fx, fy), new Position(tx, ty));
            Picture dst = result.Picture;
            //PictureHelper.WriteBitmap(@"ImageEditing\DrawingTools\test\fillEllipse_test.png", dst);

            // Assert
            Picture expected = ReadPicture(expectedImagePath);
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test]
        public void DrawRectangleTest()
        {
            // Arrange
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            var result = drawer.DrawRectangle(new(10, 10), new(15, 15));
            Picture dst = result.Picture;
            //PictureHelper.WriteBitmap(@"ImageEditing\DrawingTools\test\test.png", dst);

            // Assert
            Picture expected = ReadPicture(@"ImageEditing\DrawingTools\test\rectangle1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test]
        public void DrawFillRectangleTest()
        {
            // Arrange
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            var result = drawer.DrawFillRectangle(new(10, 10), new(15, 15));
            Picture dst = result.Picture;
            // PictureHelper.WriteBitmap(@"ImageEditing\DrawingTools\test\test.png", dst);

            // Assert
            Picture expected = ReadPicture(@"ImageEditing\DrawingTools\test\fillRectangle1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test]
        public void DrawPoint_WithThickPen_DrawsCorrectAreaAndPixels()
        {
            // Arrange: 32x32 の白画像 (A=255)
            byte[] pixels = new byte[32 * 32 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255; pixels[i + 1] = 255; pixels[i + 2] = 255; pixels[i + 3] = 255;
            }
            Picture src = Picture.Create(new PictureSize(32, 32), pixels);

            // 幅 3 の黒ペン
            PenStyle thickPen = new(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 3);
            Drawer drawer = new(src, thickPen);

            // Act: (10, 10) に点描画
            var result = drawer.DrawPoint(new Position(10, 10));

            // Assert: 幅 3 のため d=1, from=(9, 9), Size=(3, 3)
            var area = result.Area;
            Assert.Multiple(() =>
            {
                Assert.That(area.Position.X, Is.EqualTo(9));
                Assert.That(area.Position.Y, Is.EqualTo(9));
                Assert.That(area.Size.Width, Is.EqualTo(3));
                Assert.That(area.Size.Height, Is.EqualTo(3));

                // 中心 (10, 10) が黒 (R=0, G=0, B=0, A=255) になっていること
                byte[] img = result.Picture.CloneImage();
                int centerIndex = (10 * 4) + (32 * 4 * 10);
                Assert.That(img[centerIndex], Is.EqualTo(0));
                Assert.That(img[centerIndex + 1], Is.EqualTo(0));
                Assert.That(img[centerIndex + 2], Is.EqualTo(0));
                Assert.That(img[centerIndex + 3], Is.EqualTo(255));
            });
        }

        [Test]
        public void DrawPoint_WithCanvasCoordinate_ExecutesSuccessfully()
        {
            // Arrange
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1));

            // Act
            var result = drawer.DrawPoint(new CanvasCoordinate(5, 5));

            // Assert
            Assert.That(result.Area.Position, Is.EqualTo(new Position(5, 5)));
        }

        [Test]
        public void Fill_WhenTargetColorEqualsPenColor_ReturnsEmptyAreaAndUnchangedPicture()
        {
            // Arrange: 8x8 の真っ赤な画像
            byte[] pixels = new byte[8 * 8 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i + 2] = 255; // Red
                pixels[i + 3] = 255; // Alpha
            }
            Picture src = Picture.Create(new PictureSize(8, 8), pixels);

            // ペン色も赤
            PenStyle redPen = new(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1);
            Drawer drawer = new(src, redPen);

            // Act: 赤い画像に対して赤で Fill を実行
            var result = drawer.Fill(new Position(2, 2));

            // Assert: 何も塗られず、空エリアが返され、画像配列は完全に同一
            Assert.Multiple(() =>
            {
                Assert.That(result.Area.IsEmpty, Is.True);
                Assert.That(result.Picture.CloneImage(), Is.EqualTo(src.CloneImage()));
            });
        }

        [Test]
        public void Fill_WithCanvasCoordinate_ExecutesSuccessfully()
        {
            // Arrange: 8x8 の真っ黒な画像
            byte[] pixels = new byte[8 * 8 * 4];
            for (int i = 0; i < pixels.Length; i += 4) { pixels[i + 3] = 255; }
            Picture src = Picture.Create(new PictureSize(8, 8), pixels);

            PenStyle redPen = new(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1);
            Drawer drawer = new(src, redPen);

            // Act
            var result = drawer.Fill(new CanvasCoordinate(2, 2));

            // Assert
            Assert.That(result.Area.IsEmpty, Is.False);
            Assert.That(result.Area.Size, Is.EqualTo(new PictureSize(8, 8)));
        }

        [TestCase(0, 0, 10, 5, Description = "dx > dy, x2 > x1, y2 > y1 (右下緩傾斜)")]
        [TestCase(10, 5, 0, 0, Description = "dx > dy, x2 < x1, y2 < y1 (左上緩傾斜)")]
        [TestCase(0, 5, 10, 0, Description = "dx > dy, x2 > x1, y2 < y1 (右上緩傾斜)")]
        [TestCase(10, 0, 0, 5, Description = "dx > dy, x2 < x1, y2 > y1 (左下緩傾斜)")]
        [TestCase(0, 0, 5, 10, Description = "dy >= dx, x2 > x1, y2 > y1 (右下急傾斜)")]
        [TestCase(5, 10, 0, 0, Description = "dy >= dx, x2 < x1, y2 < y1 (左上急傾斜)")]
        [TestCase(0, 10, 5, 0, Description = "dy >= dx, x2 > x1, y2 < y1 (右上急傾斜)")]
        [TestCase(5, 0, 0, 10, Description = "dy >= dx, x2 < x1, y2 > y1 (左下急傾斜)")]
        [TestCase(5, 5, 5, 5, Description = "1点 (dx=0, dy=0)")]
        [TestCase(-5, -5, 20, 20, Description = "画面外から画面外へ突き抜ける線（境界クリッピング網羅）")]
        [TestCase(-5, 5, 20, 5, Description = "画面外から画面外への水平線")]
        [TestCase(5, -5, 5, 20, Description = "画面外から画面外への垂直線")]
        public void DrawLine_VariousDirectionsAndClipping(int x1, int y1, int x2, int y2)
        {
            var white = new ArgbColor(255, 255, 255, 255);
            var red = new ArgbColor(255, 255, 0, 0);
            byte[] pixels = new byte[16 * 16 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255; pixels[i + 1] = 255; pixels[i + 2] = 255; pixels[i + 3] = 255;
            }
            Picture src = Picture.Create(new PictureSize(16, 16), pixels);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            var result = drawer.DrawLine(new Position(x1, y1), new Position(x2, y2));
            Picture dst = result.Picture;

            // 始点または終点が画像内にある場合、赤くなっていることを確認
            if (x1 >= 0 && x1 < 16 && y1 >= 0 && y1 < 16)
            {
                Assert.That(dst.PickColor(new Position(x1, y1)), Is.EqualTo(red), $"Start ({x1}, {y1}) should be red");
            }
            if (x2 >= 0 && x2 < 16 && y2 >= 0 && y2 < 16)
            {
                Assert.That(dst.PickColor(new Position(x2, y2)), Is.EqualTo(red), $"End ({x2}, {y2}) should be red");
            }
        }

        [Test]
        public void DrawLine_WithThickPen_CalculatesAreaAndDrawsCorrectly()
        {
            byte[] pixels = new byte[16 * 16 * 4];
            for (int i = 0; i < pixels.Length; i += 4) { pixels[i + 3] = 255; }
            Picture src = Picture.Create(new PictureSize(16, 16), pixels);
            var red = new ArgbColor(255, 255, 0, 0);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 3));

            var result = drawer.DrawLine(new Position(5, 5), new Position(10, 10));
            var area = result.Area;

            // Width 3: d=1. fx=5, fy=5, tx=10, ty=10 -> from=(4, 4), size=(10-5+3, 10-5+3) = (8, 8)
            Assert.Multiple(() =>
            {
                Assert.That(area.Position.X, Is.EqualTo(4));
                Assert.That(area.Position.Y, Is.EqualTo(4));
                Assert.That(area.Size.Width, Is.EqualTo(8));
                Assert.That(area.Size.Height, Is.EqualTo(8));
            });
        }

        [Test]
        public void DrawEllipse_SinglePoint_DrawsOnePixel()
        {
            byte[] pixels = new byte[8 * 8 * 4];
            Picture src = Picture.Create(new PictureSize(8, 8), pixels);
            var red = new ArgbColor(255, 255, 0, 0);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            var result = drawer.DrawEllipse(new Position(3, 3), new Position(3, 3));
            Assert.That(result.Picture.PickColor(new Position(3, 3)), Is.EqualTo(red));
        }

        [TestCase(3, 3, 3, 6, Description = "幅0 高さ3")]
        [TestCase(3, 3, 6, 3, Description = "幅3 高さ0")]
        public void DrawEllipse_ZeroDimension_DrawsExpected(int fx, int fy, int tx, int ty)
        {
            byte[] pixels = new byte[8 * 8 * 4];
            Picture src = Picture.Create(new PictureSize(8, 8), pixels);
            var red = new ArgbColor(255, 255, 0, 0);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            var result = drawer.DrawEllipse(new Position(fx, fy), new Position(tx, ty));
            Assert.That(result.Picture.CloneImage(), Is.Not.All.Zero);
        }

        [Test]
        public void DrawFillEllipse_SinglePoint_DrawsOnePixel()
        {
            byte[] pixels = new byte[8 * 8 * 4];
            Picture src = Picture.Create(new PictureSize(8, 8), pixels);
            var red = new ArgbColor(255, 255, 0, 0);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            var result = drawer.DrawFillEllipse(new Position(3, 3), new Position(3, 3));
            Assert.That(result.Picture.PickColor(new Position(3, 3)), Is.EqualTo(red));
        }

        [TestCase(3, 3, 3, 6, Description = "幅0 高さ3")]
        [TestCase(3, 3, 6, 3, Description = "幅3 高さ0")]
        public void DrawFillEllipse_ZeroDimension_DrawsExpected(int fx, int fy, int tx, int ty)
        {
            byte[] pixels = new byte[8 * 8 * 4];
            Picture src = Picture.Create(new PictureSize(8, 8), pixels);
            var red = new ArgbColor(255, 255, 0, 0);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            var result = drawer.DrawFillEllipse(new Position(fx, fy), new Position(tx, ty));
            Assert.That(result.Picture.CloneImage(), Is.Not.All.Zero);
        }

        [Test]
        public void DrawFillEllipse_SolidDisk_FillsInteriorAndLeavesCorners()
        {
            var white = new ArgbColor(255, 255, 255, 255);
            var red = new ArgbColor(255, 255, 0, 0);
            byte[] pixels = new byte[20 * 20 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255; pixels[i + 1] = 255; pixels[i + 2] = 255; pixels[i + 3] = 255;
            }
            Picture src = Picture.Create(new PictureSize(20, 20), pixels);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            // (5, 5) から (15, 15) へ描画 (直径 10)
            var result = drawer.DrawFillEllipse(new Position(5, 5), new Position(15, 15));
            Picture dst = result.Picture;

            Assert.Multiple(() =>
            {
                // 円の中心 (10, 10) は赤
                Assert.That(dst.PickColor(new Position(10, 10)), Is.EqualTo(red), "Center should be red");
                // 上下左右端 (10, 5), (10, 15), (5, 10), (15, 10) は赤
                Assert.That(dst.PickColor(new Position(10, 5)), Is.EqualTo(red), "Top should be red");
                Assert.That(dst.PickColor(new Position(10, 15)), Is.EqualTo(red), "Bottom should be red");
                Assert.That(dst.PickColor(new Position(5, 10)), Is.EqualTo(red), "Left should be red");
                Assert.That(dst.PickColor(new Position(15, 10)), Is.EqualTo(red), "Right should be red");
                // 四隅 (5, 5), (15, 5), (5, 15), (15, 15) は白（円の外側）
                Assert.That(dst.PickColor(new Position(5, 5)), Is.EqualTo(white), "Top-left corner should be white");
                Assert.That(dst.PickColor(new Position(15, 5)), Is.EqualTo(white), "Top-right corner should be white");
                Assert.That(dst.PickColor(new Position(5, 15)), Is.EqualTo(white), "Bottom-left corner should be white");
                Assert.That(dst.PickColor(new Position(15, 15)), Is.EqualTo(white), "Bottom-right corner should be white");
            });
        }

        [Test]
        public void DrawRectangle_InvertedCoordinatesAndClipping()
        {
            var white = new ArgbColor(255, 255, 255, 255);
            var red = new ArgbColor(255, 255, 0, 0);
            byte[] pixels = new byte[10 * 10 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255; pixels[i + 1] = 255; pixels[i + 2] = 255; pixels[i + 3] = 255;
            }
            Picture src = Picture.Create(new PictureSize(10, 10), pixels);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            // 画面境界を跨ぐ逆向き矩形: (5, 15) から (-5, 2)
            var result = drawer.DrawRectangle(new Position(5, 15), new Position(-5, 2));
            Assert.That(result.Picture.CloneImage(), Is.Not.All.EqualTo(255));
        }

        [Test]
        public void DrawFillRectangle_InvertedCoordinatesAndClipping()
        {
            var white = new ArgbColor(255, 255, 255, 255);
            var red = new ArgbColor(255, 255, 0, 0);
            byte[] pixels = new byte[10 * 10 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255; pixels[i + 1] = 255; pixels[i + 2] = 255; pixels[i + 3] = 255;
            }
            Picture src = Picture.Create(new PictureSize(10, 10), pixels);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            // 画面外を含む逆向き矩形: (15, 15) から (-5, -5)
            var result = drawer.DrawFillRectangle(new Position(15, 15), new Position(-5, -5));
            Assert.That(result.Picture.PickColor(new Position(0, 0)), Is.EqualTo(red));
            Assert.That(result.Picture.PickColor(new Position(9, 9)), Is.EqualTo(red));
        }

        [Test]
        public void Fill_ObstacleInAdjacentRows_TogglesSpanCorrectly()
        {
            // 8x8 の画像。
            // 背景: 白 (255, 255, 255, 255)
            // 壁: 黒 (255, 0, 0, 0)
            //
            // row 1: [白][白][黒][黒][白][白]  <- 上の行に障害物（spanAbove: true -> false -> true 遷移）
            // row 2: [白][白][白][白][白][白]  <- ここを (0, 2) から塗りつぶす
            // row 3: [白][白][黒][黒][白][白]  <- 下の行に障害物（spanBelow: true -> false -> true 遷移）
            var white = new ArgbColor(255, 255, 255, 255);
            var black = new ArgbColor(255, 0, 0, 0);
            var red = new ArgbColor(255, 255, 0, 0);

            byte[] pixels = new byte[8 * 8 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255; pixels[i + 1] = 255; pixels[i + 2] = 255; pixels[i + 3] = 255;
            }
            void SetPixel(int x, int y, ArgbColor c)
            {
                int idx = (x * 4) + (y * 8 * 4);
                pixels[idx] = c.Blue; pixels[idx + 1] = c.Green; pixels[idx + 2] = c.Red; pixels[idx + 3] = c.Alpha;
            }

            // 壁を配置
            SetPixel(2, 1, black); SetPixel(3, 1, black);
            SetPixel(2, 3, black); SetPixel(3, 3, black);

            Picture src = Picture.Create(new PictureSize(8, 8), pixels);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            var result = drawer.Fill(new Position(0, 2));
            Picture dst = result.Picture;

            Assert.Multiple(() =>
            {
                // 壁の左右両側が赤く塗られていること（spanAbove/spanBelow のリセットが正常に機能した証拠）
                Assert.That(dst.PickColor(new Position(0, 1)), Is.EqualTo(red), "row 1 left should be red");
                Assert.That(dst.PickColor(new Position(5, 1)), Is.EqualTo(red), "row 1 right should be red");
                Assert.That(dst.PickColor(new Position(0, 3)), Is.EqualTo(red), "row 3 left should be red");
                Assert.That(dst.PickColor(new Position(5, 3)), Is.EqualTo(red), "row 3 right should be red");
                // 壁自体は黒のまま
                Assert.That(dst.PickColor(new Position(2, 1)), Is.EqualTo(black), "wall should remain black");
            });
        }

        [Test]
        public void Fill_TouchingAllCanvasBorders_FillsEntirePicture()
        {
            var white = new ArgbColor(255, 255, 255, 255);
            var red = new ArgbColor(255, 255, 0, 0);
            byte[] pixels = new byte[6 * 6 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255; pixels[i + 1] = 255; pixels[i + 2] = 255; pixels[i + 3] = 255;
            }
            Picture src = Picture.Create(new PictureSize(6, 6), pixels);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            // 最上段 y=0 から開始
            var result = drawer.Fill(new Position(0, 0));
            Picture dst = result.Picture;

            Assert.Multiple(() =>
            {
                // 4隅すべて赤
                Assert.That(dst.PickColor(new Position(0, 0)), Is.EqualTo(red));
                Assert.That(dst.PickColor(new Position(5, 0)), Is.EqualTo(red));
                Assert.That(dst.PickColor(new Position(0, 5)), Is.EqualTo(red));
                Assert.That(dst.PickColor(new Position(5, 5)), Is.EqualTo(red));
                Assert.That(result.Area.Size, Is.EqualTo(new PictureSize(6, 6)));
            });
        }

        [Test]
        public void Fill_TouchingBottomBorder_FillsCorrectly()
        {
            var white = new ArgbColor(255, 255, 255, 255);
            var red = new ArgbColor(255, 255, 0, 0);
            byte[] pixels = new byte[4 * 4 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255; pixels[i + 1] = 255; pixels[i + 2] = 255; pixels[i + 3] = 255;
            }
            Picture src = Picture.Create(new PictureSize(4, 4), pixels);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            // 最下段 y=3 から開始
            var result = drawer.Fill(new Position(2, 3));
            Assert.That(result.Picture.PickColor(new Position(0, 0)), Is.EqualTo(red));
        }

        [Test]
        public void DrawPoint_WithEvenWidthPen_CalculatesCorrectAreaAndBounds()
        {
            byte[] pixels = new byte[10 * 10 * 4];
            Picture src = Picture.Create(new PictureSize(10, 10), pixels);
            var red = new ArgbColor(255, 255, 0, 0);
            // 偶数幅 PenStyle (Width = 2): d = 1, dd = 2 - 1 = 1
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 2));

            var result = drawer.DrawPoint(new Position(5, 5));
            var area = result.Area;

            // fromPosition=(5-1, 5-1)=(4, 4), Size=(2, 2)
            Assert.Multiple(() =>
            {
                Assert.That(area.Position.X, Is.EqualTo(4));
                Assert.That(area.Position.Y, Is.EqualTo(4));
                Assert.That(area.Size.Width, Is.EqualTo(2));
                Assert.That(area.Size.Height, Is.EqualTo(2));
            });
        }

        [Test]
        public void DrawLine_WithEvenWidthPen_CalculatesCorrectAreaAndBounds()
        {
            byte[] pixels = new byte[16 * 16 * 4];
            Picture src = Picture.Create(new PictureSize(16, 16), pixels);
            var red = new ArgbColor(255, 255, 0, 0);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 2));

            var result = drawer.DrawLine(new Position(5, 5), new Position(10, 10));
            var area = result.Area;

            // fx=5, fy=5, tx=10, ty=10, Width=2, d=1
            // area from = (5 - 1, 5 - 1) = (4, 4)
            // area size = (10 - 5 + 2, 10 - 5 + 2) = (7, 7)
            Assert.Multiple(() =>
            {
                Assert.That(area.Position.X, Is.EqualTo(4));
                Assert.That(area.Position.Y, Is.EqualTo(4));
                Assert.That(area.Size.Width, Is.EqualTo(7));
                Assert.That(area.Size.Height, Is.EqualTo(7));
            });
        }

        [Test]
        public void DrawEllipse_AsymmetricDimensions_UsesMinimumDimension()
        {
            var white = new ArgbColor(255, 255, 255, 255);
            var red = new ArgbColor(255, 255, 0, 0);
            byte[] pixels = new byte[30 * 30 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255; pixels[i + 1] = 255; pixels[i + 2] = 255; pixels[i + 3] = 255;
            }
            Picture src = Picture.Create(new PictureSize(30, 30), pixels);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            // 幅 10, 高さ 20 (非正方形)
            // fx=5, fy=5, tx=15, ty=25
            // diameter は Math.Min(10, 20) = 10 でなければならない
            var result = drawer.DrawEllipse(new Position(5, 5), new Position(15, 25));
            Picture dst = result.Picture;

            // Y=25 (Math.Maxの直径なら塗られる位置) は白のままであること
            Assert.That(dst.PickColor(new Position(10, 25)), Is.EqualTo(white), "Y=25 should NOT be drawn if diameter uses Min(10, 20)");
        }

        [Test]
        public void DrawFillEllipse_AsymmetricDimensions_UsesMinimumDimension()
        {
            var white = new ArgbColor(255, 255, 255, 255);
            var red = new ArgbColor(255, 255, 0, 0);
            byte[] pixels = new byte[30 * 30 * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255; pixels[i + 1] = 255; pixels[i + 2] = 255; pixels[i + 3] = 255;
            }
            Picture src = Picture.Create(new PictureSize(30, 30), pixels);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), red, 1));

            // 幅 10, 高さ 20 (非正方形)
            var result = drawer.DrawFillEllipse(new Position(5, 5), new Position(15, 25));
            Picture dst = result.Picture;

            // Y=25 は白のままであること
            Assert.That(dst.PickColor(new Position(10, 25)), Is.EqualTo(white), "Y=25 should NOT be drawn if diameter uses Min(10, 20)");
        }

        [Test]
        public void Fill_DistinguishesBetweenDifferentColorChannels()
        {
            // BGRA layout: 各成分が異なる色
            var baseColor = new ArgbColor(255, 100, 150, 200); // R=100, G=150, B=200, A=255
            var diffBlue = new ArgbColor(255, 100, 150, 99);   // Bだけ異なる
            var diffGreen = new ArgbColor(255, 100, 99, 200);  // Gだけ異なる
            var fillTargetColor = new ArgbColor(255, 0, 255, 0); // 緑で塗りつぶす

            byte[] pixels = new byte[3 * 1 * 4];
            void SetPixel(int x, ArgbColor c)
            {
                int idx = x * 4;
                pixels[idx] = c.Blue; pixels[idx + 1] = c.Green; pixels[idx + 2] = c.Red; pixels[idx + 3] = c.Alpha;
            }
            SetPixel(0, baseColor);
            SetPixel(1, diffBlue);
            SetPixel(2, diffGreen);

            Picture src = Picture.Create(new PictureSize(3, 1), pixels);
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), fillTargetColor, 1));

            // (0, 0) から塗りつぶす
            var result = drawer.Fill(new Position(0, 0));
            Picture dst = result.Picture;

            Assert.Multiple(() =>
            {
                // (0, 0) のみ塗られ、Bだけ違う (1, 0) や Gだけ違う (2, 0) には浸食しないこと
                Assert.That(dst.PickColor(new Position(0, 0)), Is.EqualTo(fillTargetColor));
                Assert.That(dst.PickColor(new Position(1, 0)), Is.EqualTo(diffBlue));
                Assert.That(dst.PickColor(new Position(2, 0)), Is.EqualTo(diffGreen));
            });
        }

        private Picture ReadPicture(string path)
        {
            return PictureHelper.ReadBitmap(path);
        }

    }
}
