#nullable enable
using Eede.Domain.ImageEditing;

namespace Eede.Application.Pictures;

public interface IPictureCodec
{
    byte[] EncodeToPng(Picture picture);
    Picture DecodeFromPng(byte[] pngBytes);
}
