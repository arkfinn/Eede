using Eede.Application.Services;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures;

public class CutSelectionUseCase
{
    private readonly IClipboardService _clipboardService;

    public CutSelectionUseCase(IClipboardService clipboardService)
    {
        _clipboardService = clipboardService;
    }

    public async Task<Picture> Execute(Picture picture, PictureArea? selectingArea)
    {
        Picture target;
        Picture cleared;

        if (selectingArea.HasValue && selectingArea.Value.Width > 0 && selectingArea.Value.Height > 0)
        {
            target = picture.CutOut(selectingArea.Value);
            cleared = picture.Clear(selectingArea.Value);
        }
        else
        {
            target = picture;
            cleared = Picture.CreateEmpty(picture.Size);
        }

        await _clipboardService.CopyAsync(target);
        return cleared;
    }
}
