using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Eede.Application.Pictures;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.ViewModels.DataDisplay;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace Eede.Views.DataEntry
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
            if (DataContext is DockPictureViewModel vm)
            {
                return vm;
            }
            if (DataContext is StyledElement e)
            {
                return e.DataContext as DockPictureViewModel;
            }
            return null;
        }
        private void PictureContainer_DataContextChanged(object? sender, EventArgs e)
        {
            var vm = FetchViewModel();
            if (vm == null)
            {
                return;
            }
            var bitmap = vm.Bitmap;

            CanvasSize = new PictureSize((int)bitmap.Size.Width, (int)bitmap.Size.Height);

            // Dockableは独自のDataContextを割り当てられるので手動で値の変更やバインディングを行う
            background.Width = bitmap.Size.Width;
            background.Height = bitmap.Size.Height;
            canvas.Width = bitmap.Size.Width;
            canvas.Height = bitmap.Size.Height;
            ImageBrush canvasBrush = new();
            canvasBrush.Bind(ImageBrush.SourceProperty, new Binding
            {
                Source = vm,
                Path = nameof(vm.Bitmap)
            });
            canvas.Background = canvasBrush;
            this.Bind(MinCursorSizeProperty, new Binding
            {
                Source = vm,
                Path = nameof(vm.MinCursorSize)
            });
            PicturePushAction = vm.OnPicturePush;
            PicturePullAction = vm.OnPicturePull;
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
                var cursorArea = GetNowCursorArea();
                var grid = cursorArea.GridPosition;
                var size = cursorArea.BoxSize;

                cursor.Width = size.Width;
                cursor.Height = size.Height;
                cursor.Margin = new Thickness(cursorArea.RealPosition.X ,cursorArea.RealPosition.Y, 0, 0);

            });

        }

        private HalfBoxArea GetNowCursorArea()
        {
            if (IsSelecting)
            {
                return SelectingArea;
            }
            else
            {
                return CursorArea;
            }
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
            AvaloniaProperty.Register<PictureContainer, PictureSize>(nameof(MinCursorSize), new PictureSize(32, 32), defaultBindingMode:BindingMode.TwoWay);
        public PictureSize MinCursorSize
        {
            get => GetValue(MinCursorSizeProperty);
            set {
                SetValue(MinCursorSizeProperty, value);
                CursorArea = CursorArea.UpdateSize(value);
            }
        }

        private bool IsSelecting = false;
        private HalfBoxArea SelectingArea = HalfBoxArea.Create(new PictureSize(32, 32), new Position(0, 0));
        private HalfBoxArea CursorArea = HalfBoxArea.Create(new PictureSize(32, 32), new Position(0, 0));
        public event EventHandler<PicturePullEventArgs> PicturePulled;
        public event EventHandler<PicturePushEventArgs> PicturePushed;

        private PictureSize CanvasSize = new PictureSize(32, 32);
   


        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!VisibleCursor)
            {
                return;
            }

            switch (e.GetCurrentPoint(this.canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    if (!IsSelecting)
                    {
                        //var picture = BringPictureBuffer();
                        //PicturePulled?.Invoke(this, new PicturePullEventArgs(picture, CursorArea.RealPosition));
                        PicturePullAction?.Execute(CursorArea.RealPosition);
                    }
                    break;

                case PointerUpdateKind.RightButtonPressed:
                    // 範囲選択開始
                    IsSelecting = true;
                    SelectingArea = HalfBoxArea.Create(MinCursorSize, PointToPosition(e.GetPosition(this.canvas)));
                    break;
            }
            UpdateCursor();
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            Position nowPosition = PointToPosition(e.GetPosition(this.canvas));
            if (IsSelecting)
            {
                SelectingArea = SelectingArea.UpdatePosition(nowPosition, CanvasSize);
            }
            else
            {
                VisibleCursor = CanvasSize.Contains(nowPosition);
                CursorArea = CursorArea.Move(nowPosition);
            }
            UpdateCursor();
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            switch (e.GetCurrentPoint(this.canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonReleased:
                    break;

                case PointerUpdateKind.RightButtonReleased:
                    // 範囲選択してるとき
                    if (IsSelecting)
                    {
                        PictureArea area = SelectingArea.CreateRealArea(SelectingArea.BoxSize);
                        //Rectangle rect = new(area.X, area.Y, area.Width, area.Height);
                        //PicturePushed?.Invoke(this, new PicturePushEventArgs(picture, area));
                        PicturePushAction?.Execute(area);
                        CursorArea = SelectingArea;
                        IsSelecting = false;
                    }
                    break;
            }
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

