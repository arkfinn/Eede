using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Eede.Domain.Colors;
using Eede.Domain.DrawStyles;
using System;
using System.Drawing;

namespace Eede.Views.DataEntry
{
    public partial class PenWidthSelector : UserControl
    {
        public PenWidthSelector()
        {
            InitializeComponent();

            this.GetObservable(PenWidthProperty).Subscribe(_ =>
            {
                var penWidth = PenWidth;
                textBox1.Text = penWidth.ToString();
            });
        }

        public static readonly StyledProperty<int> PenWidthProperty =
            AvaloniaProperty.Register<PenWidthSelector, int>(nameof(PenWidth), 1, defaultBindingMode: BindingMode.TwoWay);
        public int PenWidth
        {
            get => GetValue(PenWidthProperty);
            set => SetValue(PenWidthProperty, Math.Min(Math.Max(1, value), 30));
        }

        public void OnTextInput(object sender, TextInputEventArgs args)
        {
            if (!int.TryParse(args.Text, out int result))
            {
                args.Handled = true;
                return;
            }
            if (result is not (>= '0' and <= '9'))
            {
                args.Handled = true;
            }
        }

        public void PenWidthUp(object sender, RoutedEventArgs args)
        {
            PenWidth = PenWidth - 1;
        }

        public void PenWidthDown(object sender, RoutedEventArgs args)
        {
            PenWidth = PenWidth + 1;
        }

        public void PenWidth1px(object sender, RoutedEventArgs args)
        {
            PenWidth = 1;
        }

        public void PenWidth3px(object sender, RoutedEventArgs args)
        {
            PenWidth = 3;
        }

        public void PenWidth6px(object sender, RoutedEventArgs args)
        {
            PenWidth = 6;
        }
    }
}
