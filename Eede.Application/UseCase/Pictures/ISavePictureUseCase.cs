using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures;

public interface ISavePictureUseCase
{
    Task ExecuteAsync(Picture picture, FilePath path);
}
