#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Eede.Application.Pictures;

public interface IDisambiguatableItem
{
    string FilePath { get; }
    string DisambiguationSuffix { get; set; }
}

public static class TitleDisambiguator
{
    private static readonly char[] DirectorySeparators =
    [
        Path.DirectorySeparatorChar,
        Path.AltDirectorySeparatorChar
    ];

    /// <summary>
    /// 開かれているドキュメント一覧のファイルパスを比較し、同名ファイルが存在する場合にのみ
    /// 差異のある祖先ディレクトリ階層を " — path" 形式のサフィックスとして付与します。
    /// 重複のないアイテムや新規未保存アイテムのサフィックスは自動的にクリアされます。
    /// </summary>
    /// <typeparam name="T">IDisambiguatableItem 実装型</typeparam>
    /// <param name="items">ドキュメントコレクション</param>
    public static void Recompute<T>(IEnumerable<T>? items) where T : IDisambiguatableItem
    {
        if (items == null) return;

        var itemList = items.ToList();
        ClearSuffixes(itemList);

        foreach (var group in FindDuplicateFileNameGroups(itemList))
        {
            ApplyGroupDisambiguation(group);
        }
    }

    private static void ClearSuffixes<T>(IEnumerable<T> items) where T : IDisambiguatableItem
    {
        foreach (var item in items)
        {
            item.DisambiguationSuffix = "";
        }
    }

    private static IEnumerable<List<T>> FindDuplicateFileNameGroups<T>(IEnumerable<T> items) where T : IDisambiguatableItem
    {
        return items
            .Where(x => !string.IsNullOrWhiteSpace(x.FilePath))
            .GroupBy(x => Path.GetFileName(x.FilePath), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.ToList());
    }

    private static void ApplyGroupDisambiguation<T>(List<T> groupItems) where T : IDisambiguatableItem
    {
        var dirSegmentsList = groupItems.Select(x => GetDirectorySegments(x.FilePath)).ToList();

        for (int i = 0; i < groupItems.Count; i++)
        {
            int depth = CalculateRequiredDepth(dirSegmentsList, i);
            string distinctRelativePath = BuildDistinctPath(dirSegmentsList[i], depth);
            groupItems[i].DisambiguationSuffix = string.IsNullOrEmpty(distinctRelativePath)
                ? ""
                : $" — {distinctRelativePath}";
        }
    }

    private static int CalculateRequiredDepth(IReadOnlyList<string[]> dirSegmentsList, int targetIndex)
    {
        string[] myDirs = dirSegmentsList[targetIndex];
        int maxRequiredDepth = 1;

        for (int j = 0; j < dirSegmentsList.Count; j++)
        {
            if (targetIndex == j) continue;
            maxRequiredDepth = Math.Max(maxRequiredDepth, FindDisambiguatingDepth(myDirs, dirSegmentsList[j]));
        }

        return maxRequiredDepth;
    }

    private static string[] GetDirectorySegments(string filePath)
    {
        string? dir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir)) return [];

        return dir.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
    }

    private static int FindDisambiguatingDepth(string[] a, string[] b)
    {
        int depth = 1;
        while (depth <= a.Length && depth <= b.Length)
        {
            if (!a[a.Length - depth].Equals(b[b.Length - depth], StringComparison.OrdinalIgnoreCase))
            {
                return depth;
            }
            depth++;
        }
        return depth;
    }

    private static string BuildDistinctPath(string[] segments, int depth)
    {
        if (segments.Length == 0) return "";
        int take = Math.Min(depth, segments.Length);
        return string.Join("/", segments.Skip(segments.Length - take).Take(take));
    }
}
