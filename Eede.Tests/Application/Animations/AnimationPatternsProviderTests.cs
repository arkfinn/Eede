using Eede.Application.Animations;
using Eede.Domain.Animations;
using NUnit.Framework;
using System;

namespace Eede.Application.Tests.Animations;

[TestFixture]
public class AnimationPatternsProviderTests
{
    [Test]
    public void Current_InitialState_IsEmpty()
    {
        // Arrange
        var provider = new AnimationPatternsProvider();

        // Act & Assert
        Assert.That(provider.Current, Is.Not.Null);
        Assert.That(provider.Current.Items.Count, Is.EqualTo(0));
    }

    [Test]
    public void Update_WithValidPatterns_UpdatesCurrentAndFiresChangedEvent()
    {
        // Arrange
        var provider = new AnimationPatternsProvider();
        AnimationPatterns? eventPattern = null;
        provider.Changed += (p) => eventPattern = p;

        var next = new AnimationPatterns();

        // Act
        provider.Update(next);

        // Assert
        Assert.That(object.ReferenceEquals(provider.Current, next), Is.True);
        Assert.That(object.ReferenceEquals(eventPattern, next), Is.True);
    }

    [Test]
    public void Update_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = new AnimationPatternsProvider();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => provider.Update(null!));
    }
}
