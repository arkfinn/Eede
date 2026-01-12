using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Eede.Presentation.Common.Services;
using Eede.Presentation.Files;

namespace Eede.Presentation.Views.Animations;

public partial class AnimationPreviewView : UserControl
{
    public static readonly StyledProperty<StorageService> StorageServiceProperty =
        AvaloniaProperty.Register<AnimationPreviewView, StorageService>(nameof(StorageService));

    public StorageService StorageService
    {
        get => GetValue(StorageServiceProperty);
        set => SetValue(StorageServiceProperty, value);
    }

    public AnimationPreviewView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
