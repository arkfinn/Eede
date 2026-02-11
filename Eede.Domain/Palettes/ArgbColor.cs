#nullable enable
namespace Eede.Domain.Palettes
{
    public readonly record struct ArgbColor(byte Alpha, byte Red, byte Green, byte Blue)
    {
        public bool EqualsRgb(ArgbColor other)
        {
            return Red == other.Red
                && Green == other.Green
                && Blue == other.Blue;
        }

        public bool EqualsArgb(ArgbColor other)
        {
            return Alpha == other.Alpha
                && Red == other.Red
                && Green == other.Green
                && Blue == other.Blue;
        }

        /// <summary>
        /// 相対輝度 (0.0 - 1.0) を取得する。
        /// </summary>
        public double Luminance => (0.299 * Red + 0.587 * Green + 0.114 * Blue) / 255.0;
    }
}
