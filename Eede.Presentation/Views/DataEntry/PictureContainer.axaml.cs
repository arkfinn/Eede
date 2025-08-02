using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Eede.Application.Pictures;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Sizes;
using Eede.Presentation.Common.SelectionStates;
using Eede.Presentation.ViewModels.DataDisplay;
using System;
using System.Windows.Input;

namespace Eede.Presentation.Views.DataEntry
{
    public partial class PictureContainer : UserControl
    {
        public PictureContainer()
        {
            InitializeComponent();
            DataContextChanged += PictureContainer_DataContextChanged;
            canvas.PointerPressed += OnPointerPressed;
            canvas.PointerMoved += OnPointerMoved;
            canvas.PointerReleased += OnPointerReleased;
            canvas.PointerExited += OnPointerExited;
        }

        private DockPictureViewModel? FetchViewModel()
        {
            return DataContext is DockPictureViewModel vm ? vm : DataContext is StyledElement e ? e.DataContext as DockPictureViewModel : null;
        }

        private void PictureContainer_DataContextChanged(object? sender, EventArgs e)
        {
            DockPictureViewModel vm = FetchViewModel();
            if (vm == null)
            {
                return;
            }
            Avalonia.Media.Imaging.Bitmap bitmap = vm.PremultipliedBitmap;

            CanvasSize = new PictureSize((int)bitmap.Size.Width, (int)bitmap.Size.Height);

            background.Width = bitmap.Size.Width;
            background.Height = bitmap.Size.Height;
            canvas.Width = bitmap.Size.Width;
            canvas.Height = bitmap.Size.Height;
            ImageBrush canvasBrush = new();
            _ = canvasBrush.Bind(ImageBrush.SourceProperty, new Binding
            {
                Source = vm,
                Path = nameof(vm.PremultipliedBitmap)
            });
            canvas.Background = canvasBrush;
            MinCursorSize = vm.MinCursorSize;
            _ = Bind(MinCursorSizeProperty, new Binding
            {
                Source = vm,
                Path = nameof(vm.MinCursorSize)
            });
            CursorArea = vm.CursorArea;
            _ = Bind(CursorAreaProperty, new Binding
            {
                Source = vm,
                Path = nameof(vm.CursorArea)
            });
            PicturePushAction = vm.OnPicturePush;
            PicturePullAction = vm.OnPicturePull;
            SelectionState = new NormalCursorState(CursorArea, CursorArea);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            UpdateCursor();
        }

        private void UpdateCursor()
        {
            Dispatcher.UIThread.Post(() =>
            {
                HalfBoxArea cursorArea = GetNowCursorArea();
                Position grid = cursorArea.GridPosition;
                PictureSize size = cursorArea.BoxSize;

                cursor.Width = size.Width;
                cursor.Height = size.Height;
                cursor.Margin = new Thickness(cursorArea.RealPosition.X, cursorArea.RealPosition.Y, 0, 0);
            });
        }

        private HalfBoxArea GetNowCursorArea()
        {
            return SelectionState.GetCurrentArea();
        }

        private bool _visibleCursor = false;
        public bool VisibleCursor
        {
            get => _visibleCursor;
            set
            {
                _visibleCursor = value;
                cursor.IsVisible = _visibleCursor;
            }
        }

        public static readonly StyledProperty<ICommand?> PicturePushActionProperty =
            AvaloniaProperty.Register<PictureContainer, ICommand?>(nameof(PicturePushAction));
        public ICommand? PicturePushAction
        {
            get => GetValue(PicturePushActionProperty);
            set => SetValue(PicturePushActionProperty, value);
        }

        public static readonly StyledProperty<ICommand?> PicturePullActionProperty =
            AvaloniaProperty.Register<PictureContainer, ICommand?>(nameof(PicturePullAction));
        public ICommand? PicturePullAction
        {
            get => GetValue(PicturePullActionProperty);
            set => SetValue(PicturePullActionProperty, value);
        }

        public static readonly StyledProperty<PictureSize> MinCursorSizeProperty =
            AvaloniaProperty.Register<PictureContainer, PictureSize>(nameof(MinCursorSize), new PictureSize(32, 32));
        public PictureSize MinCursorSize
        {
            get => GetValue(MinCursorSizeProperty);
            set => SetValue(MinCursorSizeProperty, value);
        }

        public static readonly StyledProperty<HalfBoxArea> CursorAreaProperty =
            AvaloniaProperty.Register<PictureContainer, HalfBoxArea>(nameof(CursorArea),
                HalfBoxArea.Create(new Position(0, 0), new PictureSize(32, 32)), defaultBindingMode: BindingMode.TwoWay);
        public HalfBoxArea CursorArea
        {
            get => GetValue(CursorAreaProperty);
            set => SetValue(CursorAreaProperty, value);
        }

        private ISelectionState SelectionState;
        public event EventHandler<PicturePullEventArgs> PicturePulled;
        public event EventHandler<PicturePushEventArgs> PicturePushed;

        private PictureSize CanvasSize = new(32, 32);

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!VisibleCursor)
            {
                return;
            }

            switch (e.GetCurrentPoint(canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    SelectionState.HandlePointerLeftButtonPressed(PicturePullAction);
                    break;

                case PointerUpdateKind.RightButtonPressed:
                    OnPointerRightButtonPressed(PointToPosition(e.GetPosition(canvas)));
                    break;
            }
            UpdateCursor();
        }

        private void OnPointerRightButtonPressed(Position nowPosition)
        {
            SelectionState = SelectionState.HandlePointerRightButtonPressed(nowPosition, MinCursorSize);
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            Position nowPosition = PointToPosition(e.GetPosition(canvas));
            (VisibleCursor, CursorArea) = SelectionState.HandlePointerMoved(VisibleCursor, nowPosition, CanvasSize);
            UpdateCursor();
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            switch (e.GetCurrentPoint(canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonReleased:
                    break;

                case PointerUpdateKind.RightButtonReleased:
                    OnPointerRightButtonReleased();
                    break;
            }
        }

        private void OnPointerRightButtonReleased()
        {
            (CursorArea, SelectionState) = SelectionState.HandlePointerRightButtonReleased(PicturePushAction);
        }

        private void OnPointerExited(object? sender, PointerEventArgs e)
        {
            VisibleCursor = false;
            UpdateCursor();
        }

        private static Position PointToPosition(Point point)
        {
            return new((int)point.X, (int)point.Y);
        }
    }
}
