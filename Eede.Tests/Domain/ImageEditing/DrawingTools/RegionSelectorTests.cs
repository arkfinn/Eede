using System;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using Eede.Domain.Palettes;
using Eede.Domain.ImageEditing.Blending;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools;

[TestFixture]
public class RegionSelectorTests
{
    private Picture CreateDummyPicture()
    {
        return Picture.CreateEmpty(new PictureSize(10, 10));
    }

    private PenStyle CreatePenStyle()
    {
        return new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
    }

    [Test]
    public void DrawStart_ReturnsUpdatedBufferWithPrevious_AndFiresEvent()
    {
        var selector = new RegionSelector();
        var previousPicture = CreateDummyPicture();
        var buffer = new DrawingBuffer(previousPicture);
        var penStyle = CreatePenStyle();
        var coordinateHistory = new CoordinateHistory(new CanvasCoordinate(5, 5));
        var isShift = false;

        PositionHistory? eventArgs = null;
        selector.OnDrawStart += (sender, args) =>
        {
            eventArgs = args;
        };

        var resultBuffer = selector.DrawStart(buffer, penStyle, coordinateHistory, isShift);

        Assert.That(resultBuffer.IsDrawing(), Is.True);
        Assert.That(resultBuffer.Fetch(), Is.EqualTo(previousPicture));
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.Start.X, Is.EqualTo(5));
        Assert.That(eventArgs.Start.Y, Is.EqualTo(5));
    }

    [Test]
    public void Drawing_ReturnsSameBuffer_AndFiresEvent()
    {
        var selector = new RegionSelector();
        var previousPicture = CreateDummyPicture();
        var buffer = new DrawingBuffer(previousPicture);
        var penStyle = CreatePenStyle();
        var coordinateHistory = new CoordinateHistory(new CanvasCoordinate(5, 5));
        var isShift = false;

        var startBuffer = selector.DrawStart(buffer, penStyle, coordinateHistory, isShift);

        var drawCoordinateHistory = coordinateHistory.Update(new CanvasCoordinate(6, 6));

        PositionHistory? eventArgs = null;
        selector.OnDrawing += (sender, args) =>
        {
            eventArgs = args;
        };

        var resultBuffer = selector.Drawing(startBuffer, penStyle, drawCoordinateHistory, isShift);

        Assert.That(resultBuffer, Is.SameAs(startBuffer));
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.Start.X, Is.EqualTo(5));
        Assert.That(eventArgs.Start.Y, Is.EqualTo(5));
        Assert.That(eventArgs.Now.X, Is.EqualTo(6));
        Assert.That(eventArgs.Now.Y, Is.EqualTo(6));
    }

    [Test]
    public void DrawEnd_ReturnsCancelledBuffer_AndFiresEvent()
    {
        var selector = new RegionSelector();
        var previousPicture = CreateDummyPicture();
        var buffer = new DrawingBuffer(previousPicture);
        var penStyle = CreatePenStyle();
        var coordinateHistory = new CoordinateHistory(new CanvasCoordinate(5, 5));
        var isShift = false;

        var startBuffer = selector.DrawStart(buffer, penStyle, coordinateHistory, isShift);
        var endCoordinateHistory = coordinateHistory.Update(new CanvasCoordinate(6, 6));

        PositionHistory? eventArgs = null;
        selector.OnDrawEnd += (sender, args) =>
        {
            eventArgs = args;
        };

        var result = selector.DrawEnd(startBuffer, penStyle, endCoordinateHistory, isShift);

        Assert.That(result.Buffer.IsDrawing(), Is.False);
        Assert.That(result.AffectedArea, Is.EqualTo(default(PictureRegion)));
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.Start.X, Is.EqualTo(5));
        Assert.That(eventArgs.Start.Y, Is.EqualTo(5));
        Assert.That(eventArgs.Now.X, Is.EqualTo(6));
        Assert.That(eventArgs.Now.Y, Is.EqualTo(6));
    }
}
