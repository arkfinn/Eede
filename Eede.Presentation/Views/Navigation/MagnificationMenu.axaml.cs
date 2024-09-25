using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Eede.Domain.Scales;

namespace Eede.Presentation.Views.Navigation
{
    public partial class MagnificationMenu : UserControl
    {
        public MagnificationMenu()
        {
            InitializeComponent();
            UpdateChecked();
        }

        public static readonly StyledProperty<Magnification> MagnificationProperty =
            AvaloniaProperty.Register<MagnificationMenu, Magnification>(
                nameof(Magnification), new Magnification(4), defaultBindingMode: BindingMode.TwoWay);

        public Magnification Magnification
        {
            get => GetValue(MagnificationProperty);
            set
            {
                _ = SetValue(MagnificationProperty, value);
                UpdateChecked();
            }
        }

        public void SetMagnification1(object sender, RoutedEventArgs e)
        {
            Magnification = new Magnification(1);
        }

        public void SetMagnification2(object sender, RoutedEventArgs e)
        {
            Magnification = new Magnification(2);
        }

        public void SetMagnification4(object sender, RoutedEventArgs e)
        {
            Magnification = new Magnification(4);
        }
        public void SetMagnification6(object sender, RoutedEventArgs e)
        {
            Magnification = new Magnification(6);
        }
        public void SetMagnification8(object sender, RoutedEventArgs e)
        {
            Magnification = new Magnification(8);
        }
        public void SetMagnification12(object sender, RoutedEventArgs e)
        {
            Magnification = new Magnification(12);
        }
        private void UpdateChecked()
        {
            m1.IsChecked = Magnification.Value == 1;
            m2.IsChecked = Magnification.Value == 2;
            m4.IsChecked = Magnification.Value == 4;
            m6.IsChecked = Magnification.Value == 6;
            m8.IsChecked = Magnification.Value == 8;
            m12.IsChecked = Magnification.Value == 12;
        }
    }
}
