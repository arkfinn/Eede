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
    }
}