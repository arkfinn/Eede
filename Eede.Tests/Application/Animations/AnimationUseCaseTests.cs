using Eede.Application.Animations;
using Eede.Application.UseCase.Animations;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Eede.Application.Tests.Animations;

[TestFixture]
public class AnimationUseCaseTests
{
    private Mock<IAnimationPatternsProvider> _providerMock;
    private AnimationPatterns _patterns;

    [SetUp]
    public void Setup()
    {
        _patterns = new AnimationPatterns();
        _providerMock = new Mock<IAnimationPatternsProvider>();
        _providerMock.Setup(p => p.Current).Returns(() => _patterns);
        _providerMock.Setup(p => p.Update(It.IsAny<AnimationPatterns>()))
            .Callback<AnimationPatterns>(p => _patterns = p);
    }

    [Test]
    public void AddAnimationPatternUseCaseTest()
    {
        var useCase = new AddAnimationPatternUseCase(_providerMock.Object);
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>(), new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0));

        useCase.Execute(pattern);

        _providerMock.Verify(p => p.Update(It.IsAny<AnimationPatterns>()), Times.Once);
        Assert.That(_patterns.Items.Count, Is.EqualTo(1));
    }

    [Test]
    public void ReplaceAnimationPatternUseCaseTest()
    {
        var pattern1 = new AnimationPattern("Test1", new List<AnimationFrame>(), new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0));
        var pattern2 = new AnimationPattern("Test2", new List<AnimationFrame>(), new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0));
        _patterns = _patterns.Add(pattern1);

        var useCase = new ReplaceAnimationPatternUseCase(_providerMock.Object);
        useCase.Execute(0, pattern2);

        Assert.That(_patterns.Items[0].Name, Is.EqualTo("Test2"));
    }

    [Test]
    public void RemoveAnimationPatternUseCaseTest()
    {
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>(), new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0));
        _patterns = _patterns.Add(pattern);

        var useCase = new RemoveAnimationPatternUseCase(_providerMock.Object);
        useCase.Execute(0);

        Assert.That(_patterns.Items.Count, Is.EqualTo(0));
    }
}
