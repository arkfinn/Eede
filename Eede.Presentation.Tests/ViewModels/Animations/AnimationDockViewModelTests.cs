using Eede.Presentation.Common.Services;
using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Application.Infrastructure;
using Eede.Presentation.ViewModels.Animations;
using Eede.Application.UseCase.Animations;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Eede.Presentation.Tests.ViewModels.Animations;

public class AnimationDockViewModelTests
{
    [Test]
    public void ShouldInitializeWithCorrectProperties()
    {
        var patternsProvider = new AnimationPatternsProvider();
        var animationViewModel = new AnimationViewModel(
            patternsProvider,
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider),
            new Mock<IFileSystem>().Object);
        
        var dockViewModel = new AnimationDockViewModel(animationViewModel);

        Assert.That(dockViewModel.Id, Is.EqualTo("Animation"));
        Assert.That(dockViewModel.Title, Is.EqualTo("Animation"));
        Assert.That(dockViewModel.AnimationViewModel, Is.SameAs(animationViewModel));
    }
}
