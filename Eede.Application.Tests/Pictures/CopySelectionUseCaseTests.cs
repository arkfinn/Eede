using Eede.Application.Infrastructure;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Eede.Application.Tests.Pictures;

[TestFixture]
public class CopySelectionUseCaseTests
{
    private Mock<IClipboard> _clipboardServiceMock;
    private CopySelectionUseCase _useCase;

    [SetUp]
    public void SetUp()
    {
        _clipboardServiceMock = new Mock<IClipboard>();
        _useCase = new CopySelectionUseCase(_clipboardServiceMock.Object);
    }

    [Test]
    public async Task Execute_WithSelection_ShouldCopySelectedArea()
    {
        var picture = Picture.CreateEmpty(new PictureSize(32, 32));
        var area = new PictureArea(new Position(10, 10), new PictureSize(5, 5));

        await _useCase.ExecuteAsync(picture, area);

        _clipboardServiceMock.Verify(x => x.CopyAsync(It.Is<Picture>(p => p.Size.Width == 5 && p.Size.Height == 5)), Times.Once);
    }

    [Test]
    public async Task Execute_WithoutSelection_ShouldCopyEntirePicture()
    {
        var picture = Picture.CreateEmpty(new PictureSize(32, 32));

        await _useCase.ExecuteAsync(picture, null);

        _clipboardServiceMock.Verify(x => x.CopyAsync(It.Is<Picture>(p => p.Size.Width == 32 && p.Size.Height == 32)), Times.Once);
    }
}
