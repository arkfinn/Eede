using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.SelectionStates;
using Eede.Presentation.ViewModels.DataDisplay;
using System;
using System.Windows.Input;

namespace Eede.Presentation.Views.DataEntry
{
    public partial class PictureContainer : UserControl
    {
        private ISelectionState _selectionState;
        private DockPictureViewModel? _viewModel;
        private bool _visibleCursor = false;

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
            _viewModel = FetchViewModel();
            if (_viewModel == null)
            {
                return;
            }
            Avalonia.Media.Imaging.Bitmap bitmap = _viewModel.PremultipliedBitmap;

            CanvasSize = new PictureSize((int)bitmap.Size.Width, (int)bitmap.Size.Height);

            background.Width = bitmap.Size.Width;
            background.Height = bitmap.Size.Height;
            canvas.Width = bitmap.Size.Width;
            canvas.Height = bitmap.Size.Height;
            ImageBrush canvasBrush = new();
            _ = canvasBrush.Bind(ImageBrush.SourceProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.PremultipliedBitmap)
            });
            canvas.Background = canvasBrush;
            MinCursorSize = _viewModel.MinCursorSize;
            _ = Bind(MinCursorSizeProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.MinCursorSize)
            });
            PicturePushAction = _viewModel.OnPicturePush;
            PicturePullAction = _viewModel.OnPicturePull;
            PictureUpdateAction = _viewModel.OnPictureUpdate;

            // SelectionState の初期化: GlobalState.CursorArea を初期値として使用
            var initialCursorArea = _viewModel.GlobalState.CursorArea;
            _selectionState = new NormalCursorState(initialCursorArea);
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
                if (_viewModel == null) return;

                HalfBoxArea cursorArea = _viewModel.GlobalState.CursorArea;
                Position grid = cursorArea.GridPosition;
                PictureSize size = cursorArea.BoxSize;

                cursor.Width = size.Width;
                cursor.Height = size.Height;
                cursor.Margin = new Thickness(cursorArea.RealPosition.X, cursorArea.RealPosition.Y, 0, 0);

                UpdateSelectionPreview();
            });
        }

        private void UpdateSelectionPreview()
        {
            var info = _selectionState.GetSelectionPreviewInfo();
            if (info == null)
            {
                selectionPreview.IsVisible = false;
                return;
            }

            selectionPreview.IsVisible = true;
            selectionPreview.Source = PictureBitmapAdapter.ConvertToPremultipliedBitmap(info.Pixels);
            selectionPreview.Width = info.Pixels.Width;
            selectionPreview.Height = info.Pixels.Height;
            selectionPreview.Margin = new Thickness(info.Position.X, info.Position.Y, 0, 0);
        }

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

        public static readonly StyledProperty<ICommand?> PictureUpdateActionProperty =
            AvaloniaProperty.Register<PictureContainer, ICommand?>(nameof(PictureUpdateAction));
        public ICommand? PictureUpdateAction
        {
            get => GetValue(PictureUpdateActionProperty);
            set => SetValue(PictureUpdateActionProperty, value);
        }

        public static readonly StyledProperty<PictureSize> MinCursorSizeProperty =
            AvaloniaProperty.Register<PictureContainer, PictureSize>(nameof(MinCursorSize), new PictureSize(32, 32));
        public PictureSize MinCursorSize
        {
            get => GetValue(MinCursorSizeProperty);
            set => SetValue(MinCursorSizeProperty, value);
        }

        public event EventHandler<PicturePullEventArgs> PicturePulled;
        public event EventHandler<PicturePushEventArgs> PicturePushed;

        private PictureSize CanvasSize = new(32, 32);

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!VisibleCursor || _viewModel == null)
            {
                return;
            }

            var currentCursorArea = _viewModel.GlobalState.CursorArea;
            switch (e.GetCurrentPoint(canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    _selectionState = _selectionState.HandlePointerLeftButtonPressed(currentCursorArea, () =>
                    {
                        PicturePullAction?.Execute(currentCursorArea.RealPosition);
                        return _viewModel.PictureBuffer;
                    });
                    break;

                case PointerUpdateKind.RightButtonPressed:
                    OnPointerRightButtonPressed(PointToPosition(e.GetPosition(canvas)));
                    break;
            }
            UpdateCursor();
        }

        private void OnPointerRightButtonPressed(Position nowPosition)
        {
            if (_viewModel == null) return;
            var (newState, newArea) = _selectionState.HandlePointerRightButtonPressed(_viewModel.GlobalState.CursorArea, nowPosition, MinCursorSize);
            _selectionState = newState;
            _viewModel.GlobalState.CursorArea = newArea;
            UpdateCursor();
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_viewModel == null) return;

            Position nowPosition = PointToPosition(e.GetPosition(canvas));
            var (newVisible, newArea) = _selectionState.HandlePointerMoved(_viewModel.GlobalState.CursorArea, VisibleCursor, nowPosition, CanvasSize);
            VisibleCursor = newVisible;
            _viewModel.GlobalState.CursorArea = newArea;
            UpdateCursor();
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!VisibleCursor || _viewModel == null) return;

            switch (e.GetCurrentPoint(canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonReleased:
                    _selectionState = _selectionState.HandlePointerLeftButtonReleased(_viewModel.GlobalState.CursorArea, PicturePushAction, PictureUpdateAction);
                    break;

                case PointerUpdateKind.RightButtonReleased:
                    OnPointerRightButtonReleased(_viewModel);
                    break;
            }
        }

        private void OnPointerRightButtonReleased(DockPictureViewModel vm)
        {
            var (newState, newArea) = _selectionState.HandlePointerRightButtonReleased(vm.GlobalState.CursorArea, PicturePushAction);
            _selectionState = newState;
            vm.GlobalState.CursorArea = newArea;
            UpdateCursor();
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
