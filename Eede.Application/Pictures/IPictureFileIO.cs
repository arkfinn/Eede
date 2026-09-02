using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using System.Threading.Tasks;

namespace Eede.Application.Pictures;

public interface IPictureFileIO
{
    Task SaveAsync(Picture picture, FilePath path);
    Task<Picture> LoadAsync(FilePath path);
}
