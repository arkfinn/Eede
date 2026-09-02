#nullable enable
using System;
using Eede.Application.Recovery;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Tests.Application.Recovery;

[TestFixture]
public class PullContextTrackerTests
{
    [Test]
    public void InitialContext_ShouldBeNull()
    {
        var tracker = new PullContextTracker();
        Assert.That(tracker.CurrentContext, Is.Null);
    }

    [Test]
    public void SetPullContext_ValidValues_SetsCurrentContext()
    {
        var tracker = new PullContextTracker();
        var area = new PictureArea(new Position(10, 20), new PictureSize(32, 32));

        tracker.SetPullContext("doc-1", area);

        Assert.That(tracker.CurrentContext, Is.Not.Null);
        Assert.That(tracker.CurrentContext!.SourceDocumentId, Is.EqualTo("doc-1"));
        Assert.That(tracker.CurrentContext.SourceArea, Is.EqualTo(area));
    }

    [Test]
    public void SetPullContext_UpdateExisting_OverwritesCurrentContext()
    {
        var tracker = new PullContextTracker();
        var area1 = new PictureArea(new Position(0, 0), new PictureSize(16, 16));
        var area2 = new PictureArea(new Position(32, 64), new PictureSize(48, 48));

        tracker.SetPullContext("doc-1", area1);
        tracker.SetPullContext("doc-2", area2);

        Assert.That(tracker.CurrentContext, Is.Not.Null);
        Assert.That(tracker.CurrentContext!.SourceDocumentId, Is.EqualTo("doc-2"));
        Assert.That(tracker.CurrentContext.SourceArea, Is.EqualTo(area2));
    }

    [Test]
    public void ClearPullContext_ResetsCurrentContextToNull()
    {
        var tracker = new PullContextTracker();
        var area = new PictureArea(new Position(0, 0), new PictureSize(16, 16));

        tracker.SetPullContext("doc-1", area);
        Assert.That(tracker.CurrentContext, Is.Not.Null);

        tracker.ClearPullContext();
        Assert.That(tracker.CurrentContext, Is.Null);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void SetPullContext_InvalidDocumentId_ThrowsArgumentException(string? invalidDocId)
    {
        var tracker = new PullContextTracker();
        var area = new PictureArea(new Position(0, 0), new PictureSize(16, 16));

        Assert.Catch<ArgumentException>(() => tracker.SetPullContext(invalidDocId!, area));
    }
}
