#nullable enable
using Eede.Domain.ImageEditing;

namespace Eede.Application.Pictures;

public interface IPictureCodec
{
    /// <summary>
    /// Encodes a picture to PNG format. Implementations must be thread-safe and reentrant
    /// as this method may be invoked concurrently by SessionRecoveryCoordinator.
    /// </summary>
    byte[] EncodeToPng(Picture picture);
    Picture DecodeFromPng(byte[] pngBytes);
}
