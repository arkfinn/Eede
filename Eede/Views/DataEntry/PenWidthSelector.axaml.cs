using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Eede.Domain.DrawStyles;
using System;

namespace Eede.Views.DataEntry
{
    public partial class PenWidthSelector : UserControl
    {
        public PenWidthSelector()
        {
            InitializeComponent();
        }

        private int _penWidth = 1;
        public int PenWidth
        {
            get { return _penWidth; }
            set
            {
                int newSize = Math.Min(Math.Max(1, value), 30);
                _penWidth = newSize;
                textBox1.Text = newSize.ToString();
                PenWidthChanged?.Invoke(newSize);
            }
        }

        public event Action<int>? PenWidthChanged;

        public void OnTextInput(object sender, TextInputEventArgs args)
        {           
            if (!int.TryParse(args.Text, out int result))
            {
                args.Handled = true;
                return;
            }
            if ( result is not (>= '0' and <= '9'))
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
