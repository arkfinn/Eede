using System;
using System.Diagnostics;
using Eede.Application.Infrastructure;

namespace Eede.Infrastructure.Services;

public class ExternalBrowserService : IExternalBrowserService
{
    internal Action<ProcessStartInfo> ProcessStarter { get; set; } = (psi) => Process.Start(psi);

    public void OpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("URL must start with http:// or https://", nameof(url));
        }

        ProcessStarter(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
