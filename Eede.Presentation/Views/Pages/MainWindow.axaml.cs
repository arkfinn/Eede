using ReactiveUI.Avalonia;
using Eede.Presentation.ViewModels.Pages;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Domain.ImageEditing;
using System;
using System.Reactive;
using Avalonia.Controls;
using Eede.Presentation.Views.DataEntry;
using System.Reactive.Disposables.Fluent;

namespace Eede.Presentation.Views.Pages;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (ViewModel != null)
            {
                ViewModel.ShowCreateNewPictureModal.RegisterHandler(DoShowCreateNewFileWindowAsync).DisposeWith(disposables);
                ViewModel.ShowScalingModal.RegisterHandler(DoShowScalingWindowAsync).DisposeWith(disposables);
            }
        });
    }

    private async Task DoShowScalingWindowAsync(IInteractionContext<ScalingDialogViewModel, ResizeContext?> interaction)
    {
        var dialog = new Views.DataEntry.ScalingDialogView()
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<ResizeContext?>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowCreateNewFileWindowAsync(IInteractionContext<NewPictureWindowViewModel, NewPictureWindowViewModel> interaction)
    {
        NewPictureWindow dialog = new()
        {
            DataContext = interaction.Input,
            Width = 300,
            Height = 350
        };
        interaction.Input.Close = new Action(dialog.Close);

        _ = await dialog.ShowDialog<NewPictureWindowViewModel>(this);
        interaction.SetOutput(interaction.Input);
    }
}
