using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Application.Drawings
{
    /// <summary>
    /// 描画操作（開始、移動、終了）のユースケース。
    /// ViewModel からの入力を受け取り、ドメインモデルを調整する。
    /// </summary>
    public class DrawActionUseCase : IDrawActionUseCase
    {
        private CoordinateHistory History;

        public DrawActionUseCase()
        {
        }

        public DrawingSession DrawStart(DrawingSession session, DrawingTool tool, DisplayCoordinate displayCoordinate, Magnification magnification, bool isShift)
        {
            if (session.IsDrawing()) return session;

            var canvasCoordinate = displayCoordinate.ToCanvas(magnification);
            History = new CoordinateHistory(canvasCoordinate);

            return session.UpdateDrawing(tool.DrawStart(new DrawingBuffer(session.CurrentPicture), History, isShift).Fetch());
        }

        public DrawingSession Drawing(DrawingSession session, DrawingTool tool, DisplayCoordinate displayCoordinate, Magnification magnification, bool isShift)
        {
            if (!session.IsDrawing() || History == null) return session;

            var canvasCoordinate = displayCoordinate.ToCanvas(magnification);
            History = History.Update(canvasCoordinate);

            return session.UpdateDrawing(tool.Drawing(new DrawingBuffer(session.PreviousPicture), History, isShift).Fetch());
        }

        public DrawingSession DrawEnd(DrawingSession session, DrawingTool tool, DisplayCoordinate displayCoordinate, Magnification magnification, bool isShift)
        {
            if (!session.IsDrawing() || History == null) return session;

            var canvasCoordinate = displayCoordinate.ToCanvas(magnification);
            History = History.Update(canvasCoordinate);

            var result = tool.DrawEnd(new DrawingBuffer(session.PreviousPicture), History, isShift);
            History = null;

            if (!result.AffectedArea.IsEmpty)
            {
                return session.PushDiff(result.Buffer.Fetch(), result.AffectedArea, session.CurrentSelectingArea);
            }
            return session.Push(result.Buffer.Fetch(), session.CurrentSelectingArea);
        }
    }
}
