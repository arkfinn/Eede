using System;
using System.Collections.Generic;
using System.Linq;

namespace Eede.Application.Settings;

public class AppSettings
{
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    public List<RecentFile> RecentFiles { get; set; } = new();

    public void AddRecentFile(string path, DateTime accessedAt)
    {
        var existing = RecentFiles.FirstOrDefault(f => f.Path == path);
        if (existing != null)
        {
            RecentFiles.Remove(existing);
        }

        RecentFiles.Insert(0, new RecentFile { Path = path, LastAccessed = accessedAt });

        if (RecentFiles.Count > 10)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }
}
