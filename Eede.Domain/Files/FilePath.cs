namespace Eede.Domain.Files
{
    public class FilePath(string filePath)
    {
        public readonly string Path = filePath;

        public bool IsEmpty()
        {
            return Path == "";
        }

        public static FilePath Empty()
        {
            return new FilePath("");
        }
    }
}
