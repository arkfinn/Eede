using Eede.Domain.ImageEditing;

namespace Eede.Application.Pictures
{
    public interface IBitmapAdapter<TBitmap>
    {
        TBitmap ConvertToBitmap(Picture picture);
        TBitmap ConvertToPremultipliedBitmap(Picture picture);
        Picture ConvertToPicture(TBitmap bitmap);
    }
}
