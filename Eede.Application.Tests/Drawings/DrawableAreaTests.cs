using NUnit.Framework;
using Eede.Application.Drawings;
using Eede.Domain.SharedKernel;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Moq;

namespace Eede.Application.Tests.Drawings
{
    [TestFixture]
    public class DrawableAreaTests
    {
        [Test]
        public void DisplaySizeOf_ShouldReturnMagnifiedSize()
        {
            var area = new DrawableArea(new Magnification(2.0f), new PictureSize(16, 16), null);
            var picture = Picture.CreateEmpty(new PictureSize(10, 20));

            var displaySize = area.DisplaySizeOf(picture);

            Assert.That(displaySize.Width, Is.EqualTo(20));
            Assert.That(displaySize.Height, Is.EqualTo(40));
        }

        [Test]
        public void PickColor_ShouldPickColorFromMinifiedPosition()
        {
            var area = new DrawableArea(new Magnification(2.0f), new PictureSize(16, 16), null);
            var picture = Picture.CreateEmpty(new PictureSize(10, 10));
            var color = new ArgbColor(255, 255, 0, 0);

            var drawer = new Drawer(picture, new PenStyle(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), color, 1));
            var p1 = drawer.DrawPoint(new Position(2, 3));
            var newPicture = p1.Picture;

            var picked1 = area.PickColor(newPicture, new Position(4, 6));
            var picked2 = area.PickColor(newPicture, new Position(5, 7));
            var pickedOther = area.PickColor(newPicture, new Position(6, 8)); // Maps to (3, 4)

            Assert.That(picked1, Is.EqualTo(color));
            Assert.That(picked2, Is.EqualTo(color));
            Assert.That(pickedOther.Alpha, Is.EqualTo(0));
        }

        [Test]
        public void UpdateMagnification_ShouldReturnNewDrawableAreaWithUpdatedMagnification()
        {
            var area = new DrawableArea(new Magnification(1.0f), new PictureSize(16, 16), null);
            var newMagnification = new Magnification(3.0f);

            var updatedArea = area.UpdateMagnification(newMagnification);

            Assert.That(updatedArea.Magnification, Is.EqualTo(newMagnification));
            Assert.That(updatedArea, Is.Not.EqualTo(area));
        }

        [Test]
        public void DrawStart_ShouldStartDrawingWithMinifiedCoordinates()
        {
            var area = new DrawableArea(new Magnification(2.0f), new PictureSize(16, 16), null);
            var buffer = new DrawingBuffer(Picture.CreateEmpty(new PictureSize(10, 10)));

            var drawStyleMock = new Mock<IDrawStyle>();
            var penStyle = new PenStyle(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), new ArgbColor(255, 255, 255, 255), 1);

            drawStyleMock.Setup(d => d.DrawStart(It.IsAny<DrawingBuffer>(), It.IsAny<PenStyle>(), It.IsAny<CoordinateHistory>(), It.IsAny<bool>()))
                .Returns((DrawingBuffer b, PenStyle p, CoordinateHistory h, bool s) =>
                {
                    Assert.That(h.Start.X, Is.EqualTo(2)); // Display 4 minifies to 2
                    Assert.That(h.Start.Y, Is.EqualTo(3)); // Display 6 minifies to 3
                    return b.UpdateDrawing(Picture.CreateEmpty(new PictureSize(10, 10)));
                });

            var result = area.DrawStart(drawStyleMock.Object, penStyle, buffer, new Position(4, 6), false);

