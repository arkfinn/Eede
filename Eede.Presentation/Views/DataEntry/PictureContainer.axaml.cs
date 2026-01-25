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
using Eede.Application.Common.SelectionStates;
using Eede.Presentation.ViewModels.DataDisplay;
using ReactiveUI;
using System;
using System.Windows.Input;
using Eede.Domain.Animations;

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
            _selectionState = CreateInitialState();

            _viewModel.AnimationViewModel.WhenAnyValue(x => x.IsAnimationMode)
                .Subscribe(_ =>
                {
                    _selectionState = CreateInitialState();
                    UpdateCursor();
                });

            // GridView のバインディング設定
            gridOverlay.Magnification = new Magnification(1);
            _ = gridOverlay.Bind(IsVisibleProperty, new Binding
            {
                Source = _viewModel.AnimationViewModel,
                Path = nameof(_viewModel.AnimationViewModel.IsAnimationMode)
            });
            _ = gridOverlay.Bind(General.GridView.GridSettingsProperty, new Binding
            {
                Source = _viewModel.AnimationViewModel,
                Path = "SelectedPattern.Grid"
            });
            _ = gridOverlay.Bind(WidthProperty, new Binding
            {
                Source = canvas,
                Path = nameof(canvas.Width)
            });
            _ = gridOverlay.Bind(HeightProperty, new Binding
            {
                Source = canvas,
                Path = nameof(canvas.Height)
            });
        }

        private ISelectionState CreateInitialState()
        {
            if (_viewModel?.AnimationViewModel.IsAnimationMode == true)
            {
                var animVM = _viewModel.AnimationViewModel;
                var currentGrid = new GridSettings(new PictureSize(animVM.GridWidth, animVM.GridHeight), new Position(0, 0), 0);
                return new AnimationEditingState(animVM, currentGrid, _viewModel.PictureBuffer.Size);
            }
            return new NormalCursorState(_viewModel!.GlobalState.CursorArea);
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
                if (_viewModel.AnimationViewModel.IsAnimationMode)
                {
                    // アニメーションモード時はアニメーション設定のグリッドサイズを使用
                    var animVM = _viewModel.AnimationViewModel;
                    cursorArea = HalfBoxArea.Create(cursorArea.RealPosition, new PictureSize(animVM.GridWidth, animVM.GridHeight));
                }

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

            var nowPosition = PointToPosition(e.GetPosition(canvas));

            // アニメーションモード時はモードに応じたステートを再作成（グリッド設定の変更を反映するため）
            if (_viewModel.AnimationViewModel.IsAnimationMode)
            {
                _selectionState = CreateInitialState();
            }

            var currentCursorArea = _viewModel.GlobalState.CursorArea.Move(nowPosition);
            if (_viewModel.AnimationViewModel.IsAnimationMode)
            {
                currentCursorArea = HalfBoxArea.Create(nowPosition, new PictureSize(_viewModel.AnimationViewModel.GridWidth, _viewModel.AnimationViewModel.GridHeight));
            }

            switch (e.GetCurrentPoint(canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    if (_viewModel.AnimationViewModel.IsAnimationMode)
                    {
                        _selectionState = _selectionState.HandlePointerLeftButtonPressed(currentCursorArea, nowPosition, null, () => _viewModel.PictureBuffer, null);
                    }
                    else
                    {
                        // ドックエリアでは移動機能は不要なため、常に転送（Pull）を実行する
                        PicturePullAction?.Execute(currentCursorArea.RealPosition);
                        // 状態は NormalCursorState にリセット/維持する
                        _selectionState = new NormalCursorState(currentCursorArea);
                    }
                    break;

                case PointerUpdateKind.RightButtonPressed:
                    OnPointerRightButtonPressed(nowPosition);
                    break;
            }
            UpdateCursor();
        }

        private void OnPointerRightButtonPressed(Position nowPosition)
        {
            if (_viewModel == null) return;
            // 範囲選択を開始するために、現在のステートに右クリックを通知
            var (newState, newArea) = _selectionState.HandlePointerRightButtonPressed(_viewModel.GlobalState.CursorArea, nowPosition, MinCursorSize, PictureUpdateAction);

            _selectionState = newState;
            _viewModel.GlobalState.CursorArea = newArea;
            UpdateCursor();
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_viewModel == null) return;

            Position nowPosition = PointToPosition(e.GetPosition(canvas));

            var cursorArea = _viewModel.GlobalState.CursorArea;
            if (_viewModel.AnimationViewModel.IsAnimationMode && _selectionState is not RegionSelectingState)
            {
                // アニメーションモード中で、かつ範囲選択中でない場合のみグリッドサイズを適用
                cursorArea = HalfBoxArea.Create(nowPosition, new PictureSize(_viewModel.AnimationViewModel.GridWidth, _viewModel.AnimationViewModel.GridHeight));
            }

            var (newVisible, newArea) = _selectionState.HandlePointerMoved(cursorArea, VisibleCursor, nowPosition, CanvasSize);
            VisibleCursor = newVisible;
            _viewModel.GlobalState.CursorArea = newArea;
            UpdateCursor();
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!VisibleCursor || _viewModel == null) return;

            var nowPosition = PointToPosition(e.GetPosition(canvas));
            switch (e.GetCurrentPoint(canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonReleased:
                    _selectionState = _selectionState.HandlePointerLeftButtonReleased(_viewModel.GlobalState.CursorArea, nowPosition, PicturePushAction, PictureUpdateAction);
                    break;

                case PointerUpdateKind.RightButtonReleased:
                    OnPointerRightButtonReleased(_viewModel);
                    break;
            }
        }

        private void OnPointerRightButtonReleased(DockPictureViewModel vm)
        {
            // 範囲選択状態からの遷移を確認
            var isSelecting = _selectionState is RegionSelectingState;
            var (newState, newArea) = _selectionState.HandlePointerRightButtonReleased(vm.GlobalState.CursorArea, PicturePushAction);

            // 範囲選択が終わった直後なら、作業エリアへ転送を実行する
            if (isSelecting && newState is SelectedState)
            {
                PicturePushAction?.Execute(newArea.CreateRealArea(newArea.BoxSize));
                // ドックエリアでは移動状態（SelectedState）を維持せず、通常状態に戻す
                _selectionState = CreateInitialState();
            }
            else
            {
                _selectionState = newState;
            }

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
