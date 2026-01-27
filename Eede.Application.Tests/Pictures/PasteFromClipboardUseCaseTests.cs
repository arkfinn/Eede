using Eede.Application.Services;
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
    private Mock<IClipboardService> _clipboardServiceMock;
    private PasteFromClipboardUseCase _useCase;

    [SetUp]
    public void SetUp()
    {
        _clipboardServiceMock = new Mock<IClipboardService>();
        _useCase = new PasteFromClipboardUseCase(_clipboardServiceMock.Object);
    }

    [Test]
    public async Task Execute_ShouldReturnPictureFromClipboard()
    {
        var expectedPicture = Picture.CreateEmpty(new PictureSize(10, 10));
        _clipboardServiceMock.Setup(x => x.GetPictureAsync()).ReturnsAsync(expectedPicture);

        var result = await _useCase.Execute();

        Assert.That(result, Is.EqualTo(expectedPicture));
    }
}
