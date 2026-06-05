using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Eede.Application.Tests.Pictures;

[TestFixture]
public class SelectionServiceTests
{
    private Mock<ICopySelectionUseCase> _copyUseCaseMock;
    private Mock<ICutSelectionUseCase> _cutUseCaseMock;
    private Mock<IPasteFromClipboardUseCase> _pasteUseCaseMock;
    private SelectionService _selectionService;

    [SetUp]
    public void SetUp()
    {
        _copyUseCaseMock = new Mock<ICopySelectionUseCase>();
        _cutUseCaseMock = new Mock<ICutSelectionUseCase>();
        _pasteUseCaseMock = new Mock<IPasteFromClipboardUseCase>();

        _selectionService = new SelectionService(
            _copyUseCaseMock.Object,
            _cutUseCaseMock.Object,
            _pasteUseCaseMock.Object);
    }

    [Test]
    public async Task CopyAsync_DelegatesToCopyUseCase()
    {
        var picture = Picture.CreateEmpty(new PictureSize(10, 10));
        var area = new PictureArea(new Position(0, 0), new PictureSize(5, 5));

        await _selectionService.CopyAsync(picture, area);

        _copyUseCaseMock.Verify(x => x.ExecuteAsync(picture, area), Times.Once);
    }

    [Test]
    public async Task CutAsync_DelegatesToCutUseCase()
    {
        var picture = Picture.CreateEmpty(new PictureSize(10, 10));
        var area = new PictureArea(new Position(0, 0), new PictureSize(5, 5));
        var expectedResult = Picture.CreateEmpty(new PictureSize(5, 5));

        _cutUseCaseMock.Setup(x => x.ExecuteAsync(picture, area)).ReturnsAsync(expectedResult);

        var result = await _selectionService.CutAsync(picture, area);

        Assert.That(result, Is.SameAs(expectedResult));
        _cutUseCaseMock.Verify(x => x.ExecuteAsync(picture, area), Times.Once);
    }

    [Test]
    public async Task PasteAsync_DelegatesToPasteUseCase()
    {
        await _selectionService.PasteAsync();

        _pasteUseCaseMock.Verify(x => x.ExecuteAsync(), Times.Once);
    }
}
