using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace Eede.Presentation.Views.DataEntry
{
    public partial class PenWidthSelector : UserControl
    {
        public PenWidthSelector()
        {
            InitializeComponent();

            _ = this.GetObservable(PenWidthProperty).Subscribe(_ =>
            {
                UpdatePenWidthTextBox();
            });
        }

        public static readonly StyledProperty<int> PenWidthProperty =
            AvaloniaProperty.Register<PenWidthSelector, int>(nameof(PenWidth), 1, defaultBindingMode: BindingMode.TwoWay);
        public int PenWidth
        {
            get => GetValue(PenWidthProperty);
            set => SetValue(PenWidthProperty, ClampPenWidth(value));
        }

        private void UpdatePenWidthTextBox()
        {
            textBox1.Text = PenWidth.ToString();
        }

        public void OnTextInput(object sender, TextInputEventArgs args)
        {
            if (!int.TryParse(args.Text, out _))
            {
                args.Handled = true;
                return;
            }
            if (!char.IsDigit(args.Text[0]))
            {
                args.Handled = true;
            }
        }

        public void PenWidthUp(object sender, RoutedEventArgs args)
        {
            PenWidth--;
        }

        public void PenWidthDown(object sender, RoutedEventArgs args)
        {
            PenWidth++;
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

        private static int ClampPenWidth(int value)
        {
            return Math.Min(Math.Max(1, value), 30);
        }
    }
}
