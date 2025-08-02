using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Eede.Presentation.Common.Services;
using Eede.Presentation.ViewModels.Pages;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Eede.Presentation.Views.Pages;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
        StorageService = new StorageService(new Window().StorageProvider);

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

            viewModel.StorageService = StorageService;
        };

        this.WhenActivated(disposables =>
        {
            if (TopLevel.GetTopLevel(this) is not Window window) return;

            // ViewModelのInteractionを購読し、通知が来たらウィンドウを閉じる
            ViewModel.CloseWindowInteraction.RegisterHandler(interaction =>
            {
                window.Close();
                interaction.SetOutput(Unit.Default);
            }).DisposeWith(disposables);


            // WindowのClosingイベントをObservableに変換
            Observable.FromEventPattern<EventHandler<WindowClosingEventArgs>, WindowClosingEventArgs>(
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
                    ViewModel.RequestCloseCommand.Execute().Subscribe();

                }).DisposeWith(disposables);
        });
    }

    public StorageService StorageService { get; private set; }

    public void OnClickThemeSelect(object sender, SelectionChangedEventArgs e)
    {
        Avalonia.Application app = Avalonia.Application.Current;
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

    private async Task DoShowCreateNewFileWindowAsync(IInteractionContext<NewPictureWindowViewModel, NewPictureWindowViewModel> interaction)
    {
        NewPictureWindow dialog = new()
        {
            DataContext = interaction.Input,
            Width = 300,
            Height = 350
        };
        interaction.Input.Close = new Action(dialog.Close);

        Window currentWindow = (Window)VisualRoot;
        _ = await dialog.ShowDialog<NewPictureWindowViewModel>(currentWindow);
        interaction.SetOutput(interaction.Input);
    }
}
