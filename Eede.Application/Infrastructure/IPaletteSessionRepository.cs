using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eede.Application.Infrastructure;

public interface IPaletteSessionRepository
{
    Task SaveAsync(IEnumerable<string> filePaths);
    IEnumerable<string> Load();
}
