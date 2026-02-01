using Eede.Application.Infrastructure;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures;

public class CopySelectionUseCase
{
    private readonly IClipboard _clipboard;

    public CopySelectionUseCase(IClipboard clipboard)
    {
        _clipboard = clipboard;
    }

    public async Task ExecuteAsync(Picture picture, PictureArea? area)
    {
        if (area.HasValue && area.Value.IsEmpty)
        {
            return;
        }
        var target = area != null ? picture.CutOut(area.Value) : picture;
        await _clipboard.CopyAsync(target);
    }
}
