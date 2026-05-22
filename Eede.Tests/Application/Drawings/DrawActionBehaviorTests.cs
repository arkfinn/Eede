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
    public class DrawActionBehaviorTests
    {
        [Test]
        public void DrawSinglePixelTest()
        {
            var useCase = new DrawActionUseCase();
            var picture = Picture.CreateEmpty(new PictureSize(10, 10));
            var session = new DrawingSession(picture);
            var color = new ArgbColor(255, 255, 0, 0); // Red
            var tool = new DrawingTool(new FreeCurve(), new PenStyle(new DirectImageBlender(), color, 1));
            var mag = new Magnification(1.0f);

            // (5, 5) に描画
            session = useCase.DrawStart(session, tool, new DisplayCoordinate(5, 5), mag, false);
            session = useCase.DrawEnd(session, tool, new DisplayCoordinate(5, 5), mag, false);

            var resultColor = session.CurrentPicture.PickColor(new Position(5, 5));
            Assert.That(resultColor, Is.EqualTo(color), "描画したピクセルの色が一致すること");

            var otherColor = session.CurrentPicture.PickColor(new Position(0, 0));
            Assert.That(otherColor.Alpha, Is.EqualTo(0), "描画していない場所は透明であること");
        }

        [Test]
        public void DrawLineTest()
        {
            var useCase = new DrawActionUseCase();
            var picture = Picture.CreateEmpty(new PictureSize(10, 10));
            var session = new DrawingSession(picture);
            var color = new ArgbColor(255, 0, 0, 255); // Blue
            var tool = new DrawingTool(new FreeCurve(), new PenStyle(new DirectImageBlender(), color, 1));
            var mag = new Magnification(1.0f);

            // (1, 1) から (3, 3) へ線を引く
            session = useCase.DrawStart(session, tool, new DisplayCoordinate(1, 1), mag, false);
            session = useCase.Drawing(session, tool, new DisplayCoordinate(2, 2), mag, false);
            session = useCase.Drawing(session, tool, new DisplayCoordinate(3, 3), mag, false);
            session = useCase.DrawEnd(session, tool, new DisplayCoordinate(3, 3), mag, false);

            Assert.That(session.CurrentPicture.PickColor(new Position(1, 1)), Is.EqualTo(color));
            Assert.That(session.CurrentPicture.PickColor(new Position(2, 2)), Is.EqualTo(color));
            Assert.That(session.CurrentPicture.PickColor(new Position(3, 3)), Is.EqualTo(color));
        }
    }
}
