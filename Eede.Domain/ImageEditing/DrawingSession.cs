#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        public const int DefaultMaxUndoCount = 200;

        public readonly DrawingBuffer Buffer;
        private readonly PictureArea? SelectingArea;
        private readonly SelectionPreviewInfo? PreviewContent;
        private readonly ImmutableStack<IHistoryItem> UndoStack;
        private readonly ImmutableStack<IHistoryItem> RedoStack;
        public readonly int UndoCount;
        public readonly int MaxUndoCount;

        public DrawingSession(Picture initialPicture, PictureArea? initialArea = null, int maxUndoCount = DefaultMaxUndoCount)
            : this(new DrawingBuffer(initialPicture ?? throw new ArgumentNullException(nameof(initialPicture))), initialArea, null, ImmutableStack<IHistoryItem>.Empty, ImmutableStack<IHistoryItem>.Empty, 0, maxUndoCount <= 0 ? DefaultMaxUndoCount : maxUndoCount)
        {
        }

        private DrawingSession(DrawingBuffer buffer, PictureArea? selectingArea, SelectionPreviewInfo? previewContent, ImmutableStack<IHistoryItem> undoStack, ImmutableStack<IHistoryItem> redoStack, int undoCount, int maxUndoCount)
        {
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            SelectingArea = selectingArea;
            PreviewContent = previewContent;
            UndoStack = undoStack ?? throw new ArgumentNullException(nameof(undoStack));
            RedoStack = redoStack ?? throw new ArgumentNullException(nameof(redoStack));
            UndoCount = undoCount;
            MaxUndoCount = maxUndoCount;
        }

        private (ImmutableStack<IHistoryItem> Stack, int Count) PushUndoStack(IHistoryItem item)
        {
            var nextStack = UndoStack.Push(item);
            if (UndoCount >= MaxUndoCount)
            {
                var trimmed = TrimBottom(nextStack, MaxUndoCount);
                return (trimmed, MaxUndoCount);
            }
            return (nextStack, UndoCount + 1);
        }

        private static ImmutableStack<T> TrimBottom<T>(ImmutableStack<T> stack, int limit)
        {
            var list = stack.Take(limit).ToList();
            list.Reverse();
            var newStack = ImmutableStack<T>.Empty;
            foreach (var item in list)
            {
                newStack = newStack.Push(item);
            }
            return newStack;
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
            return new DrawingSession(Buffer.UpdateDrawing(drawing), SelectingArea, PreviewContent, UndoStack, RedoStack, UndoCount, MaxUndoCount);
        }

        /// <summary>
        /// プレビュー画像の状態を更新する。履歴には追加されない。
        /// </summary>
        public DrawingSession UpdatePreviewContent(SelectionPreviewInfo? previewContent)
        {
            return new DrawingSession(Buffer, SelectingArea, previewContent, UndoStack, RedoStack, UndoCount, MaxUndoCount);
        }

        /// <summary>
        /// 現在の描画バッファを直接差し替える。履歴には追加しない。
        /// </summary>
        public DrawingSession UpdateBuffer(DrawingBuffer buffer)
        {
            return new DrawingSession(buffer, SelectingArea, PreviewContent, UndoStack, RedoStack, UndoCount, MaxUndoCount);
        }

        /// <summary>
        /// 現在の画像を差し替える。履歴には追加しない。
        /// </summary>
        public DrawingSession ReplacePicture(Picture picture)
        {
            return new DrawingSession(Buffer.Reset(picture), SelectingArea, PreviewContent, UndoStack, RedoStack, UndoCount, MaxUndoCount);
        }

        /// <summary>
        /// 描画をキャンセルし、描画開始前の状態に戻す。
        /// プレビュー中の画像がある場合も破棄する。
        /// </summary>
        public DrawingSession CancelDrawing()
        {
            return new DrawingSession(Buffer.CancelDrawing(), SelectingArea, null, UndoStack, RedoStack, UndoCount, MaxUndoCount);
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
            // 確定時は、現在の状態（プレビューあり）を履歴に積む。
            // また、確定後の画像を nextPicture として渡し、履歴の「戻り先」として OriginalArea を記録する。
            return Push(FetchPicture(blender), nextArea, PreviewContent.OriginalArea);
        }

        /// <summary>
        /// 新しい画像データを確定させ、履歴に追加する。
        /// </summary>
        public DrawingSession Push(Picture nextPicture, PictureArea? nextArea = null, PictureArea? previousArea = null, Picture? beforePicture = null)
        {
            var areaToStore = previousArea ?? SelectingArea;
            var pictureToStore = beforePicture ?? Buffer.Previous;
            if (nextPicture == pictureToStore && nextArea == SelectingArea && !IsDrawing() && PreviewContent == null) return this;

            var (newUndoStack, newUndoCount) = PushUndoStack(new CanvasHistoryItem(pictureToStore, areaToStore));
            return new DrawingSession(
                new DrawingBuffer(nextPicture),
                nextArea,
                null,
                newUndoStack,
                ImmutableStack<IHistoryItem>.Empty,
                newUndoCount,
                MaxUndoCount);
        }

        public DrawingSession PushDiff(Picture nextPicture, PictureRegion region, PictureArea? nextArea = null, PictureArea? previousArea = null, Picture? beforePicture = null)
        {
            var areaToStore = previousArea ?? SelectingArea;
            var pictureToCompare = beforePicture ?? Buffer.Previous;
            var diffs = region.Select(area => new PictureDiff(
                area,
                pictureToCompare.CutOut(area),
                nextPicture.CutOut(area)
            )).ToList().AsReadOnly();

            var (newUndoStack, newUndoCount) = PushUndoStack(new DiffHistoryItem(diffs, areaToStore));
            return new DrawingSession(
                new DrawingBuffer(nextPicture),
                nextArea,
                null,
                newUndoStack,
                ImmutableStack<IHistoryItem>.Empty,
                newUndoCount,
                MaxUndoCount);
        }

        public DrawingSession PushDockUpdate(string dockId, Position position, Picture before, Picture after, bool beforeEdited, bool afterEdited)
        {
            var (newUndoStack, newUndoCount) = PushUndoStack(new DockActiveHistoryItem(dockId, position, before, after, beforeEdited, afterEdited));
            return new DrawingSession(
                Buffer,
                SelectingArea,
                PreviewContent,
                newUndoStack,
                ImmutableStack<IHistoryItem>.Empty,
                newUndoCount,
                MaxUndoCount);
        }

        /// <summary>
        /// 現在の選択範囲を更新した新しいセッションを返す。履歴には追加されない。
        /// </summary>
        public DrawingSession UpdateSelectingArea(PictureArea? area)
        {
            return new DrawingSession(Buffer, area, PreviewContent, UndoStack, RedoStack, UndoCount, MaxUndoCount);
        }

        public UndoResult Undo()
        {
            if (!CanUndo()) return new UndoResult(this, null);
            var historyItem = UndoStack.Peek();
            var newUndoStack = UndoStack.Pop();
            int newUndoCount = Math.Max(0, UndoCount - 1);

            if (historyItem is CanvasHistoryItem canvasItem)
            {
                var nextSession = new DrawingSession(
                    new DrawingBuffer(canvasItem.Picture),
                    canvasItem.SelectingArea,
                    null,
                    newUndoStack,
                    RedoStack.Push(new CanvasHistoryItem(Buffer.Previous, SelectingArea)),
                    newUndoCount,
                    MaxUndoCount);
                return new UndoResult(nextSession, canvasItem);
            }
            else if (historyItem is DiffHistoryItem diffItem)
            {
                var restoredPicture = Buffer.Previous;
                var blender = new DirectImageBlender();
                foreach (var diff in diffItem.Diffs)
                {
                    restoredPicture = restoredPicture.Blend(blender, diff.Before, diff.Area.Position);
                }
                var nextSession = new DrawingSession(
                    new DrawingBuffer(restoredPicture),
                    diffItem.SelectingArea,
                    null,
                    newUndoStack,
                    RedoStack.Push(diffItem.Reverse(SelectingArea)),
                    newUndoCount,
                    MaxUndoCount);
                return new UndoResult(nextSession, diffItem);
            }
            else if (historyItem is DockActiveHistoryItem dockItem)
            {
                // For Dock updates, we just move the item to Redo stack.
                // The DrawingSession's own state (Picture/Selection) does NOT change.
                var nextSession = new DrawingSession(
                    Buffer,
                    SelectingArea,
                    null,
                    newUndoStack,
                    RedoStack.Push(dockItem),
                    newUndoCount,
                    MaxUndoCount);
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
                var (newUndoStack, newUndoCount) = PushUndoStack(new CanvasHistoryItem(Buffer.Previous, SelectingArea));
                var nextSession = new DrawingSession(
                    new DrawingBuffer(canvasItem.Picture),
                    canvasItem.SelectingArea,
                    null,
                    newUndoStack,
                    newRedoStack,
                    newUndoCount,
                    MaxUndoCount);
                return new RedoResult(nextSession, canvasItem);
            }
            else if (historyItem is DiffHistoryItem diffItem)
            {
                var restoredPicture = Buffer.Previous;
                var blender = new DirectImageBlender();
                foreach (var diff in diffItem.Diffs)
                {
                    restoredPicture = restoredPicture.Blend(blender, diff.Before, diff.Area.Position);
                }
                var (newUndoStack, newUndoCount) = PushUndoStack(diffItem.Reverse(SelectingArea));
                var nextSession = new DrawingSession(
                    new DrawingBuffer(restoredPicture),
                    diffItem.SelectingArea,
                    null,
                    newUndoStack,
                    newRedoStack,
                    newUndoCount,
                    MaxUndoCount);
                return new RedoResult(nextSession, diffItem);
            }
            else if (historyItem is DockActiveHistoryItem dockItem)
            {
                var (newUndoStack, newUndoCount) = PushUndoStack(dockItem);
                var nextSession = new DrawingSession(
                    Buffer,
                    SelectingArea,
                    null,
                    newUndoStack,
                    newRedoStack,
                    newUndoCount,
                    MaxUndoCount);
                return new RedoResult(nextSession, dockItem);
            }

            throw new InvalidOperationException("Unknown history item type.");
        }

        public bool CanUndo() => !IsDrawing() && !UndoStack.IsEmpty;
        public bool CanRedo() => !IsDrawing() && !RedoStack.IsEmpty;
    }
}
