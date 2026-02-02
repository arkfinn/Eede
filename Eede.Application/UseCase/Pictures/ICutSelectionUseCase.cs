using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures;

public interface ICutSelectionUseCase
{
    Task<Picture> ExecuteAsync(Picture picture, PictureArea? area);
}
