using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eede.Common.Drawings;
using Eede.Domain.DrawStyles;
using System;
using System.Windows.Input;

namespace Eede.Views.Navigation
{
    public partial class DrawStyleMenu : UserControl
    {
        public DrawStyleMenu()
        {
            InitializeComponent();
            UpdateChecked();
        }

        public static readonly StyledProperty<DrawStyles> DrawStyleProperty =
            AvaloniaProperty.Register<DrawStyleMenu, DrawStyles>(nameof(DrawStyle), DrawStyles.Free, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
        public DrawStyles DrawStyle
        {
            get => GetValue(DrawStyleProperty);
            set
            {
                SetValue(DrawStyleProperty, value);
                UpdateChecked();
                DrawStyleChanged?.Invoke(GetValue(DrawStyleProperty));
            }
        }

        public void SetDrawStyleRegionSelector(object sender, RoutedEventArgs e)
        {
            DrawStyle = DrawStyles.RegionSelect;
        }

        public void SetDrawStyleFreeCurve(object sender, RoutedEventArgs e)
        {
            DrawStyle = DrawStyles.Free;
        }

        public void SetDrawStyleLine(object sender, RoutedEventArgs e)
        {
            DrawStyle = DrawStyles.Line;
        }

        public void SetDrawStyleFill(object sender, RoutedEventArgs e)
        {
            DrawStyle = DrawStyles.Fill;
        }

        private void UpdateChecked()
        {
            ButtonRegionSelector.IsChecked = DrawStyle == DrawStyles.RegionSelect;
            ButtonFreeVurve.IsChecked = DrawStyle ==DrawStyles.Free;
            ButtonLine.IsChecked = DrawStyle == DrawStyles.Line;
            ButtonFill.IsChecked = DrawStyle == DrawStyles.Fill;
        }

        public event Action<DrawStyles>? DrawStyleChanged;
    }
}
