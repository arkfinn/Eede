using System;
using System.IO;

namespace Eede.Infrastructure.ImageEditing.Persistence.ArvFileFormat
{
    public class RleDecoder
    {
        private readonly BinaryReader _reader;
        private int _lastDecodedByte = -1;

        public RleDecoder(BinaryReader reader)
        {
            _reader = reader;
        }

        public void DecodePlane(byte[] planeData, int vramPlaneSize)
        {
            int currentPlaneAddress = 0;
            while (currentPlaneAddress < vramPlaneSize)
            {
                currentPlaneAddress = DecodeNextByte(planeData, currentPlaneAddress, vramPlaneSize);
            }
        }

        private int DecodeNextByte(byte[] planeData, int currentPlaneAddress, int vramPlaneSize)
        {
            ThrowIfEndOfStream();
            byte currentByteValue = _reader.ReadByte();
            (byte repeatCount, int newLastDecodedByte) = GetRleInfo(currentByteValue);
            _lastDecodedByte = newLastDecodedByte;
            return FillPlaneData(planeData, currentPlaneAddress, currentByteValue, repeatCount, vramPlaneSize);
        }

        private void ThrowIfEndOfStream()
        {
            if (_reader.BaseStream.Position >= _reader.BaseStream.Length)
            {
                throw new InvalidDataException("Unexpected end of content while reading plane data.");
            }
        }

        private (byte repeatCount, int newLastDecodedByte) GetRleInfo(byte currentByteValue)
        {
            if (currentByteValue != _lastDecodedByte)
            {
                return (1, currentByteValue);
            }
            return (ReadRepeatCount(), -1);
        }

        private byte ReadRepeatCount()
        {
            byte repeatCount = _reader.ReadByte();
            return repeatCount == 0 ? (byte)255 : (byte)(repeatCount - 1);
        }

        private int FillPlaneData(byte[] planeData, int currentPlaneAddress, byte currentByteValue, byte repeatCount, int vramPlaneSize)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                currentPlaneAddress = WriteSingleByteToPlane(planeData, currentPlaneAddress, currentByteValue, vramPlaneSize);
            }
            return currentPlaneAddress;
        }

        private int WriteSingleByteToPlane(byte[] planeData, int currentPlaneAddress, byte currentByteValue, int vramPlaneSize)
        {
            ThrowIfVramPlaneOverflow(currentPlaneAddress, vramPlaneSize);
            planeData[currentPlaneAddress++] = currentByteValue;
            return currentPlaneAddress;
        }

        private void ThrowIfVramPlaneOverflow(int currentPlaneAddress, int vramPlaneSize)
        {
            if (currentPlaneAddress >= vramPlaneSize)
            {
                throw new IndexOutOfRangeException("VramPlaneの書き込み範囲を超えました。");
            }
        }
    }
}
