using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing;

/// <summary>
/// プレビューの種類
/// </summary>
public enum SelectionPreviewType
{
    /// <summary>
    /// 切り取り＆移動（元の位置が透明になる）
    /// </summary>
    CutAndMove,
    /// <summary>
    /// 貼り付け（元の位置が維持される）
    /// </summary>
    Paste
}

/// <summary>
/// 選択範囲の移動中や貼り付け直後など、キャンバスに未確定のプレビュー画像情報を表す。
/// </summary>
/// <param name="Pixels">プレビュー画像データ</param>
/// <param name="Position">キャンバス上の位置</param>
/// <param name="Type">プレビューの種類</param>
/// <param name="OriginalArea">切り取り元の範囲（CutAndMoveの場合に使用）</param>
public record SelectionPreviewInfo(Picture Pixels, Position Position, SelectionPreviewType Type = SelectionPreviewType.CutAndMove, PictureArea? OriginalArea = null);
