using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Threading.Tasks;

namespace Eede.Application.Pictures;

public interface ISelectionClipboard
{
    Task CopyAsync(Picture picture, PictureArea? area);
    Task<Picture> CutAsync(Picture picture, PictureArea? area);
    Task PasteAsync();
}
