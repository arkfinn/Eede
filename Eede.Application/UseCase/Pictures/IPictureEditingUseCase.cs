using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;

namespace Eede.Application.UseCase.Pictures;

public interface IPictureEditingUseCase
{
    PictureEditingUseCase.EditResult ExecuteAction(Picture source, PictureActions action);
    PictureEditingUseCase.EditResult ExecuteAction(Picture source, PictureActions action, PictureArea selectedRegion);
    PictureEditingUseCase.EditResult PushToCanvas(Picture source, Picture from, PictureArea rect);
    PictureEditingUseCase.EditResult PullFromCanvas(Picture target, Picture source, Position position, IImageBlender blender);
}
