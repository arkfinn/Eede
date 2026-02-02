using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Application.Infrastructure;
using Eede.Presentation.ViewModels.Animations;
using Eede.Application.UseCase.Animations;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Eede.Presentation.Tests.ViewModels.Animations;

public class AnimationViewModelTests
{
    private Mock<IFileSystem> _fileSystemMock;
    private AnimationViewModel _viewModel;
    private AnimationPatternsProvider _patternsProvider;

    [SetUp]
    public void SetUp()
    {
        _fileSystemMock = new Mock<IFileSystem>();
        _patternsProvider = new AnimationPatternsProvider();
        var patternService = new AnimationPatternService(
            new AddAnimationPatternUseCase(_patternsProvider),
            new ReplaceAnimationPatternUseCase(_patternsProvider),
            new RemoveAnimationPatternUseCase(_patternsProvider));
        _viewModel = new AnimationViewModel(
            _patternsProvider,
            patternService,
            _fileSystemMock.Object);
    }

    [Test]
    public void ShouldInitializeWithDefaultValues()
    {
        Assert.That(_viewModel.GridWidth, Is.EqualTo(32));
        Assert.That(_viewModel.GridHeight, Is.EqualTo(32));
        Assert.That(_viewModel.WaitTime, Is.EqualTo(100));
        Assert.That(_viewModel.Patterns.Count, Is.EqualTo(1));
    }
}
