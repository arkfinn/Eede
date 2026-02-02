using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.SharedKernel;

namespace Eede.Application.UseCase.Pictures;

public interface ITransferImageFromCanvasUseCase
{
    Picture Execute(Picture target, Picture source, Position position, IImageBlender blender);
}
