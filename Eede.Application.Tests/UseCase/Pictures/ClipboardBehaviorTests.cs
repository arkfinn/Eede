using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Eede.Application.Tests.UseCase.Pictures
{
    [TestFixture]
    public class ClipboardBehaviorTests
    {
        private Mock<IClipboard> _clipboardMock;
        private Mock<IDrawingSessionProvider> _sessionProviderMock;
        private DrawingSession _currentSession;

        [SetUp]
        public void SetUp()
        {
            _clipboardMock = new Mock<IClipboard>();
            _sessionProviderMock = new Mock<IDrawingSessionProvider>();

            var picture = Picture.CreateEmpty(new PictureSize(10, 10));
            _currentSession = new DrawingSession(picture);
            _sessionProviderMock.Setup(p => p.CurrentSession).Returns(_currentSession);
        }

        [Test]
        public async Task CopySelectionBehaviorTest()
        {
            var useCase = new CopySelectionUseCase(_clipboardMock.Object);
            var picture = Picture.CreateEmpty(new PictureSize(10, 10));
            var area = new PictureArea(new Position(1, 1), new PictureSize(2, 2));

            await useCase.ExecuteAsync(picture, area);

            _clipboardMock.Verify(c => c.CopyAsync(It.Is<Picture>(p => p.Size.Width == 2 && p.Size.Height == 2)), Times.Once);
        }

        [Test]
        public async Task PasteFromClipboardBehaviorTest()
        {
            var pastePicture = Picture.CreateEmpty(new PictureSize(5, 5));
            _clipboardMock.Setup(c => c.GetPictureAsync()).ReturnsAsync(pastePicture);

            var useCase = new PasteFromClipboardUseCase(_clipboardMock.Object, _sessionProviderMock.Object);

            await useCase.ExecuteAsync();

            _sessionProviderMock.Verify(p => p.Update(It.Is<DrawingSession>(s =>
                s.CurrentPreviewContent != null &&
                s.CurrentPreviewContent.Pixels == pastePicture)), Times.Once);
        }
    }
}
