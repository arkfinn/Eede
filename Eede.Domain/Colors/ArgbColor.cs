namespace Eede.Domain.Colors
{
    public record ArgbColor(byte alpha, byte red, byte green, byte blue)
    {
        public readonly byte Alpha = alpha;
        public readonly byte Red = red;
        public readonly byte Green = green;
        public readonly byte Blue = blue;

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
    }
}
