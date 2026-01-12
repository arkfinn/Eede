using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Presentation.Common.Services;
using Eede.Presentation.ViewModels.Animations;
using Moq;

namespace Eede.Presentation.Tests.ViewModels.Animations;

public class AnimationDockViewModelTests
{
    [Test]
    public void ShouldInitializeWithCorrectProperties()
    {
        var mockService = new Mock<IAnimationService>();
        mockService.Setup(x => x.Patterns).Returns(new List<AnimationPattern>());
        var animationViewModel = new AnimationViewModel(mockService.Object, new Mock<IFileSystem>().Object);
        
        var dockViewModel = new AnimationDockViewModel(animationViewModel);

        Assert.That(dockViewModel.Id, Is.EqualTo("Animation"));
        Assert.That(dockViewModel.Title, Is.EqualTo("Animation"));
        Assert.That(dockViewModel.AnimationViewModel, Is.SameAs(animationViewModel));
    }
}
