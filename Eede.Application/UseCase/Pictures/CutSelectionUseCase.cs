using Eede.Application.Infrastructure;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures;

public class CutSelectionUseCase
{
    private readonly IClipboard _clipboard;

    public CutSelectionUseCase(IClipboard clipboard)
    {
        _clipboard = clipboard;
    }

    public async Task<Picture> ExecuteAsync(Picture picture, PictureArea? area)
    {
        if (area.HasValue && area.Value.IsEmpty)
        {
            return picture;
        }
        var target = area != null ? picture.CutOut(area.Value) : picture;
        await _clipboard.CopyAsync(target);

        if (area == null)
        {
            return Picture.CreateEmpty(picture.Size);
        }
        
        return picture.Clear(area.Value);
    }
}
