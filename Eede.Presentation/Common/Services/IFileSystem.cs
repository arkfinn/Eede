using System.Threading.Tasks;

namespace Eede.Presentation.Common.Services;

public interface IFileSystem
{
    Task WriteAllTextAsync(string path, string contents);
    Task<string> ReadAllTextAsync(string path);
}

public class RealFileSystem : IFileSystem
{
    public Task WriteAllTextAsync(string path, string contents) => System.IO.File.WriteAllTextAsync(path, contents);
    public Task<string> ReadAllTextAsync(string path) => System.IO.File.ReadAllTextAsync(path);
}
