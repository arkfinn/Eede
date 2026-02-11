#nullable enable
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.GeometricTransformations
{
    public class NearestNeighborResampler : IImageResampler
    {
        public Picture Resize(Picture source, PictureSize newSize)
        {
            // Validate newSize
            if (newSize.Width <= 0 || newSize.Height <= 0)
            {
                throw new ArgumentException("Width and Height must be greater than 0.");
            }

            // 新しい画像バッファ
            var newData = new byte[newSize.Width * newSize.Height * 4];
            
            var sourceSpan = source.AsSpan();
            int sourceStride = source.Stride;
            int sourceWidth = source.Width;
            int sourceHeight = source.Height;

            int newWidth = newSize.Width;
            int newHeight = newSize.Height;
            int newStride = newWidth * 4;

            // X軸のマッピングテーブルを事前に計算
            int[] xMap = new int[newWidth];
            for (int x = 0; x < newWidth; x++)
            {
                xMap[x] = (x * sourceWidth) / newWidth;
            }

            for (int y = 0; y < newHeight; y++)
            {
                int srcY = (y * sourceHeight) / newHeight;
                int srcRowOffset = srcY * sourceStride;
                int destRowOffset = y * newStride;

                for (int x = 0; x < newWidth; x++)
                {
                    int srcX = xMap[x];
                    int srcIndex = srcRowOffset + (srcX * 4);
                    int destIndex = destRowOffset + (x * 4);

                    // RGBAコピー
                    newData[destIndex] = sourceSpan[srcIndex];
                    newData[destIndex + 1] = sourceSpan[srcIndex + 1];
                    newData[destIndex + 2] = sourceSpan[srcIndex + 2];
                    newData[destIndex + 3] = sourceSpan[srcIndex + 3];
                }
            }

            return Picture.Create(newSize, newData);
        }
    }
}