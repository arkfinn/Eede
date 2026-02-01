using Eede.Application.Infrastructure;
using System.Threading.Tasks;

namespace Eede.Presentation.Common.Adapters;

public class AvaloniaFileSystem : IFileSystem
{
    public Task WriteAllTextAsync(string path, string contents) => System.IO.File.WriteAllTextAsync(path, contents);
    public Task<string> ReadAllTextAsync(string path) => System.IO.File.ReadAllTextAsync(path);
}
