using System;
using System.Collections.Immutable;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing
{
    /// <summary>
    /// 描画セッションを管理する集約。
    /// 現在の画像データ（描画中を含む）と、Undo/Redo のための履歴を保持する。
    /// </summary>
    public sealed record DrawingSession
    {
        private record HistoryItem(Picture Picture, PictureArea? SelectingArea);

        private readonly DrawingBuffer Buffer;
        private readonly PictureArea? SelectingArea;
        private readonly ImmutableStack<HistoryItem> UndoStack;
        private readonly ImmutableStack<HistoryItem> RedoStack;

        public DrawingSession(Picture initialPicture, PictureArea? initialArea = null)
            : this(new DrawingBuffer(initialPicture), initialArea, ImmutableStack<HistoryItem>.Empty, ImmutableStack<HistoryItem>.Empty)
        {
        }

        private DrawingSession(DrawingBuffer buffer, PictureArea? selectingArea, ImmutableStack<HistoryItem> undoStack, ImmutableStack<HistoryItem> redoStack)
        {
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            SelectingArea = selectingArea;
            UndoStack = undoStack ?? throw new ArgumentNullException(nameof(undoStack));
            RedoStack = redoStack ?? throw new ArgumentNullException(nameof(redoStack));
        }

        /// <summary>
        /// 現在の（表示すべき）画像データを取得する。
        /// </summary>
        public Picture CurrentPicture => Buffer.Fetch();

        /// <summary>
        /// 現在の選択範囲を取得する。
        /// </summary>
        public PictureArea? CurrentSelectingArea => SelectingArea;

        /// <summary>
        /// 履歴として保存されている直近の画像データを取得する。
        /// </summary>
        public Picture PreviousPicture => Buffer.Previous;

        public bool IsDrawing() => Buffer.IsDrawing();

        /// <summary>
        /// 描画中の一時的な状態を更新する。履歴には追加されない。
        /// </summary>
        public DrawingSession UpdateDrawing(Picture drawing)
        {
            return new DrawingSession(Buffer.UpdateDrawing(drawing), SelectingArea, UndoStack, RedoStack);
        }

        /// <summary>
        /// 描画をキャンセルし、描画開始前の状態に戻す。
        /// </summary>
        public DrawingSession CancelDrawing()
        {
            return new DrawingSession(Buffer.CancelDrawing(), SelectingArea, UndoStack, RedoStack);
        }

        /// <summary>
        /// 新しい画像データを確定させ、履歴に追加する。
        /// </summary>
        public DrawingSession Push(Picture nextPicture, PictureArea? nextArea = null)
        {
            if (nextPicture == Buffer.Previous && nextArea == SelectingArea && !IsDrawing()) return this;
            return new DrawingSession(
                new DrawingBuffer(nextPicture),
                nextArea,
                UndoStack.Push(new HistoryItem(Buffer.Previous, SelectingArea)),
                ImmutableStack<HistoryItem>.Empty);
        }

        public DrawingSession Undo()
        {
            if (!CanUndo()) return this;
            var previous = UndoStack.Peek();
            return new DrawingSession(
                new DrawingBuffer(previous.Picture),
                previous.SelectingArea,
                UndoStack.Pop(),
                RedoStack.Push(new HistoryItem(Buffer.Previous, SelectingArea)));
        }

        public DrawingSession Redo()
        {
            if (!CanRedo()) return this;
            var next = RedoStack.Peek();
            return new DrawingSession(
                new DrawingBuffer(next.Picture),
                next.SelectingArea,
                UndoStack.Push(new HistoryItem(Buffer.Previous, SelectingArea)),
                RedoStack.Pop());
        }

        public bool CanUndo() => !IsDrawing() && !UndoStack.IsEmpty;
        public bool CanRedo() => !IsDrawing() && !RedoStack.IsEmpty;
    }
}
