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
            InitializeFileStorage();

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
            InitializeFileStorage();

            // DragDropハンドラの登録
            AddHandler(DragDrop.DragOverEvent, ViewModel.DragOverPicture);
            AddHandler(DragDrop.DropEvent, ViewModel.DropPicture);
            Disposable.Create(() =>
            {
                RemoveHandler(DragDrop.DragOverEvent, ViewModel.DragOverPicture);
                RemoveHandler(DragDrop.DropEvent, ViewModel.DropPicture);
            }).DisposeWith(disposables);

            // Window依存の登録
            var window = TopLevel.GetTopLevel(this) as Window;
            if (window != null)
            {
                // ViewModelのInteractionを購読し、通知が来たらウィンドウを閉じる
                ViewModel.CloseWindowInteraction.RegisterHandler(interaction =>
                {
                    window.Close();
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);

                // WindowのClosingイベントを登録
                EventHandler<WindowClosingEventArgs> closingHandler = (s, args) =>
                {
                    if (ViewModel.IsCloseConfirmed) return;
                    args.Cancel = true;
                    ViewModel.RequestCloseCommand.Execute().Subscribe();
                };
                window.Closing += closingHandler;
                Disposable.Create(() => window.Closing -= closingHandler).DisposeWith(disposables);
            }
        });
    }

    public AvaloniaFileStorage? FileStorage { get; private set; }

    private CompositeDisposable? _visualTreeDisposables;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        InitializeFileStorage();

        var topLevelWindow = TopLevel.GetTopLevel(this) as Window;


        _visualTreeDisposables?.Dispose();
        _visualTreeDisposables = new CompositeDisposable();

        if (topLevelWindow is Window window)
        {
            // Window の Activated イベントが発生した際にクリップボードをチェックする
            EventHandler activatedHandler = (s, ev) =>
            {
                if (DataContext is MainViewModel viewModel)
                {
                    _ = viewModel.UpdateClipboardStatusAsync();
                }
            };
            window.Activated += activatedHandler;
            Disposable.Create(() => window.Activated -= activatedHandler).DisposeWith(_visualTreeDisposables);

            // マウスがウィンドウ内に入ったときにもクリップボードをチェックする
            EventHandler<PointerEventArgs> pointerEnteredHandler = (s, ev) =>
            {
                if (DataContext is MainViewModel viewModel)
                {
                    _ = viewModel.UpdateClipboardStatusAsync();
                }
            };
            this.PointerEntered += pointerEnteredHandler;
            Disposable.Create(() => this.PointerEntered -= pointerEnteredHandler).DisposeWith(_visualTreeDisposables);

            // フォーカスを得たときにもクリップボードをチェックする
            EventHandler<FocusChangedEventArgs> gotFocusHandler = (s, ev) =>
            {
                if (DataContext is MainViewModel viewModel)
                {
                    _ = viewModel.UpdateClipboardStatusAsync();
                }
            };
            this.GotFocus += gotFocusHandler;
            Disposable.Create(() => this.GotFocus -= gotFocusHandler).DisposeWith(_visualTreeDisposables);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _visualTreeDisposables?.Dispose();
        _visualTreeDisposables = null;
    }

    private void InitializeFileStorage()
    {
        if (FileStorage == null)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                FileStorage = new AvaloniaFileStorage(topLevel.StorageProvider);
            }
        }

        if (FileStorage != null && DataContext is MainViewModel viewModel)
        {
            viewModel.FileStorage = FileStorage;
        }
    }

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
