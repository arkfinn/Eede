using Avalonia;
using Avalonia.Controls;
using Eede.Presentation.Common.Adapters;

namespace Eede.Presentation.Views.Animations;

public partial class AnimationPreviewView : UserControl
{
    public AnimationPreviewView()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<AvaloniaFileStorage> FileStorageProperty =
        AvaloniaProperty.Register<AnimationPreviewView, AvaloniaFileStorage>(nameof(FileStorage));

    public AvaloniaFileStorage FileStorage
    {
        get => GetValue(FileStorageProperty);
        set => SetValue(FileStorageProperty, value);
    }
}
