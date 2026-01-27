using Eede.Application.Services;
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
    private Mock<IClipboardService> _clipboardServiceMock;
    private CutSelectionUseCase _useCase;

    [SetUp]
    public void SetUp()
    {
        _clipboardServiceMock = new Mock<IClipboardService>();
        _useCase = new CutSelectionUseCase(_clipboardServiceMock.Object);
    }

    [Test]
    public async Task Execute_WithSelection_ShouldCopyAndClearArea()
    {
        var picture = Picture.CreateEmpty(new PictureSize(32, 32));
        var area = new PictureArea(new Position(10, 10), new PictureSize(5, 5));

        var result = await _useCase.Execute(picture, area);

        _clipboardServiceMock.Verify(x => x.CopyAsync(It.Is<Picture>(p => p.Size.Width == 5 && p.Size.Height == 5)), Times.Once);
        Assert.That(result, Is.Not.Null);
        // 画像の中身が変わっているはず（元の画像とは別インスタンス）
        Assert.That(result, Is.Not.EqualTo(picture));
    }
}
