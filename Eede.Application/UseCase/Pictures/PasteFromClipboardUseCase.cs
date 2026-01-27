using Eede.Application.Services;
using Eede.Domain.ImageEditing;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures;

public class PasteFromClipboardUseCase
{
    private readonly IClipboardService _clipboardService;

    public PasteFromClipboardUseCase(IClipboardService clipboardService)
    {
        _clipboardService = clipboardService;
    }

    public async Task<Picture?> Execute()
    {
        return await _clipboardService.GetPictureAsync();
    }
}
