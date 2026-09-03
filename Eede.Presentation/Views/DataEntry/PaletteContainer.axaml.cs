using Avalonia;
using Avalonia.Controls;
using ReactiveUI.Avalonia;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.Common.Enums;
using Eede.Views.Pages;
using ReactiveUI;
using System.Threading.Tasks;

namespace Eede.Presentation.Views.DataEntry;

public partial class PaletteContainer : ReactiveUserControl<PaletteContainerViewModel>
{
    public PaletteContainer()
    {
        InitializeComponent();
        this.DataContextChanged += (s, e) =>
        {
            if (DataContext is PaletteContainerViewModel vm)
            {
                this.BindInteraction(vm, x => x.ConfirmCloseInteraction, DoConfirmCloseAsync);
            }
        };
    }

    private async Task DoConfirmCloseAsync(IInteractionContext<PaletteTabViewModel, SaveAlertResult> interaction)
    {
        string displayName = string.IsNullOrEmpty(interaction.Input.FilePath)
            ? interaction.Input.CustomTitle
            : interaction.Input.FilePath;
        var window = new Eede.Views.Pages.SaveAlertWindow(displayName);
        if (TopLevel.GetTopLevel(this) is Window parent)
        {
            await window.ShowDialog(parent);
            interaction.SetOutput(window.Result);
        }
    }

    public static readonly StyledProperty<AvaloniaFileStorage> FileStorageProperty =
        AvaloniaProperty.Register<PaletteContainer, AvaloniaFileStorage>(nameof(FileStorage));

    public AvaloniaFileStorage FileStorage
    {
        get => GetValue(FileStorageProperty);
        set => SetValue(FileStorageProperty, value);
    }
}