using Avalonia.Input;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using System;
using System.Reactive;
using ReactiveUI;

namespace Eede.Presentation.Services;

public interface IInteractionCoordinator
{
    // マウス操作に対応するメソッド
    void PointerBegin(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, PenStyle penStyle, bool isShift, bool isAnimationMode, PictureSize gridSize, ReactiveCommand<Picture, Unit> internalUpdateCommand);
    void PointerMoved(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, PenStyle penStyle, bool isShift, bool isAnimationMode, PictureSize gridSize);
    void PointerRightButtonPressed(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, bool isAnimationMode, PictureSize gridSize, Action<ArgbColor> colorPickedAction, ReactiveCommand<Picture, Unit> internalUpdateCommand);
    void PointerLeftButtonReleased(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, bool isAnimationMode, PictureSize gridSize, PenStyle penStyle, bool isShift, ReactiveCommand<Picture, Unit> internalUpdateCommand);
    void CanvasLeave(DrawingBuffer buffer);

    // 状態更新
    void UpdateMagnification(Magnification magnification);
    void SetupRegionSelector(RegionSelector tool, DrawingBuffer buffer, bool isAnimationMode, PictureSize gridSize);
    void PasteImage(Picture picture, DrawingBuffer buffer, IDrawStyle drawStyle);

    // 描画
    Picture Painted(DrawingBuffer buffer, PenStyle penStyle, IImageTransfer imageTransfer);

    // 状態取得 (ViewModelがバインドするもの)
    DrawingBuffer CurrentBuffer { get; }
    PictureArea? SelectingArea { get; }
    bool IsRegionSelecting { get; }
    Picture? PreviewPixels { get; }
    Position PreviewPosition { get; }
    Cursor ActiveCursor { get; }

    // イベント
    event Action<Picture, Picture, PictureArea?, PictureArea?> Drew;
    event Action StateChanged;
}