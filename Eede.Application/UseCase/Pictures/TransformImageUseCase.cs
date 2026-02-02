using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;

namespace Eede.Application.UseCase.Pictures;

public class TransformImageUseCase : ITransformImageUseCase
{
    public Picture Execute(Picture source, PictureActions action)
    {
        return action.Execute(source);
    }

    public Picture Execute(Picture source, PictureActions action, PictureArea selectedRegion)
    {
        Picture region = source.CutOut(selectedRegion);
        Picture updatedRegion = action.Execute(region);
        DirectImageBlender blender = new();
        return blender.Blend(updatedRegion, source, selectedRegion.Position);
    }
}
