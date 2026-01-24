using Eede.Domain.ImageEditing;
using System.Threading.Tasks;

namespace Eede.Application.Services;

public interface IClipboardService
{
    Task CopyAsync(Picture picture);
    Task<Picture> GetPictureAsync();
    Task<bool> HasPictureAsync();
}
