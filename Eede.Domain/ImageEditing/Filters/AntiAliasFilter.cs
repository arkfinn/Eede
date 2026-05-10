using System;
using System.Threading.Tasks;
using Eede.Domain.ImageEditing;

namespace Eede.Domain.ImageEditing.Filters;

public unsafe class AntiAliasFilter
{
    private const float Threshold = 0.1f;
    private const int MaxSearchDistance = 3;

    private readonly IAntiAliasStrategy Strategy;
    private readonly int MagnificationFactor;

    public AntiAliasFilter(IAntiAliasStrategy strategy, int magnificationFactor = 1)
    {
        Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        MagnificationFactor = magnificationFactor;
    }

    public Picture Apply(Picture source)
    {
        if (MagnificationFactor <= 1)
        {
            return ApplyInternal(source);
        }

        var upSize = new SharedKernel.PictureSize(source.Width * MagnificationFactor, source.Height * MagnificationFactor);
        var upscaled = new GeometricTransformations.NearestNeighborResampler().Resize(source, upSize);
        var filtered = ApplyInternal(upscaled);
        return new GeometricTransformations.BoxResampler().Resize(filtered, source.Size);
    }

    private Picture ApplyInternal(Picture source)
    {
        int width = source.Width;
        int height = source.Height;
        int stride = source.Stride;
        byte[] sourceData = source.CloneImage();
        byte[] resultData = (byte[])sourceData.Clone();

        bool[] edgeHoriz = new bool[width * height];
        bool[] edgeVert = new bool[width * height];

        fixed (byte* srcPtr = sourceData)
        fixed (byte* dstPtr = resultData)
        fixed (bool* hEdgePtr = edgeHoriz)
        fixed (bool* vEdgePtr = edgeVert)
        {
            DetectEdges(srcPtr, width, height, stride, hEdgePtr, vEdgePtr);
            
            ApplyVerticalMLAA(srcPtr, dstPtr, width, height, stride, hEdgePtr, vEdgePtr);
            ApplyHorizontalMLAA(srcPtr, dstPtr, width, height, stride, hEdgePtr, vEdgePtr);
        }

        return Picture.Create(source.Size, resultData);
    }

