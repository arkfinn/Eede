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
        public void Commit_WithDirectImageBlender_CommitsPreviewAndUpdatesHistory()
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

            // Drag to (20, 20) -> Size becomes 20x20
            state.HandlePointerMoved(HalfBoxArea.Create(startPos, new PictureSize(2, 2)), true, new Position(20, 20), false, new PictureSize(100, 100));

            var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(100, 100)));
            var blender = new DirectImageBlender();
            var backgroundColor = new ArgbColor(255, 255, 255, 255);

            var resultSession = state.Commit(session, blender, backgroundColor);

            Assert.That(resultSession, Is.Not.Null);
            Assert.That(resultSession.CurrentPreviewContent, Is.Null, "Preview should be committed, so it clears out");
            Assert.That(resultSession.CanUndo(), Is.True, "History should have an item since a real commit happened");
            Assert.That(resultSession.CurrentSelectingArea, Is.Not.Null, "Selecting area should be retained/updated from the preview commit");
            Assert.That(resultSession.CurrentSelectingArea.Value.Size.Width, Is.EqualTo(20), "Selecting area should be updated to resized bounds");
        }

        [Test]
        public void Cancel_CancelsDrawingAndClearsPreview()
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

            // Drag to (20, 20) -> Size becomes 20x20
            state.HandlePointerMoved(HalfBoxArea.Create(startPos, new PictureSize(2, 2)), true, new Position(20, 20), false, new PictureSize(100, 100));

            var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(100, 100)));

            var resultSession = state.Cancel(session);

            Assert.That(resultSession, Is.Not.Null);
            Assert.That(resultSession.CurrentPreviewContent, Is.Null, "Preview content should be null after cancel");
            Assert.That(resultSession.IsDrawing(), Is.False, "Session should not be in drawing state");
            Assert.That(resultSession.CanUndo(), Is.False, "History should be empty because drawing was cancelled");
            Assert.That(resultSession.CurrentSelectingArea, Is.EqualTo(session.CurrentSelectingArea), "Selecting area should remain unchanged from the original session");
        }
    }
}
