using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
using ReactiveUI.Avalonia;
using System.Reactive.Disposables.Fluent;

namespace Eede.Presentation.Views.Pages;

#nullable enable

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();

        DataContextChanged += (sender, e) =>
        {
            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }
            if (FileStorage == null)
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    FileStorage = new AvaloniaFileStorage(topLevel.StorageProvider);
                    viewModel.FileStorage = FileStorage;
                }
            }
            else
            {
                viewModel.FileStorage = FileStorage;
            }

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
            if (ViewModel == null) return;

            // ViewModelの初期化
            if (FileStorage == null)
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    FileStorage = new AvaloniaFileStorage(topLevel.StorageProvider);
                    ViewModel.FileStorage = FileStorage;
                }
            }
            else
            {
                ViewModel.FileStorage = FileStorage;
            }

            // DragDropハンドラの登録
            AddHandler(DragDrop.DragOverEvent, ViewModel.DragOverPicture);
            AddHandler(DragDrop.DropEvent, ViewModel.DropPicture);
            Disposable.Create(() =>
            {
                RemoveHandler(DragDrop.DragOverEvent, ViewModel.DragOverPicture);
                RemoveHandler(DragDrop.DropEvent, ViewModel.DropPicture);
            }).DisposeWith(disposables);

            // Window依存の登録
            this.WhenAnyValue(x => x.VisualRoot)
                .Where(vr => vr is Window)
                .Cast<Window>()
                .Subscribe(window =>
                {
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
                            if (ViewModel.IsCloseConfirmed) return;
                            args.EventArgs.Cancel = true;
                            ViewModel.RequestCloseCommand.Execute().Subscribe();
                        }).DisposeWith(disposables);
                }).DisposeWith(disposables);
        });
    }

    public AvaloniaFileStorage? FileStorage { get; private set; }

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
}
