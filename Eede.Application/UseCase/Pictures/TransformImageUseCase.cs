using Eede.Domain.ImageEditing.Filters;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;

namespace Eede.Application.UseCase.Pictures;

public class TransformImageUseCase : ITransformImageUseCase
{
    public Picture Execute(Picture source, PictureActions action, AntiAliasMode mode = AntiAliasMode.Argb)
    {
        return action.Execute(source, mode);
    }

    public Picture Execute(Picture source, PictureActions action, PictureArea selectedRegion, AntiAliasMode mode = AntiAliasMode.Argb)
    {
        Picture region = source.CutOut(selectedRegion);
        Picture updatedRegion = action.Execute(region, mode);
        DirectImageBlender blender = new();
        return blender.Blend(updatedRegion, source, selectedRegion.Position);
    }
}
