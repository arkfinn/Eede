using System;
using System.Collections.Immutable;
using Eede.Domain.ImageEditing.DrawingTools;

namespace Eede.Domain.ImageEditing
{
    /// <summary>
    /// 描画セッションを管理する集約。
    /// 現在の画像データ（描画中を含む）と、Undo/Redo のための履歴を保持する。
    /// </summary>
    public sealed record DrawingSession
    {
        private readonly DrawingBuffer Buffer;
        private readonly ImmutableStack<Picture> UndoStack;
        private readonly ImmutableStack<Picture> RedoStack;

        public DrawingSession(Picture initialPicture) : this(new DrawingBuffer(initialPicture), ImmutableStack<Picture>.Empty, ImmutableStack<Picture>.Empty)
        {
        }

        private DrawingSession(DrawingBuffer buffer, ImmutableStack<Picture> undoStack, ImmutableStack<Picture> redoStack)
        {
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            UndoStack = undoStack ?? throw new ArgumentNullException(nameof(undoStack));
            RedoStack = redoStack ?? throw new ArgumentNullException(nameof(redoStack));
        }

        /// <summary>
        /// 現在の（表示すべき）画像データを取得する。
        /// </summary>
        public Picture CurrentPicture => Buffer.Fetch();

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
            return new DrawingSession(Buffer.UpdateDrawing(drawing), UndoStack, RedoStack);
        }

        /// <summary>
        /// 描画をキャンセルし、描画開始前の状態に戻す。
        /// </summary>
        public DrawingSession CancelDrawing()
        {
            return new DrawingSession(Buffer.CancelDrawing(), UndoStack, RedoStack);
        }

        /// <summary>
        /// 新しい画像データを確定させ、履歴に追加する。
        /// </summary>
        public DrawingSession Push(Picture nextPicture)
        {
            if (nextPicture == Buffer.Previous && !IsDrawing()) return this;
            return new DrawingSession(new DrawingBuffer(nextPicture), UndoStack.Push(Buffer.Previous), ImmutableStack<Picture>.Empty);
        }

        public DrawingSession Undo()
        {
            if (!CanUndo()) return this;
            var previous = UndoStack.Peek();
            return new DrawingSession(new DrawingBuffer(previous), UndoStack.Pop(), RedoStack.Push(Buffer.Previous));
        }

        public DrawingSession Redo()
        {
            if (!CanRedo()) return this;
            var next = RedoStack.Peek();
            return new DrawingSession(new DrawingBuffer(next), UndoStack.Push(Buffer.Previous), RedoStack.Pop());
        }

        public bool CanUndo() => !IsDrawing() && !UndoStack.IsEmpty;
        public bool CanRedo() => !IsDrawing() && !RedoStack.IsEmpty;
    }
}