            drawStyleMock.Verify(d => d.DrawStart(It.IsAny<DrawingBuffer>(), It.IsAny<PenStyle>(), It.IsAny<CoordinateHistory>(), It.IsAny<bool>()), Times.Once);
            Assert.That(result.PictureBuffer, Is.Not.Null);
        }

        [Test]
        public void Move_ShouldDrawingWithMinifiedCoordinates()
        {
            var area = new DrawableArea(new Magnification(2.0f), new PictureSize(16, 16), null);
            var buffer = new DrawingBuffer(Picture.CreateEmpty(new PictureSize(10, 10)));
            var penStyle = new PenStyle(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), new ArgbColor(255, 255, 255, 255), 1);

            var drawStyleMock = new Mock<IDrawStyle>();
            drawStyleMock.Setup(d => d.DrawStart(It.IsAny<DrawingBuffer>(), It.IsAny<PenStyle>(), It.IsAny<CoordinateHistory>(), It.IsAny<bool>()))
                .Returns((DrawingBuffer b, PenStyle p, CoordinateHistory h, bool s) => b.UpdateDrawing(b.Fetch()));

            drawStyleMock.Setup(d => d.Drawing(It.IsAny<DrawingBuffer>(), It.IsAny<PenStyle>(), It.IsAny<CoordinateHistory>(), It.IsAny<bool>()))
                .Returns((DrawingBuffer b, PenStyle p, CoordinateHistory h, bool s) =>
                {
                    Assert.That(h.Now.X, Is.EqualTo(4)); // Display 8 minifies to 4
                    Assert.That(h.Now.Y, Is.EqualTo(5)); // Display 10 minifies to 5
                    return b.UpdateDrawing(b.Fetch());
                });

            // Need to DrawStart first to get into drawing state
            var startResult = area.DrawStart(drawStyleMock.Object, penStyle, buffer, new Position(4, 6), false);

            // Then Move
            var moveResult = startResult.DrawableArea.Move(drawStyleMock.Object, penStyle, startResult.PictureBuffer!, new Position(8, 10), false);

            drawStyleMock.Verify(d => d.Drawing(It.IsAny<DrawingBuffer>(), It.IsAny<PenStyle>(), It.IsAny<CoordinateHistory>(), It.IsAny<bool>()), Times.Once);
            Assert.That(moveResult.PictureBuffer, Is.Not.Null);
        }

        [Test]
        public void DrawEnd_ShouldEndDrawingWithMinifiedCoordinatesAndClearHistory()
        {
            var area = new DrawableArea(new Magnification(2.0f), new PictureSize(16, 16), null);
            var buffer = new DrawingBuffer(Picture.CreateEmpty(new PictureSize(10, 10)));
            var penStyle = new PenStyle(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), new ArgbColor(255, 255, 255, 255), 1);

            var drawStyleMock = new Mock<IDrawStyle>();
            drawStyleMock.Setup(d => d.DrawStart(It.IsAny<DrawingBuffer>(), It.IsAny<PenStyle>(), It.IsAny<CoordinateHistory>(), It.IsAny<bool>()))
                .Returns((DrawingBuffer b, PenStyle p, CoordinateHistory h, bool s) => b.UpdateDrawing(b.Fetch()));

            drawStyleMock.Setup(d => d.DrawEnd(It.IsAny<DrawingBuffer>(), It.IsAny<PenStyle>(), It.IsAny<CoordinateHistory>(), It.IsAny<bool>()))
                .Returns((DrawingBuffer b, PenStyle p, CoordinateHistory h, bool s) =>
                {
                    Assert.That(h.Now.X, Is.EqualTo(5)); // Display 10 minifies to 5
                    Assert.That(h.Now.Y, Is.EqualTo(6)); // Display 12 minifies to 6
                    return new DrawEndResult(ContextFactory.Create(b.Fetch()), new PictureRegion(new PictureArea(new Position(0, 0), new PictureSize(10, 10))));
                });

            // Need to DrawStart first to get into drawing state
            var startResult = area.DrawStart(drawStyleMock.Object, penStyle, buffer, new Position(4, 6), false);

            // Then DrawEnd
            var endResult = startResult.DrawableArea.DrawEnd(drawStyleMock.Object, penStyle, startResult.PictureBuffer!, new Position(10, 12), false);

            drawStyleMock.Verify(d => d.DrawEnd(It.IsAny<DrawingBuffer>(), It.IsAny<PenStyle>(), It.IsAny<CoordinateHistory>(), It.IsAny<bool>()), Times.Once);
            Assert.That(endResult.PictureBuffer, Is.Not.Null);
            Assert.That(endResult.PictureBuffer!.IsDrawing(), Is.False);
        }

        [Test]
        public void DrawCancel_ShouldCancelDrawingAndClearHistory()
        {
            var area = new DrawableArea(new Magnification(2.0f), new PictureSize(16, 16), null);
            var buffer = new DrawingBuffer(Picture.CreateEmpty(new PictureSize(10, 10)));
            var penStyle = new PenStyle(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), new ArgbColor(255, 255, 255, 255), 1);

            var drawStyleMock = new Mock<IDrawStyle>();
            drawStyleMock.Setup(d => d.DrawStart(It.IsAny<DrawingBuffer>(), It.IsAny<PenStyle>(), It.IsAny<CoordinateHistory>(), It.IsAny<bool>()))
                .Returns((DrawingBuffer b, PenStyle p, CoordinateHistory h, bool s) => b.UpdateDrawing(b.Fetch()));

            // Start drawing
            var startResult = area.DrawStart(drawStyleMock.Object, penStyle, buffer, new Position(4, 6), false);
            Assert.That(startResult.PictureBuffer!.IsDrawing(), Is.True);

            // Cancel drawing
            var cancelResult = startResult.DrawableArea.DrawCancel(startResult.PictureBuffer);
            Assert.That(cancelResult.PictureBuffer!.IsDrawing(), Is.False);
        }
    }
}
