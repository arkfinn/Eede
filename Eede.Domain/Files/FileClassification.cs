#nullable enable
using System;

namespace Eede.Domain.Files
{
    public enum FileKind
    {
        Unknown = 0,
        PngImage,
        BmpImage,
        ArvImage,
        ActPalette,
        AactPalette
    }

    /// <summary>
    /// 対応ファイル形式の判定と分類を司る純粋ドメインポリシー。
    /// </summary>
    public static class FileClassification
    {
        public static FileKind Classify(string? fileNameOrPath)
        {
            if (string.IsNullOrWhiteSpace(fileNameOrPath)) return FileKind.Unknown;

            string ext = GetExtension(fileNameOrPath);
            return ext switch
            {
                ".png" => FileKind.PngImage,
                ".bmp" => FileKind.BmpImage,
                ".arv" => FileKind.ArvImage,
                ".act" => FileKind.ActPalette,
                ".aact" => FileKind.AactPalette,
                _ => FileKind.Unknown
            };
        }

        public static bool IsSupportedImage(string? fileNameOrPath)
        {
            var kind = Classify(fileNameOrPath);
            return kind is FileKind.PngImage or FileKind.BmpImage or FileKind.ArvImage;
        }

        public static bool IsSupportedPalette(string? fileNameOrPath)
        {
            var kind = Classify(fileNameOrPath);
            return kind is FileKind.ActPalette or FileKind.AactPalette;
        }

        public static string GetExtension(string fileNameOrPath)
        {
            if (string.IsNullOrEmpty(fileNameOrPath)) return string.Empty;

            // クエリパラメータやフラグメントを除去
            int queryIndex = fileNameOrPath.IndexOf('?');
            if (queryIndex >= 0) fileNameOrPath = fileNameOrPath[..queryIndex];
            int fragmentIndex = fileNameOrPath.IndexOf('#');
            if (fragmentIndex >= 0) fileNameOrPath = fileNameOrPath[..fragmentIndex];

            int lastDot = fileNameOrPath.LastIndexOf('.');
            if (lastDot < 0) return string.Empty;
            return fileNameOrPath[lastDot..].ToLowerInvariant();
        }
    }
}
