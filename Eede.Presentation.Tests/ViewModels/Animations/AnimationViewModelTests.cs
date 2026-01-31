using Eede.Application.Animations;
using Eede.Application.UseCase.Animations;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Services;
using Eede.Presentation.ViewModels.Animations;
using Moq;
using NUnit.Framework;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace Eede.Presentation.Tests.ViewModels.Animations;

[TestFixture]
public class AnimationViewModelTests
{
    private AnimationPatternsProvider _patternsProvider;
    private Mock<IFileSystem> _fileSystemMock;

    [SetUp]
    public void Setup()
    {
        _patternsProvider = new AnimationPatternsProvider();
        _fileSystemMock = new Mock<IFileSystem>();
    }

    private AnimationViewModel CreateViewModel()
    {
        return new AnimationViewModel(
            _patternsProvider,
            new AddAnimationPatternUseCase(_patternsProvider),
            new ReplaceAnimationPatternUseCase(_patternsProvider),
            new RemoveAnimationPatternUseCase(_patternsProvider),
            _fileSystemMock.Object);
    }

    [Test]
    public void ConstructorTest()
    {
        var vm = CreateViewModel();
        Assert.That(vm, Is.Not.Null);
    }
}