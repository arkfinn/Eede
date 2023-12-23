using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eede.Domain.DrawStyles;
using System;

namespace Eede.Views.Navigaion
{
    internal partial class DrawStyleMenu : UserControl
    {
        public DrawStyleMenu()
        {
            InitializeComponent();
            UpdateChecked();
        }

        public static readonly StyledProperty<IDrawStyle> DrawStyleProperty =
            AvaloniaProperty.Register<DrawStyleMenu, IDrawStyle>(nameof(DrawStyle), new FreeCurve());

        public IDrawStyle DrawStyle
        {
            get { return GetValue(DrawStyleProperty); }
            set
            {
                SetValue(DrawStyleProperty, value);
                UpdateChecked();
                DrawStyleChanged?.Invoke(GetValue(DrawStyleProperty));
            }
        }

        public void SetDrawStyleFreeCurve(object sender, RoutedEventArgs e)
        {
            DrawStyle = new FreeCurve();
        }

        public void SetDrawStyleLine(object sender, RoutedEventArgs e)
        {
            DrawStyle = new Line();
        }

        public void SetDrawStyleFill(object sender, RoutedEventArgs e)
        {
            DrawStyle = new Fill();
        }

        private void UpdateChecked()
        {
            ButtonFreeVurve.IsChecked = DrawStyle is FreeCurve;
            ButtonLine.IsChecked = DrawStyle is Line;
            ButtonFill.IsChecked = DrawStyle is Fill;
        }

        public event Action<IDrawStyle>? DrawStyleChanged;
    }
}
