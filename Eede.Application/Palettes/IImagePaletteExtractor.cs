#nullable enable
using System.IO;
using System.Threading.Tasks;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;

namespace Eede.Application.Palettes;

/// <summary>
/// 画像データから 256 色以下のカラーパレットを抽出する能動アクターのインターフェース。
/// </summary>
public interface IImagePaletteExtractor
{
    /// <summary>
    /// 画像ストリームおよびデコード済み Picture からパレットの抽出を試みる。
    /// 256色以下の完全一致パレットまたは257色以上の代表256色パレットを PaletteExtractionResult として返す。
    /// </summary>
    Task<PaletteExtractionResult?> ExtractAsync(Stream stream, Picture picture, string extension);
}
