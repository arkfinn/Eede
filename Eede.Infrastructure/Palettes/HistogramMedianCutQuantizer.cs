#nullable enable
using System;
using Eede.Domain.Palettes;

namespace Eede.Infrastructure.Palettes;

/// <summary>
/// RGB555ヒストグラム・メディアンカットおよび実在色アンカー・バックフィルにより代表256色パレットを生成する量子化器。
/// </summary>
public static class HistogramMedianCutQuantizer
{
    public const int HistogramSize = 32768;

    private struct ColorBucket
    {
        public byte RMin;
        public byte RMax;
        public byte GMin;
        public byte GMax;
        public byte BMin;
        public byte BMax;
        public long Weight;

        public readonly bool CanSplit => (RMax > RMin) || (GMax > GMin) || (BMax > BMin);
    }

    private struct WeightedColor
    {
        public ArgbColor Color;
        public long Weight;
        public int Color555;
    }

    public static Palette Quantize(
        ReadOnlySpan<int> histogram,
        ReadOnlySpan<uint> anchorArgb,
        ReadOnlySpan<ArgbColor> fallbackUniqueColors,
        bool hasTransparent)
    {
        int targetBuckets = hasTransparent ? Palette.MAX_LENGTH - 1 : Palette.MAX_LENGTH;
        Span<ColorBucket> buckets = stackalloc ColorBucket[Palette.MAX_LENGTH];
        int bucketCount = 0;

        ColorBucket root = new()
        {
            RMin = 0, RMax = 31,
            GMin = 0, GMax = 31,
            BMin = 0, BMax = 31
        };

        Tighten(ref root, histogram);
        if (root.Weight > 0)
        {
            buckets[bucketCount++] = root;
        }

        while (bucketCount < targetBuckets)
        {
            int bestIdx = -1;
            long bestWeight = 0;

            for (int i = 0; i < bucketCount; i++)
            {
                if (buckets[i].CanSplit && buckets[i].Weight > bestWeight)
                {
                    bestWeight = buckets[i].Weight;
                    bestIdx = i;
                }
            }

            if (bestIdx < 0)
            {
                break;
            }

            SplitBucket(buckets[bestIdx], histogram, out ColorBucket left, out ColorBucket right);
            buckets[bestIdx] = left;
            buckets[bucketCount++] = right;
        }

        Span<WeightedColor> extracted = stackalloc WeightedColor[bucketCount];
        for (int i = 0; i < bucketCount; i++)
        {
            ref readonly ColorBucket b = ref buckets[i];
            int bestColor555 = -1;
            int maxFreq = -1;

            for (int r = b.RMin; r <= b.RMax; r++)
            {
                int rBits = r << 10;
                for (int g = b.GMin; g <= b.GMax; g++)
                {
                    int rgBits = rBits | (g << 5);
                    for (int bl = b.BMin; bl <= b.BMax; bl++)
                    {
                        int idx = rgBits | bl;
                        int freq = histogram[idx];
                        if (freq > maxFreq)
                        {
                            maxFreq = freq;
                            bestColor555 = idx;
                        }
                    }
                }
            }

            uint packed = bestColor555 >= 0 ? anchorArgb[bestColor555] : 0u;
            extracted[i] = new WeightedColor
            {
                Color = UnpackArgb(packed),
                Weight = b.Weight,
                Color555 = bestColor555
            };
        }

        // バケット重み（Weight）降順、同重み時は RGB555 インデックス昇順で決定論的に整列
        for (int i = 1; i < bucketCount; i++)
        {
            WeightedColor key = extracted[i];
            int j = i - 1;
            while (j >= 0 && (extracted[j].Weight < key.Weight ||
                   (extracted[j].Weight == key.Weight && extracted[j].Color555 > key.Color555)))
            {
                extracted[j + 1] = extracted[j];
                j--;
            }
            extracted[j + 1] = key;
        }

        ArgbColor[] paletteColors = new ArgbColor[Palette.MAX_LENGTH];
        int outIdx = 0;

        if (hasTransparent)
        {
            paletteColors[outIdx++] = new ArgbColor(0, 0, 0, 0);
        }

        for (int i = 0; i < bucketCount && outIdx < Palette.MAX_LENGTH; i++)
        {
            paletteColors[outIdx++] = extracted[i].Color;
        }

        // RGB555縮退や極薄半透明(A < 16)のみで目標数に満たない場合、Fast-Path収集済みの実在ユニーク色をバックフィル
        if (outIdx < Palette.MAX_LENGTH)
        {
            for (int i = 0; i < fallbackUniqueColors.Length && outIdx < Palette.MAX_LENGTH; i++)
            {
                ArgbColor candidate = fallbackUniqueColors[i];
                if (candidate.Alpha == 0)
                {
                    continue;
                }

                bool alreadyExists = false;
                for (int j = hasTransparent ? 1 : 0; j < outIdx; j++)
                {
                    if (paletteColors[j].EqualsArgb(candidate))
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    paletteColors[outIdx++] = candidate;
                }
            }
        }

        while (outIdx < Palette.MAX_LENGTH)
        {
            paletteColors[outIdx++] = new ArgbColor(0, 0, 0, 0);
        }

        return Palette.FromColors(paletteColors);
    }

