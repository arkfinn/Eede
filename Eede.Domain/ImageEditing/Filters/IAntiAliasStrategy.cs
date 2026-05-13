using System;

namespace Eede.Domain.ImageEditing.Filters;

public unsafe interface IAntiAliasStrategy
{
    bool IsDifferent(byte* p1, byte* p2, float threshold);
    float GetValueForEdgeDetection(byte* p);
    void Blend(byte* src, byte* dst, int xT, int yT, int xN, int yN, int stride, float weight);
}
