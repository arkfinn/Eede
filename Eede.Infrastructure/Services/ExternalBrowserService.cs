using System.Diagnostics;
using Eede.Application.Infrastructure;

namespace Eede.Infrastructure.Services;

public class ExternalBrowserService : IExternalBrowserService
{
    public void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
