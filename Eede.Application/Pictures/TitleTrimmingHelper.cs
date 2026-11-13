using System;
using System.IO;

namespace Eede.Application.Pictures;

public static class TitleTrimmingHelper
{
    /// <summary>
    /// 文字列が指定長を超える場合、先頭と末尾（拡張子含む）を保持したまま中間部を "..." に置換して短縮します。
    /// </summary>
    /// <param name="text">対象文字列（ファイル名等）</param>
    /// <param name="maxLength">最大文字数（デフォルト: 18）</param>
    /// <returns>中間省略された文字列</returns>
    public static string TruncateMiddle(string text, int maxLength = 18)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength || maxLength <= 6)
        {
            return text;
        }

        string ext = Path.GetExtension(text);
        string nameWithoutExt = Path.GetFileNameWithoutExtension(text);

        // 拡張子自体が長すぎて中間省略枠に収まらない場合は末尾をトリム
        if (ext.Length >= maxLength - 3)
        {
            return text.Substring(0, maxLength - 3) + "...";
        }

        int availableLength = maxLength - 3 - ext.Length;
        int headLength = (availableLength + 1) / 2;
        int tailLength = availableLength - headLength;

        string head = nameWithoutExt.Substring(0, Math.Min(headLength, nameWithoutExt.Length));
        string tail = tailLength > 0 && nameWithoutExt.Length >= tailLength
            ? nameWithoutExt.Substring(nameWithoutExt.Length - tailLength)
            : "";

        return $"{head}...{tail}{ext}";
    }
}
