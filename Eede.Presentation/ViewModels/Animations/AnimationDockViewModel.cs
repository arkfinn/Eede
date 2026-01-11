using Dock.Model.Avalonia.Controls;

namespace Eede.Presentation.ViewModels.Animations;

public class AnimationDockViewModel : Tool
{
    public AnimationViewModel AnimationViewModel { get; }

    public AnimationDockViewModel(AnimationViewModel animationViewModel)
    {
        AnimationViewModel = animationViewModel;
        Id = "Animation";
        Title = "Animation";
        CanClose = false;
        CanFloat = true;
        CanPin = true;
    }
}
