using Avalonia;
using Avalonia.Controls;
using Eede.Presentation.Common.Adapters;

namespace Eede.Presentation.Views.DataEntry;

public partial class PaletteContainer : UserControl
{
    public PaletteContainer()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<AvaloniaFileStorage> FileStorageProperty =
        AvaloniaProperty.Register<PaletteContainer, AvaloniaFileStorage>(nameof(FileStorage));

    public AvaloniaFileStorage FileStorage
    {
        get => GetValue(FileStorageProperty);
        set => SetValue(FileStorageProperty, value);
    }
}