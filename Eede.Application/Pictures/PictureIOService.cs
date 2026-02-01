using Eede.Application.UseCase.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using System.Threading.Tasks;

namespace Eede.Application.Pictures;

public interface IPictureIOService
{
    Task SaveAsync(Picture picture, FilePath path);
    Task<Picture> LoadAsync(FilePath path);
}

public class PictureIOService(
    ISavePictureUseCase saveUseCase,
    ILoadPictureUseCase loadUseCase) : IPictureIOService
{
    public Task SaveAsync(Picture picture, FilePath path) => saveUseCase.ExecuteAsync(picture, path);
    public Task<Picture> LoadAsync(FilePath path) => loadUseCase.ExecuteAsync(path);
}
