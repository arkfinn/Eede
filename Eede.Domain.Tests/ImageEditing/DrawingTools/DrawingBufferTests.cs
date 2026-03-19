using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools;

[TestFixture]
public class DrawingBufferTests
{
    private Picture _previousPicture;
    private Picture _drawingPicture;
    private Picture _resetPicture;

    [SetUp]
    public void Setup()
    {
        _previousPicture = Picture.CreateEmpty(new PictureSize(10, 10));
        _drawingPicture = Picture.CreateEmpty(new PictureSize(10, 10));
        _resetPicture = Picture.CreateEmpty(new PictureSize(10, 10));
    }

    [Test]
    public void Constructor_WithPreviousPicture_SetsIdleState()
    {
        var buffer = new DrawingBuffer(_previousPicture);

        Assert.Multiple(() =>
        {
            Assert.That(buffer.IsDrawing(), Is.False, "New buffer should be in Idle state.");
            Assert.That(buffer.Fetch(), Is.EqualTo(_previousPicture), "Fetch should return the previous picture when idle.");
            Assert.That(buffer.Previous, Is.EqualTo(_previousPicture), "Previous property should match the initialization picture.");
            Assert.That(buffer.Drawing, Is.Null, "Drawing property should be null when idle.");
        });
    }

    [Test]
    public void Constructor_WithNullPreviousPicture_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DrawingBuffer(null));
    }

    [Test]
    public void UpdateDrawing_WithNewPicture_TransitionsToActiveState()
    {
        var buffer = new DrawingBuffer(_previousPicture);
        var activeBuffer = buffer.UpdateDrawing(_drawingPicture);

        Assert.Multiple(() =>
        {
            Assert.That(activeBuffer.IsDrawing(), Is.True, "Buffer should be in Active state after UpdateDrawing.");
            Assert.That(activeBuffer.Fetch(), Is.EqualTo(_drawingPicture), "Fetch should return the drawing picture when active.");
            Assert.That(activeBuffer.Previous, Is.EqualTo(_previousPicture), "Previous property should remain the same.");
            Assert.That(activeBuffer.Drawing, Is.EqualTo(_drawingPicture), "Drawing property should match the updated picture.");
        });
    }

    [Test]
    public void UpdateDrawing_WithNullPicture_ThrowsArgumentNullException()
    {
        var buffer = new DrawingBuffer(_previousPicture);
        Assert.Throws<ArgumentNullException>(() => buffer.UpdateDrawing(null));
    }

    [Test]
    public void CancelDrawing_FromActiveState_TransitionsToIdleState()
    {
        var buffer = new DrawingBuffer(_previousPicture).UpdateDrawing(_drawingPicture);
        var cancelledBuffer = buffer.CancelDrawing();

        Assert.Multiple(() =>
        {
            Assert.That(cancelledBuffer.IsDrawing(), Is.False, "Buffer should return to Idle state after CancelDrawing.");
            Assert.That(cancelledBuffer.Fetch(), Is.EqualTo(_previousPicture), "Fetch should return the previous picture after cancellation.");
            Assert.That(cancelledBuffer.Previous, Is.EqualTo(_previousPicture), "Previous property should remain the same.");
            Assert.That(cancelledBuffer.Drawing, Is.Null, "Drawing property should be null after cancellation.");
        });
    }

    [Test]
    public void Reset_WithNewPicture_CreatesNewIdleState()
    {
        var buffer = new DrawingBuffer(_previousPicture).UpdateDrawing(_drawingPicture);
        var resetBuffer = buffer.Reset(_resetPicture);

        Assert.Multiple(() =>
        {
            Assert.That(resetBuffer.IsDrawing(), Is.False, "Buffer should be in Idle state after Reset.");
            Assert.That(resetBuffer.Fetch(), Is.EqualTo(_resetPicture), "Fetch should return the new reset picture.");
            Assert.That(resetBuffer.Previous, Is.EqualTo(_resetPicture), "Previous property should match the new reset picture.");
            Assert.That(resetBuffer.Drawing, Is.Null, "Drawing property should be null after reset.");
        });
    }

    [Test]
    public void Clone_OnIdleState_PreservesState()
    {
        var buffer = new DrawingBuffer(_previousPicture);
        var clonedBuffer = buffer.Clone();

        Assert.Multiple(() =>
        {
            Assert.That(clonedBuffer, Is.Not.SameAs(buffer), "Clone should return a new instance.");
            Assert.That(clonedBuffer.IsDrawing(), Is.EqualTo(buffer.IsDrawing()), "IsDrawing state should match.");
            Assert.That(clonedBuffer.Fetch(), Is.EqualTo(buffer.Fetch()), "Fetch result should match.");
            Assert.That(clonedBuffer.Previous, Is.EqualTo(buffer.Previous), "Previous property should match.");
            Assert.That(clonedBuffer.Drawing, Is.EqualTo(buffer.Drawing), "Drawing property should match.");
        });
    }

    [Test]
    public void Clone_OnActiveState_PreservesState()
    {
        var buffer = new DrawingBuffer(_previousPicture).UpdateDrawing(_drawingPicture);
        var clonedBuffer = buffer.Clone();

        Assert.Multiple(() =>
        {
            Assert.That(clonedBuffer, Is.Not.SameAs(buffer), "Clone should return a new instance.");
            Assert.That(clonedBuffer.IsDrawing(), Is.EqualTo(buffer.IsDrawing()), "IsDrawing state should match.");
            Assert.That(clonedBuffer.Fetch(), Is.EqualTo(buffer.Fetch()), "Fetch result should match.");
            Assert.That(clonedBuffer.Previous, Is.EqualTo(buffer.Previous), "Previous property should match.");
            Assert.That(clonedBuffer.Drawing, Is.EqualTo(buffer.Drawing), "Drawing property should match.");
        });
    }
}
