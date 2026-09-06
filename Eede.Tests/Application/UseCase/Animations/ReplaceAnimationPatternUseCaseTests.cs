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
public class ReplaceAnimationPatternUseCaseTests
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
    public void Execute_ReplacesPatternAtSpecifiedIndex_WhenIndexIsValid()
    {
        var pattern1 = CreateDummyPattern("Pattern1");
        var pattern2 = CreateDummyPattern("Pattern2");
        _patterns = _patterns.Add(pattern1).Add(pattern2);

        var updatedPattern = CreateDummyPattern("Pattern2_Updated");
        var useCase = new ReplaceAnimationPatternUseCase(_providerMock.Object);

        useCase.Execute(1, updatedPattern);

        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Once);
        Assert.That(_patterns.Items.Count, Is.EqualTo(2));
        Assert.That(_patterns.Items[0].Name, Is.EqualTo("Pattern1"));
        Assert.That(_patterns.Items[1].Name, Is.EqualTo("Pattern2_Updated"));
    }

    [Test]
    public void Execute_ReplacesFirstPattern_WhenIndexIsZero()
    {
        var pattern0 = CreateDummyPattern("Pattern0");
        var pattern1 = CreateDummyPattern("Pattern1");
        var pattern2 = CreateDummyPattern("Pattern2");
        _patterns = _patterns.Add(pattern0).Add(pattern1).Add(pattern2);

        var updatedPattern = CreateDummyPattern("Pattern0_Updated");
        var useCase = new ReplaceAnimationPatternUseCase(_providerMock.Object);

        useCase.Execute(0, updatedPattern);

        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Once);
        Assert.That(_patterns.Items.Count, Is.EqualTo(3));
        Assert.That(_patterns.Items[0].Name, Is.EqualTo("Pattern0_Updated"));
        Assert.That(_patterns.Items[1].Name, Is.EqualTo("Pattern1"));
        Assert.That(_patterns.Items[2].Name, Is.EqualTo("Pattern2"));
    }

    [Test]
    public void Execute_ThrowsArgumentOutOfRangeException_WhenIndexIsNegative()
    {
        var pattern = CreateDummyPattern("Pattern1");
        _patterns = _patterns.Add(pattern);

        var useCase = new ReplaceAnimationPatternUseCase(_providerMock.Object);
        var newPattern = CreateDummyPattern("NewPattern");

        Assert.Throws<ArgumentOutOfRangeException>(() => useCase.Execute(-1, newPattern));
        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Never);
    }

    [Test]
    public void Execute_ThrowsArgumentOutOfRangeException_WhenIndexIsGreaterThanOrEqualToCount()
    {
        var pattern = CreateDummyPattern("Pattern1");
        _patterns = _patterns.Add(pattern);

        var useCase = new ReplaceAnimationPatternUseCase(_providerMock.Object);
        var newPattern = CreateDummyPattern("NewPattern");

        Assert.Throws<ArgumentOutOfRangeException>(() => useCase.Execute(1, newPattern));
        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Never);
    }

    [Test]
    public void Execute_ThrowsArgumentNullException_WhenPatternIsNull()
    {
        var pattern = CreateDummyPattern("Pattern1");
        _patterns = _patterns.Add(pattern);

        var useCase = new ReplaceAnimationPatternUseCase(_providerMock.Object);

        Assert.Throws<ArgumentNullException>(() => useCase.Execute(0, null!));
        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Never);
    }
}
