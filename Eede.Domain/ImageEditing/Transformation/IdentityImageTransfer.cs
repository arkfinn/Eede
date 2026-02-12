using Eede.Domain.SharedKernel;

#nullable enable
namespace Eede.Domain.ImageEditing.Transformation;

public class IdentityImageTransfer : IImageTransfer
{
    public Picture Transfer(Picture src, Magnification magnification)
    {
        return src;
    }
}
