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
public class AnimationPatternUseCaseTests
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

    #region AddAnimationPatternUseCase

    [Test]
    public void Add_AddsPatternToProvider_WhenPatternIsValid()
    {
        var pattern = CreateDummyPattern("NewPattern");
        var useCase = new AddAnimationPatternUseCase(_providerMock.Object);

        useCase.Execute(pattern);

        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Once);
        Assert.That(_patterns.Items.Count, Is.EqualTo(1));
        Assert.That(_patterns.Items[0].Name, Is.EqualTo("NewPattern"));
    }

    [Test]
    public void Add_ThrowsArgumentNullException_WhenPatternIsNull()
    {
        var useCase = new AddAnimationPatternUseCase(_providerMock.Object);

        Assert.Throws<ArgumentNullException>(() => useCase.Execute(null!));
        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Never);
    }

    #endregion

    #region RemoveAnimationPatternUseCase

    [TestCase(0, "Pattern1", 2)]
    [TestCase(1, "Pattern0", 2)]
    public void Remove_RemovesPatternAtSpecifiedIndex_WhenIndexIsValid(int removeIndex, string remainingFirstName, int expectedRemainingCount)
    {
        var pattern0 = CreateDummyPattern("Pattern0");
        var pattern1 = CreateDummyPattern("Pattern1");
        var pattern2 = CreateDummyPattern("Pattern2");
        _patterns = _patterns.Add(pattern0).Add(pattern1).Add(pattern2);

        var useCase = new RemoveAnimationPatternUseCase(_providerMock.Object);
        useCase.Execute(removeIndex);

        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Once);
        Assert.That(_patterns.Items.Count, Is.EqualTo(expectedRemainingCount));
        Assert.That(_patterns.Items[0].Name, Is.EqualTo(remainingFirstName));
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void Remove_ThrowsArgumentOutOfRangeException_WhenIndexIsOutOfRange(int invalidIndex)
    {
        _patterns = _patterns.Add(CreateDummyPattern("Pattern1"));
        var useCase = new RemoveAnimationPatternUseCase(_providerMock.Object);

        Assert.Throws<ArgumentOutOfRangeException>(() => useCase.Execute(invalidIndex));
        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Never);
    }

    #endregion

    #region ReplaceAnimationPatternUseCase

    [TestCase(0, "Pattern0_Updated")]
    [TestCase(1, "Pattern1_Updated")]
    public void Replace_ReplacesPatternAtSpecifiedIndex_WhenIndexIsValid(int replaceIndex, string updatedName)
    {
        var pattern0 = CreateDummyPattern("Pattern0");
        var pattern1 = CreateDummyPattern("Pattern1");
        _patterns = _patterns.Add(pattern0).Add(pattern1);

        var updatedPattern = CreateDummyPattern(updatedName);
        var useCase = new ReplaceAnimationPatternUseCase(_providerMock.Object);

        useCase.Execute(replaceIndex, updatedPattern);

        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Once);
        Assert.That(_patterns.Items.Count, Is.EqualTo(2));
        Assert.That(_patterns.Items[replaceIndex].Name, Is.EqualTo(updatedName));
    }

    [TestCase(-1)]
    [TestCase(1)]
    public void Replace_ThrowsArgumentOutOfRangeException_WhenIndexIsOutOfRange(int invalidIndex)
    {
        _patterns = _patterns.Add(CreateDummyPattern("Pattern1"));
        var useCase = new ReplaceAnimationPatternUseCase(_providerMock.Object);
        var newPattern = CreateDummyPattern("NewPattern");

        Assert.Throws<ArgumentOutOfRangeException>(() => useCase.Execute(invalidIndex, newPattern));
        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Never);
    }

    [Test]
    public void Replace_ThrowsArgumentNullException_WhenPatternIsNull()
    {
        _patterns = _patterns.Add(CreateDummyPattern("Pattern1"));
        var useCase = new ReplaceAnimationPatternUseCase(_providerMock.Object);

        Assert.Throws<ArgumentNullException>(() => useCase.Execute(0, null!));
        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Never);
    }

    #endregion
}
