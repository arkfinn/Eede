using Avalonia;
using Avalonia.Controls;
using Eede.Presentation.Common.Services;
using System.Windows.Input;

namespace Eede.Presentation.Views.DataEntry;

public partial class PaletteContainer : UserControl
{
    public static readonly StyledProperty<StorageService> StorageServiceProperty =
        AvaloniaProperty.Register<PaletteContainer, StorageService>(nameof(StorageService));

    public StorageService StorageService
    {
        get => GetValue(StorageServiceProperty);
        set => SetValue(StorageServiceProperty, value);
    }

    public static readonly StyledProperty<ICommand?> FetchColorCommandProperty =
        AvaloniaProperty.Register<PaletteContainer, ICommand?>(nameof(FetchColorCommand));
    public ICommand? FetchColorCommand
    {
        get => GetValue(FetchColorCommandProperty);
        set => SetValue(FetchColorCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> ApplyColorCommandProperty =
        AvaloniaProperty.Register<PaletteContainer, ICommand?>(nameof(ApplyColorCommand));
    public ICommand? ApplyColorCommand
    {
        get => GetValue(ApplyColorCommandProperty);
        set => SetValue(ApplyColorCommandProperty, value);
    }


    public PaletteContainer()
    {
        InitializeComponent();
    }
}