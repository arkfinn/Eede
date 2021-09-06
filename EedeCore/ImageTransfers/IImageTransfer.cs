using System.Drawing;

namespace Eede.ImageTransfers
{
    public interface IImageTransfer
    {
        void Transfer(Bitmap from, Graphics to, Size size);
    }
}