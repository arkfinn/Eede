using Eede.Application.Infrastructure;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Eede.Application.Tests.Pictures;

[TestFixture]
public class CutSelectionUseCaseTests
{
    private Mock<IClipboard> _clipboardServiceMock;
    private CutSelectionUseCase _useCase;

    [SetUp]
    public void Setup()
    {
        _clipboardServiceMock = new Mock<IClipboard>();
        _useCase = new CutSelectionUseCase(_clipboardServiceMock.Object);
    }

    [Test]
    public async Task ExecuteTest()
    {
        var picture = Picture.CreateEmpty(new PictureSize(10, 10));
        var area = new PictureArea(new Position(0, 0), new PictureSize(5, 5));
        
        // Setup mock to verify CopyAsync is called
        _clipboardServiceMock.Setup(x => x.CopyAsync(It.IsAny<Picture>())).Returns(Task.CompletedTask);

        var result = await _useCase.ExecuteAsync(picture, area);

        Assert.That(result, Is.Not.Null);
        // Verify that CopyAsync was called once
        _clipboardServiceMock.Verify(x => x.CopyAsync(It.IsAny<Picture>()), Times.Once);
    }
}
