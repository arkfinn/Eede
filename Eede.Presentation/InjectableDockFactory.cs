using System;
using Dock.Model.Avalonia;
using Dock.Model.Core;

namespace Eede.Presentation;

public class InjectableDockFactory : Factory
{
    public new event EventHandler<IDockable?>? ActiveDockableChanged;

    private IDockable? _lastFocused;

    public override void SetFocusedDockable(IDock? dock, IDockable? dockable)
    {
        if (dock is not null)
        {
            base.SetFocusedDockable(dock, dockable);
        }
        
        if (_lastFocused != dockable)
        {
            _lastFocused = dockable;
            ActiveDockableChanged?.Invoke(this, dockable);
        }
    }
}