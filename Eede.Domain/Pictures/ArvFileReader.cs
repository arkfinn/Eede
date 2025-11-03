using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using System;
using System.IO;

namespace Eede.Domain.Pictures
{
    public class ArvFileReader
    {
        private const int ArvColorPlanes = 4; // ARVファイルのカラープレーン数

        public Picture Read(Stream fileStream)
        {
            using BinaryReader reader = new(fileStream);
            return ReadPictureData(reader);
        }

        private Picture ReadPictureData(BinaryReader reader)
        {
            (int width, int height, byte[] arvHeaderFlags) = ReadHeader(reader);
            ArgbColor[] palette = ReadPalette(reader, arvHeaderFlags, ArvColorPlanes);
            return ReadBody(reader, width, height, palette, arvHeaderFlags, ArvColorPlanes);
        }

        private (int width, int height, byte[] arvHeaderFlags) ReadHeader(BinaryReader reader)
        {
            byte[] arvHeaderFlags = ReadHeaderFlags(reader);
            (int width, int height) = ReadDimensions(reader);
            SkipBytes(reader, 12);
            return (width, height, arvHeaderFlags);
        }

        private byte[] ReadHeaderFlags(BinaryReader reader)
        {
            _ = reader.ReadBytes(16);
            byte[] arvHeaderFlags = reader.ReadBytes(6);
            SkipBytes(reader, 2);
            return arvHeaderFlags;
        }

        private (int width, int height) ReadDimensions(BinaryReader reader)
        {
            int width = reader.ReadUInt16();
            int height = reader.ReadUInt16();
            return (width, height);
        }

        private void SkipBytes(BinaryReader reader, int count)
        {
            _ = reader.ReadBytes(count);
        }

        private ArgbColor[] ReadPalette(BinaryReader reader, byte[] arvHeaderFlags, int arvColorPlanes)
        {
            ReadImageData(reader, arvHeaderFlags);
            ValidatePaletteFlag(arvHeaderFlags);
            byte[] paletteBytes = ReadPaletteBytes(reader);
            return CreatePalette(paletteBytes, arvColorPlanes);
        }

        private void ReadImageData(BinaryReader reader, byte[] arvHeaderFlags)
        {
            if (arvHeaderFlags[0] == 'I')
            {
                int imageDataLength = reader.ReadUInt16();
                ThrowIfInvalidImageDataLength(imageDataLength);
                SkipBytes(reader, imageDataLength - 2);
            }
        }

        private void ThrowIfInvalidImageDataLength(int imageDataLength)
        {
            if (imageDataLength < 2)
            {
                throw new InvalidDataException("Invalid image data length.");
            }
        }

        private void ValidatePaletteFlag(byte[] arvHeaderFlags)
        {
            if (arvHeaderFlags[1] != 'R')
            {
                throw new InvalidDataException("Invalid flag[1] value (expected 'R' or 82).");
            }
        }

        private byte[] ReadPaletteBytes(BinaryReader reader)
        {
            byte paletteLength = reader.ReadByte();
            _ = reader.ReadByte(); // Skip a byte
            return reader.ReadBytes(paletteLength - 2);
        }

        private ArgbColor[] CreatePalette(byte[] paletteBytes, int arvColorPlanes)
        {
            ArgbColor[] palette = new ArgbColor[16];
            for (int colorIndex = 0; colorIndex < (1 << arvColorPlanes); colorIndex++)
            {
                palette[colorIndex] = GetColorFromPaletteBytes(paletteBytes, colorIndex);
            }
            return palette;
        }

        private ArgbColor GetColorFromPaletteBytes(byte[] paletteBytes, int colorIndex)
        {
            int paletteByteOffset = colorIndex * 6;
            ThrowIfPaletteDataIncomplete(paletteBytes, paletteByteOffset);

            byte redByte = paletteBytes[paletteByteOffset];
            byte greenByte = paletteBytes[paletteByteOffset + 2];
            byte blueByte = paletteBytes[paletteByteOffset + 4];

            return new ArgbColor(255,
                (byte)(redByte * 17),
                (byte)(greenByte * 17),
                (byte)(blueByte * 17));
        }

