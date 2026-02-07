using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Application.Infrastructure;
using Eede.Presentation.ViewModels.Animations;
using Eede.Application.UseCase.Animations;
using Eede.Presentation.Common.Adapters;
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
        var patternService = new AnimationPatternService(
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider));
        var animationViewModel = new AnimationViewModel(
            patternsProvider,
            patternService,
            new Mock<IFileSystem>().Object,
            new AvaloniaBitmapAdapter());
        
        var dockViewModel = new AnimationDockViewModel(animationViewModel);

        Assert.That(dockViewModel.Id, Is.EqualTo("Animation"));
        Assert.That(dockViewModel.Title, Is.EqualTo("Animation"));
        Assert.That(dockViewModel.AnimationViewModel, Is.SameAs(animationViewModel));
    }
}
