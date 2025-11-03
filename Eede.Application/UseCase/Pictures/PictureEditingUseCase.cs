using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.Pictures;
using Eede.Domain.Pictures.Actions;
using Eede.Domain.SharedKernel;

namespace Eede.Application.UseCase.Pictures;

public class PictureEditingUseCase
{
    public record EditResult(Picture Previous, Picture Updated);

    public static EditResult ExecuteAction(Picture source, PictureActions action)
    {
        Picture previous = source;
        Picture updated;

        updated = action.Execute(previous);

        return new EditResult(previous, updated);
    }

    public static EditResult ExecuteAction(Picture source, PictureActions action, PictureArea selectedRegion)
    {
        Picture previous = source;
        Picture updated;

        Picture region = previous.CutOut(selectedRegion);
        Picture updatedRegion = action.Execute(region);
        DirectImageBlender blender = new();
        updated = blender.Blend(updatedRegion, previous, selectedRegion.Position);

        return new EditResult(previous, updated);
    }

    public static EditResult PushToCanvas(Picture source, Picture from, PictureArea rect)
    {
        Picture previous = source;
        Picture updated = from.CutOut(rect);
        return new EditResult(previous, updated);
    }

    public static EditResult PullFromCanvas(Picture target, Picture source, Position position, IImageBlender blender)
    {
        Picture previous = target;
        Picture updated = target.Blend(blender, source, position);
        return new EditResult(previous, updated);
    }
}
