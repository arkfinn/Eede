namespace Eede.Domain.Files
{
    public class FilePath
    {
        public readonly string Path;
        public FilePath(string filePath)
        {
            Path = filePath;
        }

        public bool IsEmpty()
        {
            return Path == "";
        }
    }
}
