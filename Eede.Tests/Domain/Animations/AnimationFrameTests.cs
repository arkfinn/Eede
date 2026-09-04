using Eede.Domain.Animations;
using NUnit.Framework;

namespace Eede.Domain.Tests.Animations;

[TestFixture]
public class AnimationFrameTests
{
    [Test]
    public void Constructor_AssignsPropertiesCorrectly()
    {
        var frame = new AnimationFrame(1, 100);

        Assert.That(frame.CellIndex, Is.EqualTo(1));
        Assert.That(frame.Duration, Is.EqualTo(100));
    }

    [Test]
    public void Equality_RecordsWithSameValuesAreEqual()
    {
        var frame1 = new AnimationFrame(2, 200);
        var frame2 = new AnimationFrame(2, 200);

        Assert.That(frame1, Is.EqualTo(frame2));
        Assert.That(frame1 == frame2, Is.True);
    }

    [Test]
    public void Inequality_RecordsWithDifferentValuesAreNotEqual()
    {
        var frame1 = new AnimationFrame(3, 300);
        var frame2 = new AnimationFrame(4, 300);
        var frame3 = new AnimationFrame(3, 400);

        Assert.That(frame1, Is.Not.EqualTo(frame2));
        Assert.That(frame1 != frame2, Is.True);

        Assert.That(frame1, Is.Not.EqualTo(frame3));
        Assert.That(frame1 != frame3, Is.True);
    }

    [Test]
    public void ValidateTest()
    {
        var validFrame = new AnimationFrame(0, 100);
        Assert.That(validFrame.Validate(), Is.True);

        var zeroDurationFrame = new AnimationFrame(0, 0);
        Assert.That(zeroDurationFrame.Validate(), Is.False, "Duration == 0 must be invalid to prevent infinite loop DoS.");

        var negativeDurationFrame = new AnimationFrame(0, -50);
        Assert.That(negativeDurationFrame.Validate(), Is.False);

        var negativeIndexFrame = new AnimationFrame(-1, 100);
        Assert.That(negativeIndexFrame.Validate(), Is.False);
    }
}
