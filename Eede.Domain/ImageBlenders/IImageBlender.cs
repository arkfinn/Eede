using Eede.Domain.Positions;
using System.Drawing;

namespace Eede.Domain.ImageBlenders
{
    public interface IImageBlender
    {
        void Blend(Bitmap from, Bitmap to);

        void Blend(Bitmap from, Bitmap to, Position toPosition);
    }
}