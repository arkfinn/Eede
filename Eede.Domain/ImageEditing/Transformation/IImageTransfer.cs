namespace Eede.Domain.ImageEditing.Transformation;

public interface IImageTransfer
{
    Picture Transfer(Picture from, Magnification magnification);
}