using System.Drawing;

namespace Eede.ImageBlenders
{
    public interface IImageBlender
    {
        void Blend(Bitmap from, Bitmap to);
    }
}