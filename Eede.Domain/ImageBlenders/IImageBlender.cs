using Eede.Domain.Pictures;
using Eede.Domain.Positions;

namespace Eede.Domain.ImageBlenders
{
    public interface IImageBlender
    {
        Picture Blend(Picture from, Picture to);

        Picture Blend(Picture from, Picture to, Position toPosition);
    }
}