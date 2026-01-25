using Eede.Application.Drawings;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Application.Tests.Drawings
{
    [TestFixture]
    public class DrawActionUseCaseTests
    {
        [Test]
        public void DrawCycleTest()
        {
            var useCase = new DrawActionUseCase();
            var picture = Picture.CreateEmpty(new PictureSize(10, 10));
            var session = new DrawingSession(picture);
            var tool = new DrawingTool(new FreeCurve(), new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1));
            var mag = new Magnification(1.0f);

            // Start
            session = useCase.DrawStart(session, tool, new DisplayCoordinate(0, 0), mag, false);
            Assert.That(session.IsDrawing(), Is.True);

            // Drawing
            session = useCase.Drawing(session, tool, new DisplayCoordinate(1, 1), mag, false);
            Assert.That(session.IsDrawing(), Is.True);

            // End
            session = useCase.DrawEnd(session, tool, new DisplayCoordinate(2, 2), mag, false);
            Assert.That(session.IsDrawing(), Is.False);
            Assert.That(session.CanUndo(), Is.True);
        }
    }
}
