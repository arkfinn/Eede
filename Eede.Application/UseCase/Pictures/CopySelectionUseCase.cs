using Eede.Application.Services;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures;

public class CopySelectionUseCase
{
    private readonly IClipboardService _clipboardService;

    public CopySelectionUseCase(IClipboardService clipboardService)
    {
        _clipboardService = clipboardService;
    }

    public async Task Execute(Picture picture, PictureArea? selectingArea)
    {
        Picture target;
        if (selectingArea.HasValue && selectingArea.Value.Width > 0 && selectingArea.Value.Height > 0)
        {
            target = picture.CutOut(selectingArea.Value);
        }
        else
        {
            target = picture;
        }

        await _clipboardService.CopyAsync(target);
    }
}
