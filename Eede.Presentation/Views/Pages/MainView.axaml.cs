using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.ViewModels.DataEntry;
using Avalonia.Interactivity;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.GeometricTransformations;
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

        this.AddHandler(KeyDownEvent, OnGlobalKeyDown, RoutingStrategies.Tunnel);

        _ = this.WhenActivated(disposables =>
        {
            if (ViewModel == null) return;

            // ViewModelの初期化
            InitializeFileStorage();

            // モーダルダイアログの登録
            disposables.Add(ViewModel.ShowCreateNewPictureModal.RegisterHandler(DoShowCreateNewFileWindowAsync));
            disposables.Add(ViewModel.ShowScalingModal.RegisterHandler(DoShowScalingWindowAsync));

            // DragDropハンドラの登録
            AddHandler(DragDrop.DragOverEvent, ViewModel.DragOverPicture);
            AddHandler(DragDrop.DropEvent, ViewModel.DropPicture);
            disposables.Add(Disposable.Create(() =>
            {
                RemoveHandler(DragDrop.DragOverEvent, ViewModel.DragOverPicture);
                RemoveHandler(DragDrop.DropEvent, ViewModel.DropPicture);
            }));

            // Window依存の登録
            var window = TopLevel.GetTopLevel(this) as Window;
            if (window != null)
            {
                // ViewModelのInteractionを購読し、通知が来たらウィンドウを閉じる
                disposables.Add(ViewModel.CloseWindowInteraction.RegisterHandler(interaction =>
                {
                    window.Close();
                    interaction.SetOutput(Unit.Default);
                }));

                // WindowのClosingイベントを登録
                EventHandler<WindowClosingEventArgs> closingHandler = (s, args) =>
                {
                    if (ViewModel.IsCloseConfirmed) return;
                    args.Cancel = true;
                    ViewModel.RequestCloseCommand.Execute().Subscribe();
                };
                window.Closing += closingHandler;
                disposables.Add(Disposable.Create(() => window.Closing -= closingHandler));
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
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window window)
        {
            var dialog = new Views.DataEntry.ScalingDialogView()
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<ResizeContext?>(window);
            interaction.SetOutput(result);
        }
        else
        {
            // Web / SingleView 用インラインモーダル
            var tcs = new TaskCompletionSource<ResizeContext?>();
            var view = new Views.DataEntry.ScalingDialogContentView()
            {
                DataContext = interaction.Input
            };

            IDisposable? subOk = null;
            IDisposable? subCancel = null;

            void CloseOverlay(ResizeContext? result)
            {
                subOk?.Dispose();
                subCancel?.Dispose();
                ModalOverlayHost.IsVisible = false;
                ModalContentControl.Content = null;
                tcs.TrySetResult(result);
            }

            subOk = interaction.Input.OkCommand.Subscribe(ctx => CloseOverlay(ctx));
            subCancel = interaction.Input.CancelCommand.Subscribe(_ => CloseOverlay(null));

            ModalContentControl.Content = view;
            ModalOverlayHost.IsVisible = true;

            var result = await tcs.Task;
            interaction.SetOutput(result);
        }
    }

    private async Task DoShowCreateNewFileWindowAsync(IInteractionContext<NewPictureWindowViewModel, NewPictureWindowViewModel> interaction)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window window)
        {
            NewPictureWindow dialog = new()
            {
                DataContext = interaction.Input,
                Width = 300,
                Height = 350
            };
            interaction.Input.Close = new Action(dialog.Close);

            _ = await dialog.ShowDialog<NewPictureWindowViewModel>(window);
            interaction.SetOutput(interaction.Input);
        }
        else
        {
            // Web / SingleView 用インラインモーダル
            var tcs = new TaskCompletionSource<NewPictureWindowViewModel>();
            var view = new NewPictureView()
            {
                DataContext = interaction.Input
            };

            interaction.Input.Close = new Action(() =>
            {
                ModalOverlayHost.IsVisible = false;
                ModalContentControl.Content = null;
                tcs.TrySetResult(interaction.Input);
            });

            ModalContentControl.Content = view;
            ModalOverlayHost.IsVisible = true;

            var result = await tcs.Task;
            interaction.SetOutput(result);
        }
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
