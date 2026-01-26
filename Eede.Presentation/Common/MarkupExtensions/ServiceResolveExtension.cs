using System;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace Eede.Presentation.Common.MarkupExtensions;

public class ServiceResolveExtension : MarkupExtension
{
    public Type Type { get; set; }

    public ServiceResolveExtension(Type type)
    {
        Type = type;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (App.Services == null)
        {
            throw new InvalidOperationException("App.Services is not initialized.");
        }
        return App.Services.GetRequiredService(Type);
    }
}
