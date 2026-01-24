using System;
using Eede.Domain.Palettes;

namespace Eede.Domain.ImageEditing.DrawingTools
{
    /// <summary>
    /// 描画スタイルとペン設定を統合した描画ツールの値オブジェクト。
    /// </summary>
    public sealed record DrawingTool
    {
        public IDrawStyle Style { get; init; }
        public PenStyle PenStyle { get; init; }

        public DrawingTool(IDrawStyle style, PenStyle penStyle)
        {
            Style = style ?? throw new ArgumentNullException(nameof(style));
            PenStyle = penStyle ?? throw new ArgumentNullException(nameof(penStyle));
        }

        public DrawingBuffer DrawStart(DrawingBuffer buffer, PositionHistory positionHistory, bool isShift)
        {
            return Style.DrawStart(buffer, PenStyle, positionHistory, isShift);
        }

        public DrawingBuffer Drawing(DrawingBuffer buffer, PositionHistory positionHistory, bool isShift)
        {
            return Style.Drawing(buffer, PenStyle, positionHistory, isShift);
        }

        public DrawingBuffer DrawEnd(DrawingBuffer buffer, PositionHistory positionHistory, bool isShift)
        {
            return Style.DrawEnd(buffer, PenStyle, positionHistory, isShift);
        }

        public DrawingTool WithColor(ArgbColor color)
        {
            return this with { PenStyle = PenStyle.UpdateColor(color) };
        }

        public DrawingTool WithWidth(int width)
        {
            return this with { PenStyle = PenStyle.UpdateWidth(width) };
        }

        public DrawingTool WithStyle(IDrawStyle style)
        {
            return this with { Style = style };
        }
    }
}
