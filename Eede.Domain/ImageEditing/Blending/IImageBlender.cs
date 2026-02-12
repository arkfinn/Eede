#nullable enable
using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.Blending;

public interface IImageBlender
{
    Picture Blend(Picture from, Picture to);

    Picture Blend(Picture from, Picture to, Position toPosition);
}