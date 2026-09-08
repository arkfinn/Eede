using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Pictures;
using System.IO;

namespace Eede.Domain.Tests.Helpers
{
    internal static class PictureHelper
    {
        private static readonly SkiaSharpPictureCodec Codec = new();

        public static Picture ReadBitmap(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            return Codec.DecodeFromPng(bytes);
        }

        public static void WriteBitmap(string path, Picture picture)
        {
            byte[] bytes = Codec.EncodeToPng(picture);
            File.WriteAllBytes(path, bytes);
        }
    }
}

