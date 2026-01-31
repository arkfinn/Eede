using Eede.Application.Infrastructure;
using Eede.Domain.ImageEditing;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures;

public class PasteFromClipboardUseCase
{
    private readonly IClipboard _clipboard;

    public PasteFromClipboardUseCase(IClipboard clipboard)
    {
        _clipboard = clipboard;
    }

    public async Task<Picture> ExecuteAsync()
    {
        return await _clipboard.GetPictureAsync();
    }
}
