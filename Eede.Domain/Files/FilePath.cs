namespace Eede.Domain.Files
{
    public class FilePath(string filePath)
    {
        private readonly string Path = filePath;

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
