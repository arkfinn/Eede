using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing;

[TestFixture]
public class CanvasInteractionSessionTests
{
    [Test]
    public void InitialState_ShouldBeIdle()
    {
        var picture = Picture.CreateEmpty(new PictureSize(32, 32));
        var buffer = new DrawingBuffer(picture);
        var session = new CanvasInteractionSession(buffer, new FreeCurve());

        Assert.That(session.Buffer.IsDrawing(), Is.False);
    }

    [Test]
    public void PointerBegin_ShouldStartDrawing()
    {
        var picture = Picture.CreateEmpty(new PictureSize(32, 32));
        var buffer = new DrawingBuffer(picture);
        var session = new CanvasInteractionSession(buffer, new FreeCurve());
        var penStyle = new PenStyle(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);

        var nextSession = session.PointerBegin(new CanvasCoordinate(10, 10), penStyle, false, null);

        Assert.That(nextSession.Buffer.IsDrawing(), Is.True);
    }
}