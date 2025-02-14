using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Eede.Application.Pictures;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
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
            return IsSelecting ? SelectingArea : CursorArea;
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
                HalfBoxArea.Create(new PictureSize(32, 32), new Position(0, 0)), defaultBindingMode: BindingMode.TwoWay);
        public HalfBoxArea CursorArea
        {
            get => GetValue(CursorAreaProperty);
            set => SetValue(CursorAreaProperty, value);
        }

        private bool IsSelecting = false;
        private HalfBoxArea SelectingArea = HalfBoxArea.Create(new PictureSize(32, 32), new Position(0, 0));
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
                    if (!IsSelecting)
                    {
                        //var picture = BringPictureBuffer();
                        //PicturePulled?.Invoke(this, new PicturePullEventArgs(picture, CursorArea.RealPosition));
                        PicturePullAction?.Execute(CursorArea.RealPosition);
                    }
                    break;

                case PointerUpdateKind.RightButtonPressed:
                    // �͈͑I���J�n
                    IsSelecting = true;
                    SelectingArea = HalfBoxArea.Create(MinCursorSize, PointToPosition(e.GetPosition(canvas)));
                    break;
            }
            UpdateCursor();
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            Position nowPosition = PointToPosition(e.GetPosition(canvas));
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
            switch (e.GetCurrentPoint(canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonReleased:
                    break;

                case PointerUpdateKind.RightButtonReleased:
                    // �͈͑I�����Ă�Ƃ�
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

