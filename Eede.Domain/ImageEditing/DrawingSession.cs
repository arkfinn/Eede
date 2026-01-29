using System;
using System.Collections.Immutable;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.History;
using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing
{
    /// <summary>
    /// 描画セッションを管理する集約。
    /// 現在の画像データ（描画中を含む）と、Undo/Redo のための履歴を保持する。
    /// </summary>
    public sealed record DrawingSession
    {
        private readonly DrawingBuffer Buffer;
        private readonly PictureArea? SelectingArea;
        private readonly ImmutableStack<IHistoryItem> UndoStack;
        private readonly ImmutableStack<IHistoryItem> RedoStack;

        public DrawingSession(Picture initialPicture, PictureArea? initialArea = null)
            : this(new DrawingBuffer(initialPicture), initialArea, ImmutableStack<IHistoryItem>.Empty, ImmutableStack<IHistoryItem>.Empty)
        {
        }

        private DrawingSession(DrawingBuffer buffer, PictureArea? selectingArea, ImmutableStack<IHistoryItem> undoStack, ImmutableStack<IHistoryItem> redoStack)
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
        public DrawingSession Push(Picture nextPicture, PictureArea? nextArea = null, PictureArea? previousArea = null)
        {
            var areaToStore = previousArea ?? SelectingArea;
            if (nextPicture == Buffer.Previous && nextArea == SelectingArea && !IsDrawing()) return this;
            return new DrawingSession(
                new DrawingBuffer(nextPicture),
                nextArea,
                UndoStack.Push(new CanvasHistoryItem(Buffer.Previous, areaToStore)),
                ImmutableStack<IHistoryItem>.Empty);
        }

        public DrawingSession PushDockUpdate(string dockId, Position position, Picture picture)
        {
            return new DrawingSession(
                Buffer,
                SelectingArea,
                UndoStack.Push(new DockActiveHistoryItem(dockId, position, picture)),
                ImmutableStack<IHistoryItem>.Empty);
        }

        /// <summary>
        /// 現在の選択範囲を更新した新しいセッションを返す。履歴には追加されない。
        /// </summary>
        public DrawingSession UpdateSelectingArea(PictureArea? area)
        {
            return new DrawingSession(Buffer, area, UndoStack, RedoStack);
        }

        public UndoResult Undo()
        {
            if (!CanUndo()) return new UndoResult(this, null);
            var historyItem = UndoStack.Peek();
            var newUndoStack = UndoStack.Pop();

            if (historyItem is CanvasHistoryItem canvasItem)
            {
                var nextSession = new DrawingSession(
                    new DrawingBuffer(canvasItem.Picture),
                    canvasItem.SelectingArea,
                    newUndoStack,
                    RedoStack.Push(new CanvasHistoryItem(Buffer.Previous, SelectingArea)));
                return new UndoResult(nextSession, canvasItem);
            }
            else if (historyItem is DockActiveHistoryItem dockItem)
            {
                // For Dock updates, we just move the item to Redo stack.
                // The DrawingSession's own state (Picture/Selection) does NOT change.
                var nextSession = new DrawingSession(
                    Buffer,
                    SelectingArea,
                    newUndoStack,
                    RedoStack.Push(dockItem));
                return new UndoResult(nextSession, dockItem);
            }

            throw new InvalidOperationException("Unknown history item type.");
        }

        public RedoResult Redo()
        {
            if (!CanRedo()) return new RedoResult(this, null);
            var historyItem = RedoStack.Peek();
            var newRedoStack = RedoStack.Pop();

            if (historyItem is CanvasHistoryItem canvasItem)
            {
                var nextSession = new DrawingSession(
                    new DrawingBuffer(canvasItem.Picture),
                    canvasItem.SelectingArea,
                    UndoStack.Push(new CanvasHistoryItem(Buffer.Previous, SelectingArea)),
                    newRedoStack);
                return new RedoResult(nextSession, canvasItem);
            }
            else if (historyItem is DockActiveHistoryItem dockItem)
            {
                var nextSession = new DrawingSession(
                    Buffer,
                    SelectingArea,
                    UndoStack.Push(dockItem),
                    newRedoStack);
                return new RedoResult(nextSession, dockItem);
            }

            throw new InvalidOperationException("Unknown history item type.");
        }

        public bool CanUndo() => !IsDrawing() && !UndoStack.IsEmpty;
        public bool CanRedo() => !IsDrawing() && !RedoStack.IsEmpty;
    }
}
