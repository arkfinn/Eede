using Eede.Application.Animations;
using Eede.Application.UseCase.Animations;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Eede.Application.Tests.UseCase.Animations;

[TestFixture]
public class AddAnimationPatternUseCaseTests
{
    private Mock<IAnimationPatternsProvider> _providerMock = null!;
    private AnimationPatterns _patterns = null!;

    [SetUp]
    public void Setup()
    {
        _patterns = new AnimationPatterns();
        _providerMock = new Mock<IAnimationPatternsProvider>();
        _providerMock.Setup(p => p.Current).Returns(() => _patterns);
        _providerMock.Setup(p => p.Update(It.IsAny<AnimationPatterns>()))
            .Callback<AnimationPatterns>(p => _patterns = p);
    }

    private static AnimationPattern CreateDummyPattern(string name)
    {
        return new AnimationPattern(name, new List<AnimationFrame>(), new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0));
    }

    [Test]
    public void Execute_AddsPatternToProvider_WhenPatternIsValid()
    {
        var pattern = CreateDummyPattern("NewPattern");
        var useCase = new AddAnimationPatternUseCase(_providerMock.Object);

        useCase.Execute(pattern);

        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Once);
        Assert.That(_patterns.Items.Count, Is.EqualTo(1));
        Assert.That(_patterns.Items[0].Name, Is.EqualTo("NewPattern"));
    }

    [Test]
    public void Execute_ThrowsArgumentNullException_WhenPatternIsNull()
    {
        var useCase = new AddAnimationPatternUseCase(_providerMock.Object);

        Assert.Throws<ArgumentNullException>(() => useCase.Execute(null!));
        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Never);
    }
}
