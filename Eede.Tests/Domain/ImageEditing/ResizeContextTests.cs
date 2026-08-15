using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing
{
    [TestFixture]
    public class ResizeContextTests
    {
        private PictureSize originalSize;

        [SetUp]
        public void SetUp()
        {
            originalSize = new PictureSize(100, 200);
        }

        [Test]
        public void ConstructorTest()
        {
            // Arrange
            var targetSize = new PictureSize(50, 50);

            // Act
            var context = new ResizeContext(originalSize, targetSize, true, HorizontalAlignment.Center, VerticalAlignment.Center);

            // Assert
            Assert.That(context.OriginalSize, Is.EqualTo(originalSize));
            Assert.That(context.TargetSize, Is.EqualTo(targetSize));
            Assert.That(context.IsLockAspectRatio, Is.True);
            Assert.That(context.HorizontalAlignment, Is.EqualTo(HorizontalAlignment.Center));
            Assert.That(context.VerticalAlignment, Is.EqualTo(VerticalAlignment.Center));
        }

        [Test]
        public void UpdateWidthWithLockAspectRatioTest()
        {
            // Arrange
            var context = new ResizeContext(originalSize, originalSize, true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Act
            var updated = context.UpdateWidth(200);

            // Assert
            // 100:200 = 200:400
            Assert.That(updated.TargetSize.Width, Is.EqualTo(200));
            Assert.That(updated.TargetSize.Height, Is.EqualTo(400));
        }

        [Test]
        public void UpdateHeightWithLockAspectRatioTest()
        {
            // Arrange
            var context = new ResizeContext(originalSize, originalSize, true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Act
            var updated = context.UpdateHeight(100);

            // Assert
            // 100:200 = 50:100
            Assert.That(updated.TargetSize.Width, Is.EqualTo(50));
            Assert.That(updated.TargetSize.Height, Is.EqualTo(100));
        }

        [Test]
        public void UpdateWidthPercentTest()
        {
            // Arrange
            var context = new ResizeContext(originalSize, originalSize, true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Act
            var updated = context.UpdateWidthPercent(50);

            // Assert
            Assert.That(updated.TargetSize.Width, Is.EqualTo(50));
            Assert.That(updated.TargetSize.Height, Is.EqualTo(100));
        }

        [Test]
        public void UpdateHeightPercentTest()
        {
            // Arrange
            var context = new ResizeContext(originalSize, originalSize, true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Act
            var updated = context.UpdateHeightPercent(200);

            // Assert
            Assert.That(updated.TargetSize.Width, Is.EqualTo(200));
            Assert.That(updated.TargetSize.Height, Is.EqualTo(400));
        }

        [Test]
        public void GetWidthPercentTest()
        {
            // Arrange
            var context = new ResizeContext(originalSize, new PictureSize(50, 200), true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Act
            var percent = context.GetWidthPercent();

            // Assert
            Assert.That(percent, Is.EqualTo(50.0));
        }

        [Test]
        public void GetWidthPercent_WithZeroOriginalWidth_Returns100()
        {
            // Arrange
            var zeroOriginalSize = new PictureSize(0, 200);
            var context = new ResizeContext(zeroOriginalSize, new PictureSize(50, 200), true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Act
            var percent = context.GetWidthPercent();

            // Assert
            Assert.That(percent, Is.EqualTo(100.0));
        }

        [Test]
        public void GetHeightPercentTest()
        {
            // Arrange
            var context = new ResizeContext(originalSize, new PictureSize(100, 400), true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Act
            var percent = context.GetHeightPercent();

            // Assert
            Assert.That(percent, Is.EqualTo(200.0));
        }

        [Test]
        public void GetHeightPercent_WithZeroOriginalHeight_Returns100()
        {
            // Arrange
            var zeroOriginalSize = new PictureSize(100, 0);
            var context = new ResizeContext(zeroOriginalSize, new PictureSize(100, 400), true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Act
            var percent = context.GetHeightPercent();

            // Assert
            Assert.That(percent, Is.EqualTo(100.0));
        }

        [Test]
        public void CalculateOffsetTest()
        {
            // Arrange
            // 100x200 -> 200x400
            // Center, Center なら (-50, -100)
            var targetSize = new PictureSize(200, 400);
            var context = new ResizeContext(originalSize, targetSize, true, HorizontalAlignment.Center, VerticalAlignment.Center);

            // Act
            var offset = context.CalculateOffset();

            // Assert
            Assert.That(offset.X, Is.EqualTo(-50));
            Assert.That(offset.Y, Is.EqualTo(-100));
        }
    }
}
