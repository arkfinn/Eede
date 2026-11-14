using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ReactiveUI.Avalonia;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.Common.Enums;
using Eede.Views.Pages;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace Eede.Presentation.Views.DataEntry;

public partial class PaletteContainer : ReactiveUserControl<PaletteContainerViewModel>
{
    private ScrollViewer? _headerScrollViewer;

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

        this.Loaded += (s, e) =>
        {
            AttachScrollViewer();
        };
    }

    private void AttachScrollViewer()
    {
        if (_headerScrollViewer != null) return;

        var tabControl = this.FindControl<TabControl>("PART_TabControl");
        if (tabControl != null)
        {
            _headerScrollViewer = tabControl.FindControl<ScrollViewer>("PART_HeaderScrollViewer");
            if (_headerScrollViewer != null)
            {
                _headerScrollViewer.PointerWheelChanged += OnHeaderScrollViewerPointerWheelChanged;
            }
        }
    }

    private void OnHeaderScrollViewerPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_headerScrollViewer == null) return;
        double delta = e.Delta.Y != 0 ? e.Delta.Y : e.Delta.X;
        double newX = _headerScrollViewer.Offset.X - (delta * 50);
        _headerScrollViewer.Offset = new Vector(Math.Max(0, newX), _headerScrollViewer.Offset.Y);
        e.Handled = true;
    }

    private void OnTabControlSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is TabControl tc && tc.SelectedItem is PaletteTabViewModel selected)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var container = tc.ContainerFromItem(selected);
                if (container is Control c)
                {
                    c.BringIntoView();
                }
            });
        }
    }

    private void OnShowTabListClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not PaletteContainerViewModel vm || sender is not Control target) return;
        var menu = new ContextMenu();
        foreach (var tab in vm.Tabs)
        {
            var item = new MenuItem
            {
                Header = tab.Title,
                FontWeight = tab == vm.SelectedTab ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal,
                Command = vm.SelectTabCommand,
                CommandParameter = tab
            };
            menu.Items.Add(item);
        }
        menu.Open(target);
    }

    private async Task DoConfirmCloseAsync(IInteractionContext<PaletteTabViewModel, SaveAlertResult> interaction)
    {
        string displayName = string.IsNullOrEmpty(interaction.Input.FilePath)
            ? interaction.Input.CustomTitle
            : interaction.Input.FilePath;
        var result = await Views.Pages.MainView.ShowSaveAlertDialogAsync(displayName, this);
        interaction.SetOutput(result);
    }

    public static readonly StyledProperty<AvaloniaFileStorage> FileStorageProperty =
        AvaloniaProperty.Register<PaletteContainer, AvaloniaFileStorage>(nameof(FileStorage));

    public AvaloniaFileStorage FileStorage
    {
        get => GetValue(FileStorageProperty);
        set => SetValue(FileStorageProperty, value);
    }

    public static readonly StyledProperty<object?> SideContentProperty =
        AvaloniaProperty.Register<PaletteContainer, object?>(nameof(SideContent));

    public object? SideContent
    {
        get => GetValue(SideContentProperty);
        set => SetValue(SideContentProperty, value);
    }

    public static readonly StyledProperty<bool> IsRegisterModeProperty =
        AvaloniaProperty.Register<PaletteContainer, bool>(nameof(IsRegisterMode), false);

    public bool IsRegisterMode
    {
        get => GetValue(IsRegisterModeProperty);
        set => SetValue(IsRegisterModeProperty, value);
    }
}