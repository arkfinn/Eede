using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing
{
    [TestFixture]
    public class DraggingStateTests
    {
        [Test]
        public void GetSelectionPreviewInfo_ShouldReturnCorrectInfo()
        {
            var size = new PictureSize(32, 32);
            var red = new ArgbColor(255, 255, 0, 0);
            var pixels = Picture.CreateEmpty(new PictureSize(10, 10)).Blend(new DirectImageBlender(), Picture.Create(new PictureSize(10, 10), new byte[10 * 10 * 4]), new Position(0, 0)); // Dummy pixels
            var originalArea = new PictureArea(new Position(0, 0), new PictureSize(10, 10));
            var startPos = new Position(5, 5);
            var currentPos = new Position(15, 15);

            var state = new DraggingState(pixels, originalArea, startPos);

            // Simulate move
            state.HandlePointerMoved(HalfBoxArea.Create(startPos, new PictureSize(16, 16)), true, currentPos, false, size);

            var info = state.GetSelectionPreviewInfo();

            Assert.That(info, Is.Not.Null);
            Assert.That(info.Position, Is.EqualTo(new Position(10, 10))); // (0,0) + (15-5)
            Assert.That(info.Type, Is.EqualTo(SelectionPreviewType.CutAndMove));
            Assert.That(info.OriginalArea.HasValue, Is.True);
            Assert.That(info.OriginalArea.Value, Is.EqualTo(originalArea));
        }

        [Test]
        public void ApplyPreview_ShouldClearOriginalPosition()
        {
            // Setup base picture (all red)
            var size = new PictureSize(32, 32);
            var red = new ArgbColor(255, 255, 0, 0);
            var basePicture = CreateFilledPicture(size, red);

            // Preview info: Clear (0,0)-(10,10), Place at (10,10)
            var pixels = Picture.CreateEmpty(new PictureSize(10, 10)); // Transparent pixels to paste

            var originalArea = new PictureArea(new Position(0, 0), new PictureSize(10, 10));
            var previewPos = new Position(10, 10);
            var info = new SelectionPreviewInfo(pixels, previewPos, SelectionPreviewType.CutAndMove, originalArea);

            // Apply logic similar to InteractionCoordinator.Painted
            var picture = basePicture;
            if (info.OriginalArea.HasValue)
            {
                picture = picture.Clear(info.OriginalArea.Value);
            }
            picture = picture.Blend(new DirectImageBlender(), info.Pixels, info.Position);

            // Assert
            // (0,0) should be cleared (Alpha 0)
            Assert.That(picture.PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0), "Original area should be cleared");

            // (10,10) should be transparent (since pixels is empty) but base was red.
            // DirectImageBlender overwrites, so it should be Alpha 0.
            Assert.That(picture.PickColor(new Position(10, 10)).Alpha, Is.EqualTo(0), "Moved area should be overwritten with preview pixels");
        }

        private Picture CreateFilledPicture(PictureSize size, ArgbColor color)
        {
            byte[] data = new byte[size.Width * size.Height * 4];
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
}