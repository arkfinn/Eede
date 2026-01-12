using Avalonia;
using Avalonia.Controls;
using Eede.Domain.ImageEditing.DrawingTools;
using ReactiveUI;
using System;
using System.Reactive;

namespace Eede.Presentation.Views.Navigation
{
    public partial class DrawStyleMenu : UserControl
    {
        public ReactiveCommand<DrawStyleType, Unit> UpdateDrawStyleCommand { get; }

        public DrawStyleMenu()
        {
            InitializeComponent();

            // コマンドの初期化
            UpdateDrawStyleCommand = ReactiveCommand.Create<DrawStyleType>(style =>
            {
                DrawStyle = style;
            });

            UpdateChecked(); // 初期状態のUIを設定
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

        private void UpdateChecked()
        {
            ButtonRegionSelector.IsChecked = DrawStyle == DrawStyleType.RegionSelect;
            ButtonFreeCurve.IsChecked = DrawStyle == DrawStyleType.FreeCurve;
            ButtonLine.IsChecked = DrawStyle == DrawStyleType.Line;
            ButtonFill.IsChecked = DrawStyle == DrawStyleType.Fill;
            ButtonRect.IsChecked = DrawStyle == DrawStyleType.Rectangle;
            ButtonRectFill.IsChecked = DrawStyle == DrawStyleType.FilledRectangle;
        }

        public event Action<DrawStyleType>? DrawStyleChanged;
    }
}
