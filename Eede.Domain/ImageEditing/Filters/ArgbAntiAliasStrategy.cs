using System;

namespace Eede.Domain.ImageEditing.Filters;

public unsafe class ArgbAntiAliasStrategy : IAntiAliasStrategy
{
    public bool IsDifferent(byte* p1, byte* p2, float threshold)
    {
        float l1 = GetLuminance(p1);
        float l2 = GetLuminance(p2);
        if (Math.Abs(l1 - l2) > threshold) return true;

        float a1 = p1[3] / 255.0f;
        float a2 = p2[3] / 255.0f;
        return Math.Abs(a1 - a2) > threshold;
    }

    public float GetValueForEdgeDetection(byte* p)
    {
        return GetLuminance(p);
    }

    public void Blend(byte* src, byte* dst, int xT, int yT, int xN, int yN, int stride, float a)
    {
        byte* pT = dst + yT * stride + xT * 4;
        byte* pN = src + yN * stride + xN * 4;
        byte* d = dst + yT * stride + xT * 4;

        float w = Math.Clamp(a * 0.85f, 0, 1);

        for (int i = 0; i < 4; i++)
        {
            d[i] = (byte)(pT[i] * (1 - w) + pN[i] * w);
        }
    }

    private float GetLuminance(byte* p)
    {
        return (0.299f * p[2] + 0.587f * p[1] + 0.114f * p[0]) / 255.0f;
    }
}
