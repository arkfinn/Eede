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
    /// 256色以下のパレットが存在・抽出できた場合は Palette を、256色を超える場合や非対応の場合は null を返す。
    /// </summary>
    Task<Palette?> ExtractAsync(Stream stream, Picture picture, string extension);
}
