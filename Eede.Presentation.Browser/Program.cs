using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using ReactiveUI.Avalonia;
using Eede.Presentation;

[assembly: SupportedOSPlatform("browser")]

namespace Eede.Presentation.Browser;

internal partial class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("[Eede] Starting Avalonia Browser App...");
            await BuildAvaloniaApp()
                .WithInterFont()
                .UseReactiveUI(_ => { })
                .StartBrowserAppAsync("out");
            Console.WriteLine("[Eede] Avalonia Browser App initialized successfully. Keeping runtime active...");
            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Eede Fatal Error] {ex}");
            if (ex is TypeInitializationException tie && tie.InnerException != null)
            {
                Console.Error.WriteLine($"[Eede InnerException] {tie.InnerException}");
            }
            if (ex is AggregateException ae)
            {
                foreach (var inner in ae.Flatten().InnerExceptions)
                {
                    Console.Error.WriteLine($"[Eede AggregateInner] {inner}");
                }
            }
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
