using System;
using System.Collections.Immutable;
using Eede.Domain.ImageEditing.Blending;
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
        public readonly DrawingBuffer Buffer;
        private readonly PictureArea? SelectingArea;
        private readonly SelectionPreviewInfo? PreviewContent;
        private readonly ImmutableStack<IHistoryItem> UndoStack;
        private readonly ImmutableStack<IHistoryItem> RedoStack;

        public DrawingSession(Picture initialPicture, PictureArea? initialArea = null)
            : this(new DrawingBuffer(initialPicture), initialArea, null, ImmutableStack<IHistoryItem>.Empty, ImmutableStack<IHistoryItem>.Empty)
        {
        }

        private DrawingSession(DrawingBuffer buffer, PictureArea? selectingArea, SelectionPreviewInfo? previewContent, ImmutableStack<IHistoryItem> undoStack, ImmutableStack<IHistoryItem> redoStack)
        {
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            SelectingArea = selectingArea;
            PreviewContent = previewContent;
            UndoStack = undoStack ?? throw new ArgumentNullException(nameof(undoStack));
            RedoStack = redoStack ?? throw new ArgumentNullException(nameof(redoStack));
        }

        /// <summary>
        /// 現在の（表示すべき）画像データを取得する。
        /// プレビュー中の画像がある場合は合成して返す。
        /// </summary>
        public Picture CurrentPicture => FetchPicture(new DirectImageBlender());

        /// <summary>
        /// 指定したブレンダーを使用して、プレビュー画像を含めた現在の画像データを取得する。
        /// </summary>
        /// <param name="blender">プレビュー画像の合成に使用するブレンダー</param>
        /// <returns>合成済みの画像</returns>
        public Picture FetchPicture(IImageBlender blender)
        {
            var picture = Buffer.Fetch();
            if (PreviewContent != null)
            {
                if (PreviewContent.OriginalArea.HasValue)
                {
                    picture = picture.Clear(PreviewContent.OriginalArea.Value);
                }
                return picture.Blend(blender, PreviewContent.Pixels, PreviewContent.Position);
            }
            return picture;
        }

        /// <summary>
        /// 現在の選択範囲を取得する。
        /// </summary>
        public PictureArea? CurrentSelectingArea => SelectingArea;

        /// <summary>
        /// 現在のプレビュー画像情報を取得する。
        /// </summary>
        public SelectionPreviewInfo? CurrentPreviewContent => PreviewContent;

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
            return new DrawingSession(Buffer.UpdateDrawing(drawing), SelectingArea, PreviewContent, UndoStack, RedoStack);
        }

        /// <summary>
        /// プレビュー画像の状態を更新する。履歴には追加されない。
        /// </summary>
        public DrawingSession UpdatePreviewContent(SelectionPreviewInfo? previewContent)
        {
            return new DrawingSession(Buffer, SelectingArea, previewContent, UndoStack, RedoStack);
        }

        /// <summary>
        /// 現在の描画バッファを直接差し替える。履歴には追加しない。
        /// </summary>
        public DrawingSession UpdateBuffer(DrawingBuffer buffer)
        {
            return new DrawingSession(buffer, SelectingArea, PreviewContent, UndoStack, RedoStack);
        }

        /// <summary>
        /// 現在の画像を差し替える。履歴には追加しない。
        /// </summary>
        public DrawingSession ReplacePicture(Picture picture)
        {
            return new DrawingSession(Buffer.Reset(picture), SelectingArea, PreviewContent, UndoStack, RedoStack);
        }

        /// <summary>
        /// 描画をキャンセルし、描画開始前の状態に戻す。
        /// プレビュー中の画像がある場合も破棄する。
        /// </summary>
        public DrawingSession CancelDrawing()
        {
            return new DrawingSession(Buffer.CancelDrawing(), SelectingArea, null, UndoStack, RedoStack);
        }

        /// <summary>
        /// 貼り付け画像をプレビューとして受け入れる。
        /// </summary>
        public DrawingSession PushPastePreview(Picture pixels, Position position)
        {
            return UpdatePreviewContent(new SelectionPreviewInfo(pixels, position, SelectionPreviewType.Paste));
        }

        /// <summary>
        /// 現在のプレビュー状態を確定させ、履歴に追加する。
        /// </summary>
        public DrawingSession CommitPreview(IImageBlender blender)
        {
            if (PreviewContent == null) return this;

            var nextArea = new PictureArea(PreviewContent.Position, PreviewContent.Pixels.Size);
            return Push(FetchPicture(blender), nextArea);
        }

        /// <summary>
        /// 新しい画像データを確定させ、履歴に追加する。
        /// </summary>
        public DrawingSession Push(Picture nextPicture, PictureArea? nextArea = null, PictureArea? previousArea = null)
        {
            var areaToStore = previousArea ?? SelectingArea;
            if (nextPicture == Buffer.Previous && nextArea == SelectingArea && !IsDrawing() && PreviewContent == null) return this;
            return new DrawingSession(
                new DrawingBuffer(nextPicture),
                nextArea,
                null,
                UndoStack.Push(new CanvasHistoryItem(Buffer.Previous, areaToStore)),
                ImmutableStack<IHistoryItem>.Empty);
        }

        public DrawingSession PushDockUpdate(string dockId, Position position, Picture before, Picture after)
        {
            return new DrawingSession(
                Buffer,
                SelectingArea,
                PreviewContent,
                UndoStack.Push(new DockActiveHistoryItem(dockId, position, before, after)),
                ImmutableStack<IHistoryItem>.Empty);
        }

        /// <summary>
        /// 現在の選択範囲を更新した新しいセッションを返す。履歴には追加されない。
        /// </summary>
        public DrawingSession UpdateSelectingArea(PictureArea? area)
        {
            return new DrawingSession(Buffer, area, PreviewContent, UndoStack, RedoStack);
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
                    null,
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
                    PreviewContent,
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
                    null,
                    UndoStack.Push(new CanvasHistoryItem(Buffer.Previous, SelectingArea)),
                    newRedoStack);
                return new RedoResult(nextSession, canvasItem);
            }
            else if (historyItem is DockActiveHistoryItem dockItem)
            {
                var nextSession = new DrawingSession(
                    Buffer,
                    SelectingArea,
                    PreviewContent,
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
