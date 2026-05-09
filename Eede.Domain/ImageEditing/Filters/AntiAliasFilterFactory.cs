namespace Eede.Domain.ImageEditing.Filters;

public enum AntiAliasMode
{
    Rgb,
    Alpha,
    Argb
}

public static class AntiAliasFilterFactory
{
    public static AntiAliasFilter Create(AntiAliasMode mode)
    {
        IAntiAliasStrategy strategy = mode switch
        {
            AntiAliasMode.Rgb => new RgbAntiAliasStrategy(),
            AntiAliasMode.Alpha => new AlphaAntiAliasStrategy(),
            AntiAliasMode.Argb => new ArgbAntiAliasStrategy(),
            _ => throw new System.ArgumentException(null, nameof(mode))
        };
        return new AntiAliasFilter(strategy);
    }
}
