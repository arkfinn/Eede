using Avalonia;
using Avalonia.Headless;
using Avalonia.ReactiveUI;
using Eede.Presentation.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Eede.Presentation.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions())
        .UseReactiveUI();
}
