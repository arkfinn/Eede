using System.Drawing;

namespace Eede.Domain.ImageTransfers
{
    public interface IImageTransfer
    {
        void Transfer(Bitmap from, Graphics to, Size size);
    }
}