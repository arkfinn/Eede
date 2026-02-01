using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures;

public interface ICopySelectionUseCase
{
    Task ExecuteAsync(Picture picture, PictureArea? area);
}