    private static void Tighten(ref ColorBucket bucket, ReadOnlySpan<int> histogram)
    {
        byte rMin = 31, rMax = 0;
        byte gMin = 31, gMax = 0;
        byte bMin = 31, bMax = 0;
        long totalWeight = 0;

        for (byte r = bucket.RMin; r <= bucket.RMax; r++)
        {
            int rBits = r << 10;
            for (byte g = bucket.GMin; g <= bucket.GMax; g++)
            {
                int rgBits = rBits | (g << 5);
                for (byte b = bucket.BMin; b <= bucket.BMax; b++)
                {
                    int w = histogram[rgBits | b];
                    if (w > 0)
                    {
                        totalWeight += w;
                        if (r < rMin) rMin = r;
                        if (r > rMax) rMax = r;
                        if (g < gMin) gMin = g;
                        if (g > gMax) gMax = g;
                        if (b < bMin) bMin = b;
                        if (b > bMax) bMax = b;
                    }
                }
            }
        }

        if (totalWeight > 0)
        {
            bucket.RMin = rMin;
            bucket.RMax = rMax;
            bucket.GMin = gMin;
            bucket.GMax = gMax;
            bucket.BMin = bMin;
            bucket.BMax = bMax;
            bucket.Weight = totalWeight;
        }
        else
        {
            bucket.RMax = bucket.RMin;
            bucket.GMax = bucket.GMin;
            bucket.BMax = bucket.BMin;
            bucket.Weight = 0;
        }
    }

    private static void SplitBucket(
        ColorBucket bucket,
        ReadOnlySpan<int> histogram,
        out ColorBucket left,
        out ColorBucket right)
    {
        int rSpan = bucket.RMax - bucket.RMin;
        int gSpan = bucket.GMax - bucket.GMin;
        int bSpan = bucket.BMax - bucket.BMin;

        left = bucket;
        right = bucket;
        long halfWeight = (bucket.Weight + 1) / 2;
        long accumulated = 0;

        // 視覚感度順 (G -> R -> B) で最大スパンの軸を選択
        if (gSpan >= rSpan && gSpan >= bSpan)
        {
            int medianG = bucket.GMin;
            for (int g = bucket.GMin; g <= bucket.GMax; g++)
            {
                long sliceWeight = 0;
                int gBits = g << 5;
                for (int r = bucket.RMin; r <= bucket.RMax; r++)
                {
                    int rgBits = (r << 10) | gBits;
                    for (int b = bucket.BMin; b <= bucket.BMax; b++)
                    {
                        sliceWeight += histogram[rgBits | b];
                    }
                }
                accumulated += sliceWeight;
                if (accumulated >= halfWeight)
                {
                    medianG = g;
                    break;
                }
            }

            byte split = (byte)Math.Clamp(medianG, bucket.GMin, bucket.GMax - 1);
            left.GMax = split;
            right.GMin = (byte)(split + 1);
        }
        else if (rSpan >= gSpan && rSpan >= bSpan)
        {
            int medianR = bucket.RMin;
            for (int r = bucket.RMin; r <= bucket.RMax; r++)
            {
                long sliceWeight = 0;
                int rBits = r << 10;
                for (int g = bucket.GMin; g <= bucket.GMax; g++)
                {
                    int rgBits = rBits | (g << 5);
                    for (int b = bucket.BMin; b <= bucket.BMax; b++)
                    {
                        sliceWeight += histogram[rgBits | b];
                    }
                }
                accumulated += sliceWeight;
                if (accumulated >= halfWeight)
                {
                    medianR = r;
                    break;
                }
            }

            byte split = (byte)Math.Clamp(medianR, bucket.RMin, bucket.RMax - 1);
            left.RMax = split;
            right.RMin = (byte)(split + 1);
        }
        else
        {
            int medianB = bucket.BMin;
            for (int b = bucket.BMin; b <= bucket.BMax; b++)
            {
                long sliceWeight = 0;
                for (int r = bucket.RMin; r <= bucket.RMax; r++)
                {
                    int rBits = r << 10;
                    for (int g = bucket.GMin; g <= bucket.GMax; g++)
                    {
                        sliceWeight += histogram[rBits | (g << 5) | b];
                    }
                }
                accumulated += sliceWeight;
                if (accumulated >= halfWeight)
                {
                    medianB = b;
                    break;
                }
            }

            byte split = (byte)Math.Clamp(medianB, bucket.BMin, bucket.BMax - 1);
            left.BMax = split;
            right.BMin = (byte)(split + 1);
        }

        Tighten(ref left, histogram);
        Tighten(ref right, histogram);
    }

    private static ArgbColor UnpackArgb(uint packed)
    {
        return new ArgbColor(
            (byte)(packed >> 24),
            (byte)(packed >> 16),
            (byte)(packed >> 8),
            (byte)packed);
    }
}
