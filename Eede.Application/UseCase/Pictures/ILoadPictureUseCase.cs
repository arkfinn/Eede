using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures;

public interface ILoadPictureUseCase
{
    Task<Picture> ExecuteAsync(FilePath path);
}
