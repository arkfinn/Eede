using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Eede.Application.Drawings;
using Eede.Domain.Colors;
using Eede.Domain.Drawings;
using Eede.Domain.DrawStyles;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using Eede.Services;
using Eede.ViewModels.DataEntry;
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Eede.Views.DataEntry
{
    public partial class DrawableCanvas : UserControl
    {
        public DrawableCanvas()
        {
            InitializeComponent();

            canvas.PointerPressed += OnCanvasPointerPressed;
            canvas.PointerMoved += OnCvanvasPointerMoved;
            canvas.PointerReleased += OnCanvasPointerReleased;
            canvas.PointerExited += OnCanvasPointerExited;
            canvas.KeyDown += OnKeyDown;
            canvas.KeyUp += OnKeyUp;
        }

        public static readonly StyledProperty<ICommand?> PointerLeftButtonPressedCommandProperty =
            AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerLeftButtonPressedCommand));
        public ICommand? PointerLeftButtonPressedCommand
        {
            get => GetValue(PointerLeftButtonPressedCommandProperty);
            set => SetValue(PointerLeftButtonPressedCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand?> PointerMovedCommandProperty =
            AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerMovedCommand));
        public ICommand? PointerMovedCommand
        {
            get => GetValue(PointerMovedCommandProperty);
            set => SetValue(PointerMovedCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand?> PointerLeftButtonReleasedCommandProperty =
            AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerLeftButtonReleasedCommand));
        public ICommand? PointerLeftButtonReleasedCommand
        {
            get => GetValue(PointerLeftButtonReleasedCommandProperty);
            set => SetValue(PointerLeftButtonReleasedCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand?> PointerRightButtonPressedCommandProperty =
            AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerRightButtonPressedCommand));
        public ICommand? PointerRightButtonPressedCommand
        {
            get => GetValue(PointerRightButtonPressedCommandProperty);
            set => SetValue(PointerRightButtonPressedCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand?> PointerLeaveCommandProperty =
            AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerLeaveCommand));
        public ICommand? PointerLeaveCommand
        {
            get => GetValue(PointerLeaveCommandProperty);
            set => SetValue(PointerLeaveCommandProperty, value);
        }

        public static readonly StyledProperty<bool> IsShiftedProperty =
            AvaloniaProperty.Register<DrawableCanvas, bool>(nameof(IsShifted), false, defaultBindingMode: BindingMode.TwoWay);
        public bool IsShifted
        {
            get => GetValue(IsShiftedProperty);
            set => SetValue(IsShiftedProperty, value);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
            {
                IsShifted = true;
            }
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if ((e.KeyModifiers & KeyModifiers.Shift) == 0)
            {
                IsShifted = false;
            }
        }

        private bool IsLeftButtonPressing = false;
        //private bool IsRightButtonPressing = false;
        private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var pos = e.GetPosition(this.canvas);
            var pointer = e.GetCurrentPoint(this.canvas).Properties;
            if (pointer.IsLeftButtonPressed)
            {
                PointerLeftButtonPressedCommand?.Execute(new Position((int)pos.X, (int)pos.Y));
                IsLeftButtonPressing = true;
            }

            if (pointer.IsRightButtonPressed)
            {
                PointerRightButtonPressedCommand?.Execute(new Position((int)pos.X, (int)pos.Y));
            }
        }

        private void OnCvanvasPointerMoved(object? sender, PointerEventArgs e)
        {
            var pos = e.GetPosition(this.canvas);
            if (IsLeftButtonPressing && e.GetCurrentPoint(this.canvas).Properties.IsRightButtonPressed)
            {
                PointerRightButtonPressedCommand?.Execute(new Position((int)pos.X, (int)pos.Y));
                IsLeftButtonPressing = false;
            }
            PointerMovedCommand?.Execute(new Position((int)pos.X, (int)pos.Y));
        }

        private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (IsLeftButtonPressing)
            {
                var pos = e.GetPosition(this.canvas);
                PointerLeftButtonReleasedCommand?.Execute(new Position((int)pos.X, (int)pos.Y));
            }
            IsLeftButtonPressing = false;
        }

        private void OnCanvasPointerExited(object? sender, EventArgs e)
        {
            PointerLeaveCommand?.Execute(null);
        }
    }
}
