#nullable enable
using Eede.Domain.Palettes;

namespace Eede.Domain.ImageEditing;

public readonly record struct BackgroundColor(ArgbColor Value)
{

    // public BackgroundColor
    // {
    // 背景色に特有の制約をここに定義する
    // 例: 背景色は完全に透明にはできない、など
    // }


    // デフォルトの背景色を提供する静的プロパティ
    public static BackgroundColor Default => new(new ArgbColor(0, 0, 0, 0));

    // ArgbColorへの暗黙的な変換を定義すると便利かもしれない
    public static implicit operator ArgbColor(BackgroundColor color) => color.Value;
}
