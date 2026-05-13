#nullable enable
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.GeometricTransformations
{
    public class BoxResampler : IImageResampler
    {
        public Picture Resize(Picture source, PictureSize newSize)
        {
            if (newSize.Width <= 0 || newSize.Height <= 0)
            {
                throw new ArgumentException("Width and Height must be greater than 0.");
            }

            int srcWidth = source.Width;
            int srcHeight = source.Height;
            int destWidth = newSize.Width;
            int destHeight = newSize.Height;

            // This resampler is optimized for integer downscaling (e.g., 2x -> 1x, 3x -> 1x)
            // It calculates the average of the pixels in each block.
            float scaleX = (float)srcWidth / destWidth;
            float scaleY = (float)srcHeight / destHeight;

            byte[] newData = new byte[destWidth * destHeight * 4];
            ReadOnlySpan<byte> srcSpan = source.AsSpan();
            int srcStride = source.Stride;

            for (int dy = 0; dy < destHeight; dy++)
            {
                int srcYStart = (int)(dy * scaleY);
                int srcYEnd = (int)Math.Ceiling((dy + 1) * scaleY);
                if (srcYEnd > srcHeight) srcYEnd = srcHeight;

                for (int dx = 0; dx < destWidth; dx++)
                {
                    int srcXStart = (int)(dx * scaleX);
                    int srcXEnd = (int)Math.Ceiling((dx + 1) * scaleX);
                    if (srcXEnd > srcWidth) srcXEnd = srcWidth;

                    long sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                    int count = 0;

                    for (int sy = srcYStart; sy < srcYEnd; sy++)
                    {
                        int rowOffset = sy * srcStride;
                        for (int sx = srcXStart; sx < srcXEnd; sx++)
                        {
                            int idx = rowOffset + sx * 4;
                            sumB += srcSpan[idx];
                            sumG += srcSpan[idx + 1];
                            sumR += srcSpan[idx + 2];
                            sumA += srcSpan[idx + 3];
                            count++;
                        }
                    }

                    int destIdx = (dy * destWidth + dx) * 4;
                    if (count > 0)
                    {
                        newData[destIdx] = (byte)(sumB / count);
                        newData[destIdx + 1] = (byte)(sumG / count);
                        newData[destIdx + 2] = (byte)(sumR / count);
                        newData[destIdx + 3] = (byte)(sumA / count);
                    }
                }
            }

            return Picture.Create(newSize, newData);
        }
    }
}
