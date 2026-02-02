using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;

namespace Eede.Application.UseCase.Pictures;

public interface ITransferImageToCanvasUseCase
{
    Picture Execute(Picture source, PictureArea rect);
}
