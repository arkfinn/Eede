using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Application.Tests.UseCase.Pictures
{
    [TestFixture]
    public class ScalingImageUseCaseTests
    {
        private Picture initialPicture;
        private DrawingSession initialSession;
        private NearestNeighborResampler resampler;

        [SetUp]
        public void SetUp()
        {
            // 2x2 Picture (Red, Blue / Green, Yellow)
            initialPicture = CreateTestPicture();
            initialSession = new DrawingSession(initialPicture);
            resampler = new NearestNeighborResampler();
        }

        private Picture CreateTestPicture()
        {
            var data = new byte[2 * 2 * 4];
            // (0,0) Red (B=0, G=0, R=255, A=255)
            SetPixel(data, 0, 0, 2, 255, 0, 0, 255);
            // (1,0) Blue (B=255, G=0, R=0, A=255)
            SetPixel(data, 1, 0, 2, 0, 0, 255, 255);
            return Picture.Create(new PictureSize(2, 2), data);
        }

        private void SetPixel(byte[] data, int x, int y, int w, byte r, byte g, byte b, byte a)
        {
            int idx = (y * w + x) * 4;
            data[idx] = b;
            data[idx + 1] = g;
            data[idx + 2] = r;
            data[idx + 3] = a;
        }

        [Test]
        public void Execute_NoSelection_Expand200Percent_TopLeft_ResizesCanvas()
        {
            // Arrange
            var useCase = new ScalingImageUseCase(resampler);
            var context = new ResizeContext(initialPicture.Size, new PictureSize(4, 4), true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Act
            var result = useCase.Execute(initialSession, context);

            // Assert
            Assert.That(result.Buffer.Fetch().Width, Is.EqualTo(4));
            Assert.That(result.Buffer.Fetch().Height, Is.EqualTo(4));
            Assert.That(result.CurrentPreviewContent, Is.Null);
            // Check (0,0) Red expanded
            AssertColor(result.CurrentPicture, 0, 0, 255, 0, 0, 255);
        }

        [Test]
        public void Execute_WithSelection_Expand200Percent_EntersPreviewState()
        {
            // Arrange
            // Select (0,0) 1x1 area (Red)
            var selection = new PictureArea(new Position(0, 0), new PictureSize(1, 1));
            var sessionWithSelection = initialSession.UpdateSelectingArea(selection);
            var useCase = new ScalingImageUseCase(resampler);
            var context = new ResizeContext(selection.Size, new PictureSize(2, 2), true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Act
            var result = useCase.Execute(sessionWithSelection, context);

            // Assert
            Assert.That(result.Buffer.Fetch().Width, Is.EqualTo(2)); // Original canvas size
            Assert.That(result.CurrentPreviewContent, Is.Not.Null);
            Assert.That(result.CurrentPreviewContent.Pixels.Width, Is.EqualTo(2));
            Assert.That(result.CurrentPreviewContent.Pixels.Height, Is.EqualTo(2));
            Assert.That(result.CurrentPreviewContent.Type, Is.EqualTo(SelectionPreviewType.CutAndMove));
        }

        private void AssertColor(Picture pic, int x, int y, byte r, byte g, byte b, byte a)
        {
            var color = pic.PickColor(new Position(x, y));
            Assert.Multiple(() =>
            {
                Assert.That(color.Red, Is.EqualTo(r), $"Red at {x},{y}");
                Assert.That(color.Green, Is.EqualTo(g), $"Green at {x},{y}");
                Assert.That(color.Blue, Is.EqualTo(b), $"Blue at {x},{y}");
                Assert.That(color.Alpha, Is.EqualTo(a), $"Alpha at {x},{y}");
            });
        }
    }
}
