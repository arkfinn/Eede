using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System.Drawing;

namespace Eede.Domain.ImageBlenders
{
    public interface IImageBlender
    {
        PictureData Blend(PictureData from, PictureData to);

        PictureData Blend(PictureData from, PictureData to, Position toPosition);
    }
}