using Eede.Application.Animations;
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
    private Mock<IAnimationService> _animationServiceMock;
    private Mock<IFileSystem> _fileSystemMock;

    [SetUp]
    public void Setup()
    {
        _animationServiceMock = new Mock<IAnimationService>();
        _animationServiceMock.Setup(s => s.Patterns).Returns(new List<AnimationPattern>());
        _fileSystemMock = new Mock<IFileSystem>();
    }

    [Test]
    public void ConstructorTest()
    {
        var vm = new AnimationViewModel(_animationServiceMock.Object, _fileSystemMock.Object);
        Assert.That(vm, Is.Not.Null);
    }
}
