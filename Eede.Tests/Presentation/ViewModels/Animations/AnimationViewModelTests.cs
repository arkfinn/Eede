using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Application.Infrastructure;
using Eede.Presentation.ViewModels.Animations;
using Eede.Application.UseCase.Animations;
using Eede.Presentation.Common.Adapters;
using Moq;
using NUnit.Framework;
using System;
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
        var patternEditor = new AnimationPatternEditor(
            new AddAnimationPatternUseCase(_patternsProvider),
            new ReplaceAnimationPatternUseCase(_patternsProvider),
            new RemoveAnimationPatternUseCase(_patternsProvider));
        _viewModel = new AnimationViewModel(
            _patternsProvider,
            patternEditor,
            _fileSystemMock.Object,
            new AvaloniaBitmapAdapter());
    }

    [Test]
    public void ShouldInitializeWithDefaultValues()
    {
        Assert.That(_viewModel.GridWidth, Is.EqualTo(32));
        Assert.That(_viewModel.GridHeight, Is.EqualTo(32));
        Assert.That(_viewModel.WaitTime, Is.EqualTo(100));
        Assert.That(_viewModel.Patterns.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddFrame_WhenNoPatterns_ShouldNotThrowException()
    {
        // First pattern is added automatically if Patterns is empty during initialization.
        // We simulate the removal of all patterns.
        _viewModel.RemovePatternCommand.Execute().Subscribe();

        Assert.That(_viewModel.Patterns.Count, Is.EqualTo(0), "Patterns collection should be empty.");
        Assert.That(_viewModel.SelectedPattern, Is.Null, "SelectedPattern should be null.");

        // Action under test
        // AddFrame in Eede.Presentation/ViewModels/Animations/AnimationViewModel.cs is public and takes an int.
        Assert.DoesNotThrow(() => _viewModel.AddFrame(0), "AddFrame should handle empty patterns gracefully.");
    }
}

