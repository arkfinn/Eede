using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using Eede.Domain.Palettes;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.SelectionStates
{
    [TestFixture]
    public class ResizingStateCommitTests
    {
        [Test]
        public void Commit_IsEffectiveNoOp_ReturnsSameSessionInstance()
        {
            var originalPixels = Picture.CreateEmpty(new PictureSize(10, 10));
            var originalArea = new PictureArea(new Position(0, 0), new PictureSize(10, 10));
            var startPos = new Position(10, 10);
            var resampler = new NearestNeighborResampler();

            var state = new ResizingState(
                originalPixels,
                originalArea,
                startPos,
                SelectionHandle.BottomRight,
                resampler
            );

            var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(100, 100)));
            var blender = new DirectImageBlender();
            var backgroundColor = new ArgbColor(255, 255, 255, 255);

            var resultSession = state.Commit(session, blender, backgroundColor);

            Assert.That(resultSession, Is.SameAs(session), "Commit should return the exact same DrawingSession instance (no-op).");
            Assert.That(resultSession.CanUndo(), Is.False, "History should not have an item after no-op commit");
            Assert.That(resultSession.IsDrawing(), Is.False, "Session should not be in drawing state");
            Assert.That(resultSession.CurrentPreviewContent, Is.Null, "Preview content should be null");
        }

        [Test]
        public void Cancel_IsEffectiveNoOp_ReturnsSameSessionInstance()
        {
            var originalPixels = Picture.CreateEmpty(new PictureSize(10, 10));
            var originalArea = new PictureArea(new Position(0, 0), new PictureSize(10, 10));
            var startPos = new Position(10, 10);
            var resampler = new NearestNeighborResampler();

            var state = new ResizingState(
                originalPixels,
                originalArea,
                startPos,
                SelectionHandle.BottomRight,
                resampler
            );

            var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(100, 100)));

            var resultSession = state.Cancel(session);

            Assert.That(resultSession, Is.SameAs(session), "Cancel should return the exact same DrawingSession instance (no-op).");
            Assert.That(resultSession.CanUndo(), Is.False, "History should be empty because it was cancelled / no-op");
            Assert.That(resultSession.IsDrawing(), Is.False, "Session should not be in drawing state");
            Assert.That(resultSession.CurrentPreviewContent, Is.Null, "Preview content should be null");
        }
    }
}
