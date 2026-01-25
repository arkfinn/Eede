using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing
{
    public class CoordinateHistory
    {
        public readonly CanvasCoordinate Start;
        public readonly CanvasCoordinate Last;
        public readonly CanvasCoordinate Now;

        public CoordinateHistory(CanvasCoordinate start)
        {
            Start = start;
            Last = start;
            Now = start;
        }

        private CoordinateHistory(CanvasCoordinate start, CanvasCoordinate last, CanvasCoordinate now)
        {
            Start = start;
            Last = last;
            Now = now;
        }

        public CoordinateHistory Update(CanvasCoordinate now)
        {
            return new CoordinateHistory(Start, Now, now);
        }

        public PositionHistory ToPositionHistory()
        {
            return new PositionHistory(Start.ToPosition()).Update(Last.ToPosition()).Update(Now.ToPosition());
        }
    }
}
