using Avalonia;
using Avalonia.Controls;

namespace Eede.Presentation.Views.General
{
    public partial class Region : UserControl
    {
        public Region()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<bool> ShowHandlesProperty =
            AvaloniaProperty.Register<Region, bool>(nameof(ShowHandles));

        public bool ShowHandles
        {
            get => GetValue(ShowHandlesProperty);
            set => SetValue(ShowHandlesProperty, value);
        }

        public static readonly StyledProperty<double> HandleSizeProperty =
            AvaloniaProperty.Register<Region, double>(nameof(HandleSize), 8.0);

        public double HandleSize
        {
            get => GetValue(HandleSizeProperty);
            set => SetValue(HandleSizeProperty, value);
        }

        public static readonly StyledProperty<Thickness> HandleMarginProperty =
            AvaloniaProperty.Register<Region, Thickness>(nameof(HandleMargin), new Thickness(-4, -4, 0, 0));

        public Thickness HandleMargin
        {
            get => GetValue(HandleMarginProperty);
            set => SetValue(HandleMarginProperty, value);
        }
    }
}