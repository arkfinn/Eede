using Avalonia;
using Avalonia.Media;

namespace Eede.Presentation.Common
{
    public static class RenderOptionsHelper
    {
        public static readonly AttachedProperty<EdgeMode> EdgeModeProperty =
            AvaloniaProperty.RegisterAttached<Visual, EdgeMode>(
                "EdgeMode",
                typeof(RenderOptionsHelper),
                EdgeMode.Unspecified,
                inherits: true);

        static RenderOptionsHelper()
        {
            EdgeModeProperty.Changed.AddClassHandler<Visual>((v, e) =>
            {
                if (e.NewValue is EdgeMode mode)
                {
                    RenderOptions.SetEdgeMode(v, mode);
                }
            });
        }

        public static void SetEdgeMode(Visual element, EdgeMode value) => element.SetValue(EdgeModeProperty, value);
        public static EdgeMode GetEdgeMode(Visual element) => element.GetValue(EdgeModeProperty);
    }
}
