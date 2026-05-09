using Eede.Domain.ImageEditing.Filters;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;

namespace Eede.Application.UseCase.Pictures;

public interface ITransformImageUseCase
{
    Picture Execute(Picture source, PictureActions action, AntiAliasMode mode = AntiAliasMode.Argb);
    Picture Execute(Picture source, PictureActions action, PictureArea selectedRegion, AntiAliasMode mode = AntiAliasMode.Argb);
}
