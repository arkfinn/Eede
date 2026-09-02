using Eede.Application.UseCase.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using System.Threading.Tasks;

namespace Eede.Application.Pictures;

public class PictureFileIO(
    ISavePictureUseCase saveUseCase,
    ILoadPictureUseCase loadUseCase) : IPictureFileIO
{
    public Task SaveAsync(Picture picture, FilePath path) => saveUseCase.ExecuteAsync(picture, path);
    public Task<Picture> LoadAsync(FilePath path) => loadUseCase.ExecuteAsync(path);
}
