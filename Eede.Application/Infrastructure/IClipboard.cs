using Eede.Domain.ImageEditing;
using System.Threading.Tasks;

namespace Eede.Application.Infrastructure;

public interface IClipboard
{
    Task CopyAsync(Picture picture);
    Task<Picture?> GetPictureAsync();
    Task<bool> HasPictureAsync();
}