    private void DetectEdges(byte* srcPtr, int width, int height, int stride, bool* hEdgePtr, bool* vEdgePtr)
    {
        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                if (x < width - 1)
                {
                    vEdgePtr[y * width + x] = Strategy.IsDifferent(srcPtr + y * stride + x * 4, srcPtr + y * stride + (x + 1) * 4, Threshold);
                }
                if (y < height - 1)
                {
                    hEdgePtr[y * width + x] = Strategy.IsDifferent(srcPtr + y * stride + x * 4, srcPtr + (y + 1) * stride + x * 4, Threshold);
                }
            }
        });
    }

    private void ApplyVerticalMLAA(byte* srcPtr, byte* dstPtr, int width, int height, int stride, bool* hEdgePtr, bool* vEdgePtr)
    {
        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (vEdgePtr[y * width + x])
                {
                    int yStart = y;
                    int yEnd = y;
                    while (yEnd + 1 < height && vEdgePtr[(yEnd + 1) * width + x] && (yEnd - yStart + 1) < MaxSearchDistance)
                    {
                        yEnd++;
                    }
                    int L = yEnd - yStart + 1;

                    int d1 = 0;
                    if (yStart > 0)
                    {
                        if (hEdgePtr[(yStart - 1) * width + x]) d1 = -1;
                        else if (hEdgePtr[(yStart - 1) * width + x + 1]) d1 = 1;
                    }
                    int d2 = 0;
                    if (yEnd < height - 1)
                    {
                        if (hEdgePtr[yEnd * width + x]) d2 = -1;
                        else if (hEdgePtr[yEnd * width + x + 1]) d2 = 1;
                    }

                    if (d1 != 0 || d2 != 0)
                    {
                        for (int i = 0; i < L; i++)
                        {
                            float weightStart = CalculateMLAAArea(L, i, d1, 0);
                            float weightEnd = CalculateMLAAArea(L, i, 0, d2);

                            if (weightStart != 0)
                            {
                                if (d1 == -1) ApplyAdaptiveBlend(srcPtr, dstPtr, x, yStart + i, x + 1, yStart + i, width, height, stride, Math.Abs(weightStart));
                                else if (d1 == 1) ApplyAdaptiveBlend(srcPtr, dstPtr, x + 1, yStart + i, x, yStart + i, width, height, stride, Math.Abs(weightStart));
                            }
                            if (weightEnd != 0)
                            {
                                if (d2 == -1) ApplyAdaptiveBlend(srcPtr, dstPtr, x, yStart + i, x + 1, yStart + i, width, height, stride, Math.Abs(weightEnd));
                                else if (d2 == 1) ApplyAdaptiveBlend(srcPtr, dstPtr, x + 1, yStart + i, x, yStart + i, width, height, stride, Math.Abs(weightEnd));
                            }
                        }
                    }
                    y = yEnd;
                }
            }
        }
    }

    private void ApplyHorizontalMLAA(byte* srcPtr, byte* dstPtr, int width, int height, int stride, bool* hEdgePtr, bool* vEdgePtr)
    {
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (hEdgePtr[y * width + x])
                {
                    int xStart = x;
                    int xEnd = x;
                    while (xEnd + 1 < width && hEdgePtr[y * width + xEnd + 1] && (xEnd - xStart + 1) < MaxSearchDistance)
                    {
                        xEnd++;
                    }
                    int L = xEnd - xStart + 1;

                    int d1 = 0;
                    if (xStart > 0)
                    {
                        if (vEdgePtr[y * width + xStart - 1]) d1 = -1;
                        else if (vEdgePtr[(y + 1) * width + xStart - 1]) d1 = 1;
                    }
                    int d2 = 0;
                    if (xEnd < width - 1)
                    {
                        if (vEdgePtr[y * width + xEnd]) d2 = -1;
                        else if (vEdgePtr[(y + 1) * width + xEnd]) d2 = 1;
                    }

                    if (d1 != 0 || d2 != 0)
                    {
                        for (int i = 0; i < L; i++)
                        {
                            float weightStart = CalculateMLAAArea(L, i, d1, 0);
                            float weightEnd = CalculateMLAAArea(L, i, 0, d2);

                            if (weightStart != 0)
                            {
                                if (d1 == -1) ApplyAdaptiveBlend(srcPtr, dstPtr, xStart + i, y, xStart + i, y + 1, width, height, stride, Math.Abs(weightStart));
                                else if (d1 == 1) ApplyAdaptiveBlend(srcPtr, dstPtr, xStart + i, y + 1, xStart + i, y, width, height, stride, Math.Abs(weightStart));
                            }
                            if (weightEnd != 0)
                            {
                                if (d2 == -1) ApplyAdaptiveBlend(srcPtr, dstPtr, xStart + i, y, xStart + i, y + 1, width, height, stride, Math.Abs(weightEnd));
                                else if (d2 == 1) ApplyAdaptiveBlend(srcPtr, dstPtr, xStart + i, y + 1, xStart + i, y, width, height, stride, Math.Abs(weightEnd));
                            }
                        }
                    }
                    x = xEnd;
                }
            }
        }
    }

    private void ApplyAdaptiveBlend(byte* srcPtr, byte* dstPtr, int xT, int yT, int xN, int yN, int width, int height, int stride, float weight)
    {
        int countT = CountSimilarNeighbors(srcPtr, xT, yT, width, height, stride);
        
        float finalWeight = weight;
        if (countT <= 3) finalWeight *= 0.35f;
        else finalWeight *= 1.1f;

        Strategy.Blend(srcPtr, dstPtr, xT, yT, xN, yN, stride, finalWeight);
    }

    private int CountSimilarNeighbors(byte* ptr, int x, int y, int width, int height, int stride)
    {
        byte* pRef = ptr + y * stride + x * 4;
        float refVal = Strategy.GetValueForEdgeDetection(pRef);
        int count = 0;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    byte* pCur = ptr + ny * stride + nx * 4;
                    if (Math.Abs(Strategy.GetValueForEdgeDetection(pCur) - refVal) < Threshold)
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }

    private float CalculateMLAAArea(int L, int i, int d1, int d2)
    {
        float e1 = d1 * 0.5f;
        float e2 = d2 * 0.5f;
        float v1 = e1 + (e2 - e1) * (float)i / L;
        float v2 = e1 + (e2 - e1) * (float)(i + 1) / L;

        if (v1 > 0 || v2 > 0) return GetPositiveArea(v1, v2);
        if (v1 < 0 || v2 < 0) return -GetPositiveArea(-v1, -v2);
        return 0;
    }

    private float GetPositiveArea(float v1, float v2)
    {
        if (v1 <= 0 && v2 <= 0) return 0;
        if (v1 >= 0 && v2 >= 0) return (v1 + v2) / 2.0f;
        float t = -v1 / (v2 - v1);
        if (v1 < 0) return 0.5f * v2 * (1.0f - t);
        return 0.5f * v1 * t;
    }
}
