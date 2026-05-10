#nullable enable
using Eede.Domain.SharedKernel;
using System.Collections.Generic;

namespace Eede.Domain.ImageEditing.Blending;

public interface IImageBlender
{
    Picture Blend(Picture from, Picture to);

    Picture Blend(Picture from, Picture to, Position toPosition);

    Picture Blend(IEnumerable<(Picture Picture, Position Position)> sources, Picture destination);
}