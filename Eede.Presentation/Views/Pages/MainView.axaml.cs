using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Domain.ImageEditing;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Eede.Presentation.Views.Pages;

#nullable enable

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
        FileStorage = new AvaloniaFileStorage(new Window().StorageProvider);

        DataContextChanged += (sender, e) =>
        {
            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }
            AddHandler(DragDrop.DragOverEvent, viewModel.DragOverPicture);
            AddHandler(DragDrop.DropEvent, viewModel.DropPicture);
            _ = this.BindInteraction(
                viewModel,
                vm => vm.ShowCreateNewPictureModal, DoShowCreateNewFileWindowAsync);

            _ = this.BindInteraction(
                viewModel,
                vm => vm.ShowScalingModal, DoShowScalingWindowAsync);

            viewModel.FileStorage = FileStorage;

            // Load Custom Cursor for Animation Mode
            try
            {
                System.IO.Stream assetLoader = AssetLoader.Open(new Uri("avares://Eede.Presentation/Assets/Tools/tool_animation_record.png"));
                Bitmap bitmap = new(assetLoader);
                viewModel.AnimationCursor = new Cursor(bitmap, new PixelPoint(8, 8));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load animation cursor: {ex.Message}");
            }
        };

        _ = this.WhenActivated(disposables =>
        {
            if (TopLevel.GetTopLevel(this) is not Window window)
            {
                return;
            }

            if (ViewModel != null)
            {
                // ViewModelのInteractionを購読し、通知が来たらウィンドウを閉じる
                _ = ViewModel.CloseWindowInteraction.RegisterHandler(interaction =>
                {
                    window.Close();
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);


                // WindowのClosingイベントをObservableに変換
                _ = Observable.FromEventPattern<EventHandler<WindowClosingEventArgs>, WindowClosingEventArgs>(
                    handler => window.Closing += handler,
                    handler => window.Closing -= handler)
                    .Subscribe(args =>
                    {
                        // ViewModelが既にクローズを確定している場合は、何もしないで通過させる
                        if (ViewModel.IsCloseConfirmed)
                        {
                            return;
                        }

                        // イベントをキャンセルして、ViewModelに処理を委譲する
                        args.EventArgs.Cancel = true;

                        // ViewModelのコマンドを実行
                        _ = ViewModel.RequestCloseCommand.Execute().Subscribe();

                    }).DisposeWith(disposables);
            }
        });
    }

    public AvaloniaFileStorage FileStorage { get; private set; }

    public void OnClickThemeSelect(object? sender, SelectionChangedEventArgs e)
    {
        Avalonia.Application? app = Avalonia.Application.Current;
        if (app is null)
        {
            return;
        }

        switch (ThemeSelect?.SelectedIndex)
        {
            case 0:
                app.RequestedThemeVariant = ThemeVariant.Light;
                break;
            case 1:
                app.RequestedThemeVariant = ThemeVariant.Dark;
                break;
        }
    }

    private async Task DoShowScalingWindowAsync(IInteractionContext<ScalingDialogViewModel, ResizeContext?> interaction)
    {
        var dialog = new Views.DataEntry.ScalingDialogView()
        {
            DataContext = interaction.Input
        };

        if (VisualRoot is Window currentWindow)
        {
            var result = await dialog.ShowDialog<ResizeContext?>(currentWindow);
            interaction.SetOutput(result);
        }
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

        if (VisualRoot is Window currentWindow)
        {
            _ = await dialog.ShowDialog<NewPictureWindowViewModel>(currentWindow);
            interaction.SetOutput(interaction.Input);
        }
    }
}
