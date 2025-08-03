using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Eede.Domain.DrawStyles;
using System;

namespace Eede.Presentation.Views.Navigation
{
    public partial class DrawStyleMenu : UserControl
    {
        public DrawStyleMenu()
        {
            InitializeComponent();
            UpdateChecked();
        }

        public static readonly StyledProperty<DrawStyleType> DrawStyleProperty =
            AvaloniaProperty.Register<DrawStyleMenu, DrawStyleType>(nameof(DrawStyle), DrawStyleType.FreeCurve, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
        public DrawStyleType DrawStyle
        {
            get => GetValue(DrawStyleProperty);
            set
            {
                _ = SetValue(DrawStyleProperty, value);
                UpdateChecked();
                DrawStyleChanged?.Invoke(GetValue(DrawStyleProperty));
            }
        }

        public void SetDrawStyleRegionSelector(object sender, RoutedEventArgs e)
        {
            DrawStyle = DrawStyleType.RegionSelect;
        }

        public void SetDrawStyleFreeCurve(object sender, RoutedEventArgs e)
        {
            DrawStyle = DrawStyleType.FreeCurve;
        }

        public void SetDrawStyleLine(object sender, RoutedEventArgs e)
        {
            DrawStyle = DrawStyleType.Line;
        }

        public void SetDrawStyleFill(object sender, RoutedEventArgs e)
        {
            DrawStyle = DrawStyleType.Fill;
        }

        private void UpdateChecked()
        {
            ButtonRegionSelector.IsChecked = DrawStyle == DrawStyleType.RegionSelect;
            ButtonFreeVurve.IsChecked = DrawStyle == DrawStyleType.FreeCurve;
            ButtonLine.IsChecked = DrawStyle == DrawStyleType.Line;
            ButtonFill.IsChecked = DrawStyle == DrawStyleType.Fill;
        }

        public event Action<DrawStyleType>? DrawStyleChanged;
    }
}
