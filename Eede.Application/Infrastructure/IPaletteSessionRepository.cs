using System.Collections.Generic;

namespace Eede.Application.Infrastructure;

public interface IPaletteSessionRepository
{
    void Save(IEnumerable<string> filePaths);
    IEnumerable<string> Load();
}
