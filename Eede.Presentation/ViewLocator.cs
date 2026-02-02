using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Eede.Presentation.ViewModels;

namespace Eede.Presentation;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var name = data.GetType().FullName!.Replace(".ViewModels.", ".Views.");
        var type = Type.GetType(name.Replace("ViewModel", "View"));

        if (type == null)
        {
            // PaletteContainerViewModel -> PaletteContainer のようなケースに対応
            type = Type.GetType(name.Replace("ViewModel", ""));
        }

        if (type != null)
        {
            var services = App.Services;
            return (Control)(services?.GetService(type) ?? Activator.CreateInstance(type)!);
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase || data is Dock.Model.Core.IDockable;
    }
}
