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

using Avalonia.Input;
using Avalonia.Interactivity;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.GeometricTransformations;

namespace Eede.Presentation.Views.Pages;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        this.AddHandler(KeyDownEvent, OnGlobalKeyDown, RoutingStrategies.Tunnel);

        this.WhenActivated(disposables =>
        {
            if (ViewModel != null)
            {
                disposables.Add(ViewModel.ShowCreateNewPictureModal.RegisterHandler(DoShowCreateNewFileWindowAsync));
                disposables.Add(ViewModel.ShowScalingModal.RegisterHandler(DoShowScalingWindowAsync));
            }
        });
    }

    private void OnGlobalKeyDown(object? sender, KeyEventArgs e)
    {
        var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (focused is TextBox)
        {
            return;
        }

        if (e.KeyModifiers == KeyModifiers.None)
        {
            switch (e.Key)
            {
                case Key.B:
                case Key.P:
                    ViewModel?.SetDrawStyleCommand.Execute(DrawStyleType.FreeCurve).Subscribe();
                    e.Handled = true;
                    break;
                case Key.M:
                case Key.S:
                    ViewModel?.SetDrawStyleCommand.Execute(DrawStyleType.RegionSelect).Subscribe();
                    e.Handled = true;
                    break;
                case Key.L:
                    ViewModel?.SetDrawStyleCommand.Execute(DrawStyleType.Line).Subscribe();
                    e.Handled = true;
                    break;
                case Key.G:
                case Key.F:
                    ViewModel?.SetDrawStyleCommand.Execute(DrawStyleType.Fill).Subscribe();
                    e.Handled = true;
                    break;
                case Key.U:
                case Key.R:
                    ViewModel?.SetDrawStyleCommand.Execute(DrawStyleType.Rectangle).Subscribe();
                    e.Handled = true;
                    break;
                case Key.C:
                    ViewModel?.SetDrawStyleCommand.Execute(DrawStyleType.Ellipse).Subscribe();
                    e.Handled = true;
                    break;
                case Key.X:
                    ViewModel?.SwapColorsCommand.Execute().Subscribe();
                    e.Handled = true;
                    break;
                case Key.D:
                    ViewModel?.ResetDefaultColorsCommand.Execute().Subscribe();
                    e.Handled = true;
                    break;
                case Key.OemOpenBrackets:
                    ViewModel?.DecreasePenWidthCommand.Execute().Subscribe();
                    e.Handled = true;
                    break;
                case Key.OemCloseBrackets:
                    ViewModel?.IncreasePenWidthCommand.Execute().Subscribe();
                    e.Handled = true;
                    break;
                case Key.Up:
                    ViewModel?.PictureActionCommand.Execute(PictureActions.ShiftUp).Subscribe();
                    e.Handled = true;
                    break;
                case Key.Down:
                    ViewModel?.PictureActionCommand.Execute(PictureActions.ShiftDown).Subscribe();
                    e.Handled = true;
                    break;
                case Key.Left:
                    ViewModel?.PictureActionCommand.Execute(PictureActions.ShiftLeft).Subscribe();
                    e.Handled = true;
                    break;
                case Key.Right:
                    ViewModel?.PictureActionCommand.Execute(PictureActions.ShiftRight).Subscribe();
                    e.Handled = true;
                    break;
                case Key.D1:
                    ViewModel?.SetMagnificationCommand.Execute(1f).Subscribe();
                    e.Handled = true;
                    break;
                case Key.D2:
                    ViewModel?.SetMagnificationCommand.Execute(2f).Subscribe();
                    e.Handled = true;
                    break;
                case Key.D3:
                    ViewModel?.SetMagnificationCommand.Execute(4f).Subscribe();
                    e.Handled = true;
                    break;
                case Key.D4:
                    ViewModel?.SetMagnificationCommand.Execute(6f).Subscribe();
                    e.Handled = true;
                    break;
                case Key.D5:
                    ViewModel?.SetMagnificationCommand.Execute(8f).Subscribe();
                    e.Handled = true;
                    break;
                case Key.D6:
                    ViewModel?.SetMagnificationCommand.Execute(12f).Subscribe();
                    e.Handled = true;
                    break;
                case Key.F5:
                    ViewModel?.PullFromActiveDockCommand.Execute().Subscribe();
                    e.Handled = true;
                    break;
                case Key.Enter:
                case Key.Space:
                    ViewModel?.AnimationViewModel.TogglePlayCommand.Execute().Subscribe();
                    e.Handled = true;
                    break;
                case Key.OemComma:
                    ViewModel?.AnimationViewModel.PreviousFrameCommand.Execute().Subscribe();
                    e.Handled = true;
                    break;
                case Key.OemPeriod:
                    ViewModel?.AnimationViewModel.NextFrameCommand.Execute().Subscribe();
                    e.Handled = true;
                    break;
            }
        }
        else if (e.KeyModifiers == KeyModifiers.Control)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ViewModel?.PushToActiveDockCommand.Execute().Subscribe();
                    e.Handled = true;
                    break;
            }
        }
        else if (e.KeyModifiers == KeyModifiers.Shift)
        {
            switch (e.Key)
            {
                case Key.D1:
                    ViewModel?.SetPenWidthCommand.Execute(1).Subscribe();
                    e.Handled = true;
                    break;
                case Key.D2:
                    ViewModel?.SetPenWidthCommand.Execute(3).Subscribe();
                    e.Handled = true;
                    break;
                case Key.D3:
                    ViewModel?.SetPenWidthCommand.Execute(6).Subscribe();
                    e.Handled = true;
                    break;
            }
        }
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
