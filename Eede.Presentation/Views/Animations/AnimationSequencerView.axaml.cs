using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Eede.Presentation.Views.Animations;

public partial class AnimationSequencerView : UserControl
{
    public AnimationSequencerView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
