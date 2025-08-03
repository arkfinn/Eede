namespace Eede.Domain.Colors
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
    }
}