        private void ThrowIfPaletteDataIncomplete(byte[] paletteBytes, int offset)
        {
            if (offset + 5 >= paletteBytes.Length)
            {
                throw new InvalidDataException("Palette data is incomplete.");
            }
        }

        private Picture ReadBody(BinaryReader reader, int width, int height, ArgbColor[] palette, byte[] flag, int arvColorPlanes)
        {
            int vramPlaneSize = width * height / 8;
            byte[][] planesData = DecodePlanes(reader, arvColorPlanes, vramPlaneSize);
            byte[] frameBuffer = CreateFrameBuffer(width, height, palette, planesData, arvColorPlanes);
            return Picture.Create(new PictureSize(width, height), frameBuffer);
        }

        private byte[][] DecodePlanes(BinaryReader reader, int arvColorPlanes, int vramPlaneSize)
        {
            byte[][] planesData = new byte[arvColorPlanes][];
            RleDecoder decoder = new(reader);
            for (int planeIndex = 0; planeIndex < arvColorPlanes; planeIndex++)
            {
                planesData[planeIndex] = new byte[vramPlaneSize];
                decoder.DecodePlane(planesData[planeIndex], vramPlaneSize);
            }
            return planesData;
        }

        private byte[] CreateFrameBuffer(int width, int height, ArgbColor[] palette, byte[][] planesData, int arvColorPlanes)
        {
            byte[] frameBuffer = new byte[width * height * 4];
            int bytesPerRow = width / 8;

            for (int y = 0; y < height; y++)
            {
                FillRowPixels(frameBuffer, y, width, bytesPerRow, palette, planesData, arvColorPlanes);
            }
            return frameBuffer;
        }

        private void FillRowPixels(byte[] frameBuffer, int y, int width, int bytesPerRow, ArgbColor[] palette, byte[][] planesData, int arvColorPlanes)
        {
            for (int byteColumn = 0; byteColumn < bytesPerRow; byteColumn++)
            {
                FillColumnPixels(frameBuffer, y, width, byteColumn, bytesPerRow, palette, planesData, arvColorPlanes);
            }
        }

        private void FillColumnPixels(byte[] frameBuffer, int y, int width, int byteColumn, int bytesPerRow, ArgbColor[] palette, byte[][] planesData, int arvColorPlanes)
        {
            int planeDataOffset = (y * bytesPerRow) + byteColumn;
            for (int bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                int colorIndex = GetColorIndex(planesData, planeDataOffset, bitIndex, arvColorPlanes);
                SetPixelInFrameBuffer(frameBuffer, y, width, byteColumn, bitIndex, palette[colorIndex]);
            }
        }

        private int GetColorIndex(byte[][] planesData, int planeDataOffset, int bitIndex, int arvColorPlanes)
        {
            int colorIndex = 0;
            for (int plane = 0; plane < arvColorPlanes; plane++)
            {
                colorIndex |= ((planesData[plane][planeDataOffset] >> (7 - bitIndex)) & 1) << plane;
            }
            return colorIndex;
        }

        private void SetPixelInFrameBuffer(byte[] frameBuffer, int y, int width, int byteColumn, int bitIndex, ArgbColor color)
        {
            int outputPixelIndex = (y * width) + (byteColumn << 3) + bitIndex;
            int bufferOffset = outputPixelIndex * 4;

            ThrowIfFrameBufferOverflow(frameBuffer, bufferOffset);

            frameBuffer[bufferOffset + 0] = color.Blue;
            frameBuffer[bufferOffset + 1] = color.Green;
            frameBuffer[bufferOffset + 2] = color.Red;
            frameBuffer[bufferOffset + 3] = 0xFF;
        }

        private void ThrowIfFrameBufferOverflow(byte[] frameBuffer, int offset)
        {
            if (offset + 3 >= frameBuffer.Length)
            {
                throw new IndexOutOfRangeException("FrameBufferの書き込み範囲を超えました。");
            }
        }
    }
}
