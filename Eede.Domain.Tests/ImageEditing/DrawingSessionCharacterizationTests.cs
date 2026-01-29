using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing;

[TestFixture]
public class DrawingSessionCharacterizationTests
{
    private Picture _initialPicture;
    private PictureSize _size = new(32, 32);

    [SetUp]
    public void Setup()
    {
        _initialPicture = Picture.CreateEmpty(_size);
    }

    [Test]
    public void Push_TransitionsCurrentPicture_And_EnablesUndo()
    {
        var session = new DrawingSession(_initialPicture);
        var nextPicture = Picture.CreateEmpty(_size); // In reality, this would be different data

        var s2 = session.Push(nextPicture);

        Assert.That(s2.CurrentPicture, Is.EqualTo(nextPicture));
        Assert.That(s2.PreviousPicture, Is.EqualTo(nextPicture));
        Assert.That(s2.CanUndo(), Is.True);
        Assert.That(s2.CanRedo(), Is.False);
    }

    [Test]
    public void Undo_RestoresPreviousPicture_And_EnablesRedo()
    {
        var session = new DrawingSession(_initialPicture);
        var nextPicture = Picture.CreateEmpty(_size);
        var s2 = session.Push(nextPicture);

        var s3 = s2.Undo().Session;

        Assert.That(s3.CurrentPicture, Is.EqualTo(_initialPicture));
        Assert.That(s3.CanUndo(), Is.False);
        Assert.That(s3.CanRedo(), Is.True);
    }

    [Test]
    public void Redo_RestoresNextPicture_And_EnablesUndo()
    {
        var session = new DrawingSession(_initialPicture);
        var nextPicture = Picture.CreateEmpty(_size);
        var s2 = session.Push(nextPicture);
        var s3 = s2.Undo().Session;

        var s4 = s3.Redo().Session;

        Assert.That(s4.CurrentPicture, Is.EqualTo(nextPicture));
        Assert.That(s4.CanUndo(), Is.True);
        Assert.That(s4.CanRedo(), Is.False);
    }

    [Test]
    public void Push_WithSelectingArea_RestoresSelectingAreaOnUndo()
    {
        // Scenario:
        // 1. Initial State: Empty Picture, No Selection
        // 2. Select an area (updates SelectingArea but not history yet if just updating area? No, Push stores previous area)
        // Wait, let's check Push signature: Push(nextPicture, nextArea, previousArea)
        // If previousArea is null, it uses session.SelectingArea.

        var initialArea = new PictureArea(new Position(0, 0), new PictureSize(10, 10));
        var session = new DrawingSession(_initialPicture, initialArea);

        var nextPicture = Picture.CreateEmpty(_size);
        var nextArea = new PictureArea(new Position(5, 5), new PictureSize(5, 5));

        // Push new state
        var s2 = session.Push(nextPicture, nextArea);

        Assert.That(s2.CurrentPicture, Is.EqualTo(nextPicture));
        Assert.That(s2.CurrentSelectingArea, Is.EqualTo(nextArea));

        // Undo
        var s3 = s2.Undo().Session;

        Assert.That(s3.CurrentPicture, Is.EqualTo(_initialPicture));
        Assert.That(s3.CurrentSelectingArea, Is.EqualTo(initialArea));
    }

    [Test]
    public void MultiplePushes_BehaveAsStack()
    {
        var session = new DrawingSession(_initialPicture);
        
        var p1 = Picture.CreateEmpty(_size);
        var p2 = Picture.CreateEmpty(_size);
        var p3 = Picture.CreateEmpty(_size);

        var s1 = session.Push(p1);
        var s2 = s1.Push(p2);
        var s3 = s2.Push(p3);

        Assert.That(s3.CurrentPicture, Is.EqualTo(p3));
        
        var u1 = s3.Undo().Session;
        Assert.That(u1.CurrentPicture, Is.EqualTo(p2));

        var u2 = u1.Undo().Session;
        Assert.That(u2.CurrentPicture, Is.EqualTo(p1));

        var u3 = u2.Undo().Session;
        Assert.That(u3.CurrentPicture, Is.EqualTo(_initialPicture));
        
        Assert.That(u3.CanUndo(), Is.False);
    }
}
