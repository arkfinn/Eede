using Eede.Application.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Application.Tests.Pictures
{
    [TestFixture]
    public class PictureFileTests
    {
        [Test]
        public void Constructor_WithValidArguments_SetsProperties()
        {
            // Arrange
            var filePath = new FilePath("test.png");
            var picture = Picture.CreateEmpty(new PictureSize(32, 32));

            // Act
            var pictureFile = new PictureFile(filePath, picture);

            // Assert
            Assert.That(pictureFile.FilePath, Is.EqualTo(filePath));
            Assert.That(pictureFile.Picture, Is.EqualTo(picture));
        }

        [Test]
        public void Constructor_WithNullFilePath_ThrowsArgumentNullException()
        {
            // Arrange
            FilePath filePath = null;
            var picture = Picture.CreateEmpty(new PictureSize(32, 32));

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new PictureFile(filePath, picture));
            Assert.That(ex.ParamName, Is.EqualTo("filePath"));
        }

        [Test]
        public void Constructor_WithNullPicture_ThrowsArgumentNullException()
        {
            // Arrange
            var filePath = new FilePath("test.png");
            Picture picture = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new PictureFile(filePath, picture));
            Assert.That(ex.ParamName, Is.EqualTo("picture"));
        }
    }
}
