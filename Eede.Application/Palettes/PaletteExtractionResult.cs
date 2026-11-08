#nullable enable
using Eede.Domain.Palettes;

namespace Eede.Application.Palettes;

/// <summary>
/// パレット抽出結果を表す不変の値オブジェクト。
/// </summary>
/// <param name="Palette">抽出された256色パレット。</param>
/// <param name="IsRepresentative">257色以上のフルカラー画像から減色抽出された代表色パレットである場合は true、完全一致パレットの場合は false。</param>
public readonly record struct PaletteExtractionResult(Palette Palette, bool IsRepresentative);
