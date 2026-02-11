#nullable enable
using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing
{
    public interface IImageResampler
    {
        Picture Resize(Picture source, PictureSize newSize);
    }
}
