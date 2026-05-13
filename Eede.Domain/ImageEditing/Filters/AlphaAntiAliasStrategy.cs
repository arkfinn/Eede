using System;

namespace Eede.Domain.ImageEditing.Filters;

public unsafe class AlphaAntiAliasStrategy : IAntiAliasStrategy
{
    public bool IsDifferent(byte* p1, byte* p2, float threshold)
    {
        float a1 = p1[3] / 255.0f;
        float a2 = p2[3] / 255.0f;
        return Math.Abs(a1 - a2) > threshold;
    }

    public float GetValueForEdgeDetection(byte* p)
    {
        return p[3] / 255.0f;
    }

    public void Blend(byte* src, byte* dst, int xT, int yT, int xN, int yN, int stride, float a)
    {
        byte* pT = dst + yT * stride + xT * 4;
        byte* pN = src + yN * stride + xN * 4;
        byte* d = dst + yT * stride + xT * 4;

        float w = Math.Clamp(a * 0.85f, 0, 1);

        d[3] = (byte)(pT[3] * (1 - w) + pN[3] * w);
    }
}
