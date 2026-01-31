using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Eede.Application.Tests.Pictures;

[TestFixture]
public class PasteFromClipboardUseCaseTests
{
    private Mock<IClipboard> _clipboardServiceMock;
    private DrawingSessionProvider _sessionProvider;
    private PasteFromClipboardUseCase _useCase;

    [SetUp]
    public void SetUp()
    {
        _clipboardServiceMock = new Mock<IClipboard>();
        _sessionProvider = new DrawingSessionProvider();
        _useCase = new PasteFromClipboardUseCase(_clipboardServiceMock.Object, _sessionProvider);
    }

    [Test]
    public async Task Execute_ShouldUpdateSessionWithPictureFromClipboard()
    {
        var expectedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
        _clipboardServiceMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(expectedPicture);

        await _useCase.ExecuteAsync();

        Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent, Is.Not.Null);
        Assert.That(_sessionProvider.CurrentSession.CurrentPreviewContent.Pixels, Is.EqualTo(expectedPicture));
    }
}
