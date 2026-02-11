#nullable enable
using System;

namespace Eede.Domain.Files
{
    public class FilePath(string filePath)
    {
        private readonly string Path = filePath ?? throw new ArgumentNullException(nameof(filePath));

        public bool IsEmpty()
        {
            return Path == "";
        }

        public static FilePath Empty()
        {
            return new FilePath("");
        }

        public string GetExtension()
        {
            return System.IO.Path.GetExtension(Path).ToLower();
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
