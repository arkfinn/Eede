using System;

namespace Eede.Application.Settings;

public class RecentFile
{
    public string Path { get; set; } = string.Empty;
    public DateTime LastAccessed { get; set; }
}
