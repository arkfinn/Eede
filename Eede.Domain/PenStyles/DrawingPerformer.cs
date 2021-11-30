using Eede.Domain.Positions;

namespace Eede.Domain.PenStyles
{
    public class DrawingPerformer
    {
        private readonly AlphaPicture SourcePicture;
        public readonly AlphaPicture DrawingPicture;
        private readonly PenCase PenCase;

        public DrawingPerformer(AlphaPicture sourcePicture, AlphaPicture drawingPicture, PenCase penCase)
        {
            SourcePicture = sourcePicture;
            DrawingPicture = drawingPicture;
            PenCase = penCase;
        }

        public AlphaPicture DrawPoint(Position position)
        {
            return DrawingPicture.DrawPoint(PenCase, position);
        }

        public AlphaPicture DrawLine(Position from, Position to)
        {
            return DrawingPicture.DrawLine(PenCase, from, to);
        }

        public bool Contains(Position position)
        {
            return DrawingPicture.Contains(position);
        }
    }
}