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
public class RemoveAnimationPatternUseCaseTests
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
    public void Execute_RemovesPatternAtSpecifiedIndex_WhenIndexIsValid()
    {
        var pattern0 = CreateDummyPattern("Pattern0");
        var pattern1 = CreateDummyPattern("Pattern1");
        var pattern2 = CreateDummyPattern("Pattern2");
        _patterns = _patterns.Add(pattern0).Add(pattern1).Add(pattern2);

        var useCase = new RemoveAnimationPatternUseCase(_providerMock.Object);

        useCase.Execute(1);

        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Once);
        Assert.That(_patterns.Items.Count, Is.EqualTo(2));
        Assert.That(_patterns.Items[0].Name, Is.EqualTo("Pattern0"));
        Assert.That(_patterns.Items[1].Name, Is.EqualTo("Pattern2"));
    }

    [Test]
    public void Execute_RemovesFirstPattern_WhenIndexIsZero()
    {
        var pattern0 = CreateDummyPattern("Pattern0");
        var pattern1 = CreateDummyPattern("Pattern1");
        _patterns = _patterns.Add(pattern0).Add(pattern1);

        var useCase = new RemoveAnimationPatternUseCase(_providerMock.Object);

        useCase.Execute(0);

        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Once);
        Assert.That(_patterns.Items.Count, Is.EqualTo(1));
        Assert.That(_patterns.Items[0].Name, Is.EqualTo("Pattern1"));
    }

    [Test]
    public void Execute_ThrowsArgumentOutOfRangeException_WhenIndexIsNegative()
    {
        var pattern = CreateDummyPattern("Pattern1");
        _patterns = _patterns.Add(pattern);

        var useCase = new RemoveAnimationPatternUseCase(_providerMock.Object);

        Assert.Throws<ArgumentOutOfRangeException>(() => useCase.Execute(-1));
        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Never);
    }

    [Test]
    public void Execute_ThrowsArgumentOutOfRangeException_WhenIndexIsGreaterThanOrEqualToCount()
    {
        var pattern = CreateDummyPattern("Pattern1");
        _patterns = _patterns.Add(pattern);

        var useCase = new RemoveAnimationPatternUseCase(_providerMock.Object);

        Assert.Throws<ArgumentOutOfRangeException>(() => useCase.Execute(1));
        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Never);
    }
}
