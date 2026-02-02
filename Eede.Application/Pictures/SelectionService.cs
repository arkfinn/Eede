using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Threading.Tasks;

namespace Eede.Application.Pictures;

public interface ISelectionService
{
    Task CopyAsync(Picture picture, PictureArea? area);
    Task<Picture> CutAsync(Picture picture, PictureArea? area);
    Task PasteAsync();
}

public class SelectionService(
    ICopySelectionUseCase copyUseCase,
    ICutSelectionUseCase cutUseCase,
    IPasteFromClipboardUseCase pasteUseCase) : ISelectionService
{
    public Task CopyAsync(Picture picture, PictureArea? area) => copyUseCase.ExecuteAsync(picture, area);
    public Task<Picture> CutAsync(Picture picture, PictureArea? area) => cutUseCase.ExecuteAsync(picture, area);
    public Task PasteAsync() => pasteUseCase.ExecuteAsync();
}
