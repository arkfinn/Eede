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

public static class TitleDisambiguationService
{
    /// <summary>
    /// 開かれているドキュメント一覧のファイルパスを比較し、同名ファイルが存在する場合にのみ
    /// 差異のある祖先ディレクトリ階層を " — path" 形式のサフィックスとして付与します。
    /// 重複のないアイテムや新規未保存アイテムのサフィックスは自動的にクリアされます。
    /// </summary>
    /// <typeparam name="T">IDisambiguatableItem 実装型</typeparam>
    /// <param name="items">ドキュメントコレクション</param>
    public static void Recompute<T>(IEnumerable<T> items) where T : IDisambiguatableItem
    {
        if (items == null) return;

        var itemList = items.ToList();

        // 1. 全アイテムのサフィックスを初期化
        foreach (var item in itemList)
        {
            item.DisambiguationSuffix = "";
        }

        // 2. 有効なファイルパスを持つアイテムのみを抽出
        var validItems = itemList
            .Where(x => !string.IsNullOrWhiteSpace(x.FilePath))
            .ToList();

        // 3. ファイル名（大文字小文字無視）でグループ化
        var duplicateGroups = validItems
            .GroupBy(x => Path.GetFileName(x.FilePath), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);

        foreach (var group in duplicateGroups)
        {
            var groupItems = group.ToList();
            var dirSegmentsList = groupItems.Select(x => GetDirectorySegments(x.FilePath)).ToList();

            for (int i = 0; i < groupItems.Count; i++)
            {
                var myDirs = dirSegmentsList[i];
                int maxRequiredDepth = 1;

                for (int j = 0; j < groupItems.Count; j++)
                {
                    if (i == j) continue;
                    var otherDirs = dirSegmentsList[j];
                    int depth = FindDisambiguatingDepth(myDirs, otherDirs);
                    if (depth > maxRequiredDepth)
                    {
                        maxRequiredDepth = depth;
                    }
                }

                string distinctRelativePath = BuildDistinctPath(myDirs, maxRequiredDepth);
                groupItems[i].DisambiguationSuffix = string.IsNullOrEmpty(distinctRelativePath)
                    ? ""
                    : $" — {distinctRelativePath}";
            }
        }
    }

    private static string[] GetDirectorySegments(string filePath)
    {
        try
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(dir)) return Array.Empty<string>();

            return dir.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static int FindDisambiguatingDepth(string[] a, string[] b)
    {
        int depth = 1;
        while (depth <= a.Length && depth <= b.Length)
        {
            string segA = a[a.Length - depth];
            string segB = b[b.Length - depth];

            if (!segA.Equals(segB, StringComparison.OrdinalIgnoreCase))
            {
                return depth;
            }
            depth++;
        }
        return Math.Max(depth, 1);
    }

    private static string BuildDistinctPath(string[] segments, int depth)
    {
        if (segments.Length == 0 || depth <= 0) return "";
        int take = Math.Min(depth, segments.Length);
        int skip = segments.Length - take;
        return string.Join("/", segments.Skip(skip).Take(take));
    }
}
