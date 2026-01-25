using System.Threading.Tasks;
using Eede.Domain.Files;

namespace Eede.Domain.ImageEditing
{
    public interface IPictureRepository
    {
        Task<Picture> LoadAsync(FilePath path);
        Task SaveAsync(Picture picture, FilePath path);
    }
}
