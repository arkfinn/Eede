#nullable enable
using System;

namespace Eede.Domain.Files
{
    /// <summary>
    /// デスクトップローカルパスおよびブラウザ仮想URI（blob:, http:等）を統一的に扱うファイル識別値オブジェクト。
    /// </summary>
    public record FileIdentity
    {
        public string Path { get; }
        public string Name { get; }

        public FileIdentity(string path, string? name = null)
        {
            Path = path ?? string.Empty;
            Name = !string.IsNullOrEmpty(name)
                ? name!
                : ExtractNameFromPath(Path);
        }

        public string Extension => FileClassification.GetExtension(Name);

        public bool IsSupportedImage => FileClassification.IsSupportedImage(Name);
        public bool IsSupportedPalette => FileClassification.IsSupportedPalette(Name);

        private static string ExtractNameFromPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            // クエリパラメータやフラグメントを除去
            int queryIndex = path.IndexOf('?');
            if (queryIndex >= 0) path = path[..queryIndex];

            int lastSlash = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
            return lastSlash >= 0 ? path[(lastSlash + 1)..] : path;
        }

        public override string ToString() => Path;
    }
}
