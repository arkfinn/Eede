namespace Eede.Domain.PenStyles
{
    public class DrawingMaterial
    {
        public readonly AlphaPicture SourcePicture;
        public readonly AlphaPicture DrawingPicture;
        public readonly PenCase PenCase;

        public DrawingMaterial(AlphaPicture sourcePicture, AlphaPicture drawingPicture, PenCase penCase)
        {
            SourcePicture = sourcePicture;
            DrawingPicture = drawingPicture;
            PenCase = penCase;
        }
    }
}