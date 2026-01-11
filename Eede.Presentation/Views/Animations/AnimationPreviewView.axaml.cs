using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Eede.Presentation.Views.Animations;

public partial class AnimationPreviewView : UserControl
{
    public AnimationPreviewView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
