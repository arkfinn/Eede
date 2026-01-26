using System;
using Dock.Model.Avalonia;
using Dock.Model.Core;

namespace Eede.Presentation;

public class InjectableDockFactory : Factory
{
    private readonly IServiceProvider _services;

    public InjectableDockFactory(IServiceProvider services)
    {
        _services = services;
    }
}