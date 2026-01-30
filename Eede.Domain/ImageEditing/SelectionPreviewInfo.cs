using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing;

/// <summary>
/// 選択範囲の移動中や貼り付け直後など、キャンバスに未確定のプレビュー画像情報を表す。
/// </summary>
/// <param name="Pixels">プレビュー画像データ</param>
/// <param name="Position">キャンバス上の位置</param>
public record SelectionPreviewInfo(Picture Pixels, Position Position);
