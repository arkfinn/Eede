using System.Threading.Tasks;

namespace Eede.Application.Infrastructure;

public interface IFileSystem
{
    Task WriteAllTextAsync(string path, string contents);
    Task<string> ReadAllTextAsync(string path);
}
