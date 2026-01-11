using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Eede.Presentation.ViewModels.Animations;

namespace Eede.Presentation.Views.Animations;

public partial class AnimationSequencerView : UserControl
{
    public AnimationViewModel? ViewModel => DataContext as AnimationViewModel;

    public AnimationSequencerView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
