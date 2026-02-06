using Eede.Domain.ImageEditing;

namespace Eede.Application.UseCase.Pictures
{
    public interface IScalingImageUseCase
    {
        DrawingSession Execute(DrawingSession session, ResizeContext context);
    }
}
