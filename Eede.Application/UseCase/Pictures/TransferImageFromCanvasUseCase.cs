using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.SharedKernel;

namespace Eede.Application.UseCase.Pictures;

public class TransferImageFromCanvasUseCase : ITransferImageFromCanvasUseCase
{
    public Picture Execute(Picture target, Picture source, Position position, IImageBlender blender)
    {
        return target.Blend(blender, source, position);
    }
}
