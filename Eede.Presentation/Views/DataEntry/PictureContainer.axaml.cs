using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Eede.Application.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.ViewModels.DataDisplay;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Eede.Presentation.Views.DataEntry
{
    #nullable enable

    public partial class PictureContainer : UserControl
    {
        private ISelectionState _selectionState;
        private DockPictureViewModel? _viewModel;
        private bool _visibleCursor = false;
        private HalfBoxArea _localCursorArea;
        private PictureSize _cursorSize = new(32, 32);

        public PictureContainer()
        {
            InitializeComponent();
            DataContextChanged += PictureContainer_DataContextChanged;
            canvas.PointerPressed += OnPointerPressed;
            canvas.PointerMoved += OnPointerMoved;
            canvas.PointerReleased += OnPointerReleased;
            canvas.PointerExited += OnPointerExited;
            _localCursorArea = HalfBoxArea.Create(new Position(0, 0), new PictureSize(32, 32));
            _selectionState = new NormalCursorState(_localCursorArea);
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
            Avalonia.Media.Imaging.Bitmap bitmap = _viewModel.PremultipliedBitmap!;

            CanvasSize = new PictureSize((int)bitmap.Size.Width, (int)bitmap.Size.Height);

            _ = background.Bind(WidthProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.DisplayWidth)
            });
            _ = background.Bind(HeightProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.DisplayHeight)
            });
            _ = canvas.Bind(WidthProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.DisplayWidth)
            });
            _ = canvas.Bind(HeightProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.DisplayHeight)
            });
            _ = mainImage.Bind(WidthProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.DisplayWidth)
            });
            _ = mainImage.Bind(HeightProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.DisplayHeight)
            });
            MinCursorSize = _viewModel.MinCursorSize;
            _ = Bind(MinCursorSizeProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.MinCursorSize)
            });
            PicturePushAction = _viewModel.OnPicturePush;
            PicturePullAction = _viewModel.OnPicturePull;
            PictureUpdateAction = _viewModel.OnPictureUpdate;

            // 初期カーソルサイズの設定
            _cursorSize = _viewModel.GlobalState.BoxSize;

            // SelectionState の初期化
            _localCursorArea = HalfBoxArea.Create(new Position(0, 0), _cursorSize);
            _selectionState = CreateInitialState();

            _viewModel.AnimationViewModel.WhenAnyValue(x => x.IsAnimationMode)
                .Subscribe(_ =>
                {
                    _selectionState = CreateInitialState();
                    UpdateCursor();
                });

            _viewModel.WhenAnyValue(x => x.Magnification)
                .Subscribe(_ =>
                {
                    UpdateCursor();
                    UpdateChecked();
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
            _ = gridOverlay.Bind(General.GridView.MagnificationProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.Magnification)
            });
            _ = gridOverlay.Bind(WidthProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.DisplayWidth)
            });
            _ = gridOverlay.Bind(HeightProperty, new Binding
            {
                Source = _viewModel,
                Path = nameof(_viewModel.DisplayHeight)
            });
        }

        private void UpdateChecked()
        {
            if (_viewModel == null) return;
            float val = _viewModel.Magnification.Value;
            m1.IsChecked = val == 1;
            m2.IsChecked = val == 2;
            m4.IsChecked = val == 4;
            m6.IsChecked = val == 6;
            m8.IsChecked = val == 8;
            m12.IsChecked = val == 12;
        }

        private void SetMagnification1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_viewModel != null) _viewModel.Magnification = new Magnification(1);
        }

        private void SetMagnification2(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_viewModel != null) _viewModel.Magnification = new Magnification(2);
        }

        private void SetMagnification4(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_viewModel != null) _viewModel.Magnification = new Magnification(4);
        }

        private void SetMagnification6(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_viewModel != null) _viewModel.Magnification = new Magnification(6);
        }

        private void SetMagnification8(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_viewModel != null) _viewModel.Magnification = new Magnification(8);
        }

        private void SetMagnification12(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_viewModel != null) _viewModel.Magnification = new Magnification(12);
        }

        private ISelectionState CreateInitialState()
        {
            if (_viewModel?.AnimationViewModel.IsAnimationMode == true)
            {
                var animVM = _viewModel.AnimationViewModel;
                var currentGrid = new GridSettings(new PictureSize(animVM.GridWidth, animVM.GridHeight), new Position(0, 0), 0);
                return new AnimationEditingState(animVM, currentGrid, _viewModel.PictureBuffer.Size);
            }
            return new NormalCursorState(_localCursorArea);
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

                var mag = _viewModel.Magnification;
                HalfBoxArea cursorArea = _localCursorArea;
                if (_viewModel.AnimationViewModel.IsAnimationMode)
                {
                    // アニメーションモード時はアニメーション設定のグリッドサイズを使用
                    var animVM = _viewModel.AnimationViewModel;
                    cursorArea = HalfBoxArea.Create(cursorArea.RealPosition, new PictureSize(animVM.GridWidth, animVM.GridHeight));
                }

                var selectingArea = _selectionState.GetSelectingArea();
                if (selectingArea.HasValue)
                {
                    var displayPos = new CanvasCoordinate(selectingArea.Value.X, selectingArea.Value.Y).ToDisplay(mag);
                    var displaySize = new CanvasCoordinate(selectingArea.Value.Width, selectingArea.Value.Height).ToDisplay(mag);

                    cursor.Width = displaySize.X;
                    cursor.Height = displaySize.Y;
                    cursor.Margin = new Thickness(displayPos.X, displayPos.Y, 0, 0);
                    cursor.ShowHandles = _selectionState is SelectedState || _selectionState is ResizingState;

                    // ハンドルサイズはキャンバス上の 4ピクセル固定
                    double visualSize = 4.0;
                    cursor.HandleSize = visualSize;
                    cursor.HandleMargin = new Thickness(-2, -2, 0, 0);
                }
                else
                {
                    var displayPos = new CanvasCoordinate(cursorArea.RealPosition.X, cursorArea.RealPosition.Y).ToDisplay(mag);
                    var displaySize = new CanvasCoordinate(cursorArea.BoxSize.Width, cursorArea.BoxSize.Height).ToDisplay(mag);

                    cursor.Width = displaySize.X;
                    cursor.Height = displaySize.Y;
                    cursor.Margin = new Thickness(displayPos.X, displayPos.Y, 0, 0);
                    cursor.ShowHandles = false;
                }

                UpdateSelectionPreview();
            });
        }

        private void UpdateCursorCursor(Position mousePos)
        {
            if (_viewModel == null) return;
            var selectionCursor = _selectionState.GetCursor(mousePos, 4);
            _viewModel.AnimationCursor = selectionCursor switch
            {
                SelectionCursor.Move => new Cursor(StandardCursorType.SizeAll),
                SelectionCursor.SizeNWSE => new Cursor(StandardCursorType.TopLeftCorner),
                SelectionCursor.SizeNESW => new Cursor(StandardCursorType.TopRightCorner),
                SelectionCursor.SizeNS => new Cursor(StandardCursorType.TopSide),
                SelectionCursor.SizeWE => new Cursor(StandardCursorType.LeftSide),
                _ => Cursor.Default
            };
        }

        private void UpdateSelectionPreview()
        {
            var info = _selectionState.GetSelectionPreviewInfo();
            if (info == null || _viewModel == null)
            {
                selectionPreview.IsVisible = false;
                if (_viewModel != null)
                {
                    mainImage.Source = _viewModel.PremultipliedBitmap;
                }
                return;
            }

            var picture = _viewModel.PictureBuffer;
            if (info.OriginalArea.HasValue)
            {
                picture = picture.Clear(info.OriginalArea.Value);
            }

            var blender = ImageBlender;
            var bgColor = BackgroundColor;
            var pixels = info.Pixels;
            if (blender is Eede.Domain.ImageEditing.Blending.AlphaImageBlender)
            {
                pixels = pixels.ApplyTransparency(bgColor);
            }
            picture = picture.Blend(blender, pixels, info.Position);

            mainImage.Source = AvaloniaBitmapAdapter.StaticConvertToPremultipliedBitmap(picture);
            selectionPreview.IsVisible = false;
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

        public static readonly StyledProperty<Eede.Domain.ImageEditing.Blending.IImageBlender> ImageBlenderProperty =
            AvaloniaProperty.Register<PictureContainer, Eede.Domain.ImageEditing.Blending.IImageBlender>(nameof(ImageBlender), new Eede.Domain.ImageEditing.Blending.DirectImageBlender());
        public Eede.Domain.ImageEditing.Blending.IImageBlender ImageBlender
        {
            get => GetValue(ImageBlenderProperty);
            set => SetValue(ImageBlenderProperty, value);
        }

        public static readonly StyledProperty<Eede.Domain.Palettes.ArgbColor> BackgroundColorProperty =
            AvaloniaProperty.Register<PictureContainer, Eede.Domain.Palettes.ArgbColor>(nameof(BackgroundColor), new Eede.Domain.Palettes.ArgbColor(0, 0, 0, 0));
        public Eede.Domain.Palettes.ArgbColor BackgroundColor
        {
            get => GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        private PictureSize CanvasSize = new(32, 32);

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_viewModel == null)
            {
                return;
            }

            var nowPosition = PointToPosition(e.GetPosition(canvas));

            // アニメーションモード時はモードに応じたステートを再作成（グリッド設定の変更を反映するため）
            if (_viewModel.AnimationViewModel.IsAnimationMode)
            {
                _selectionState = CreateInitialState();
            }

            // ドックエリアでは _cursorSize を使用して判定する
            var currentCursorArea = HalfBoxArea.Create(nowPosition, _cursorSize);
            if (_viewModel.AnimationViewModel.IsAnimationMode)
            {
                currentCursorArea = HalfBoxArea.Create(nowPosition, new PictureSize(_viewModel.AnimationViewModel.GridWidth, _viewModel.AnimationViewModel.GridHeight));
            }

            switch (e.GetCurrentPoint(canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    if (_viewModel.AnimationViewModel.IsAnimationMode)
                    {
                        _selectionState = _selectionState.HandlePointerLeftButtonPressed(currentCursorArea, nowPosition, null, () => _viewModel.PictureBuffer, null, 4);
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
            _localCursorArea = currentCursorArea;
            UpdateCursor();
        }

        private void OnPointerRightButtonPressed(Position nowPosition)
        {
            if (_viewModel == null) return;

            // ドックエリアでは _cursorSize を使用してカーソル領域を作成
            var currentCursorArea = HalfBoxArea.Create(nowPosition, _cursorSize);

            // 範囲選択を開始するために、現在のステートに右クリックを通知
            var (newState, newArea) = _selectionState.HandlePointerRightButtonPressed(currentCursorArea, nowPosition, MinCursorSize, PictureUpdateAction);

            _selectionState = newState;
            _localCursorArea = newArea;
            UpdateCursor();
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_viewModel == null) return;

            Position nowPosition = PointToPosition(e.GetPosition(canvas));

            // ドックエリア基準のカーソル領域を作成（_cursorSizeを使用）
            var cursorArea = HalfBoxArea.Create(nowPosition, _cursorSize);

            if (_viewModel.AnimationViewModel.IsAnimationMode && _selectionState is not RegionSelectingState)
            {
                // アニメーションモード中で、かつ範囲選択中でない場合のみグリッドサイズを適用
                cursorArea = HalfBoxArea.Create(nowPosition, new PictureSize(_viewModel.AnimationViewModel.GridWidth, _viewModel.AnimationViewModel.GridHeight));
            }

            bool isShift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            var (newVisible, newArea) = _selectionState.HandlePointerMoved(cursorArea, VisibleCursor, nowPosition, isShift, CanvasSize);
            VisibleCursor = newVisible;
            _localCursorArea = newArea;
            UpdateCursor();
            UpdateCursorCursor(nowPosition);
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_viewModel == null) return;

            var nowPosition = PointToPosition(e.GetPosition(canvas));
            switch (e.GetCurrentPoint(canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonReleased:
                    _selectionState = _selectionState.HandlePointerLeftButtonReleased(_localCursorArea, nowPosition, PicturePushAction, PictureUpdateAction);
                    break;

                case PointerUpdateKind.RightButtonReleased:
                    OnPointerRightButtonReleased(_viewModel);
                    break;
            }
        }

        private void OnPointerRightButtonReleased(DockPictureViewModel vm)
        {
            // 範囲選択状態からの遷移を確認
            var selectingState = _selectionState as RegionSelectingState;
            var (newState, newArea) = _selectionState.HandlePointerRightButtonReleased(_localCursorArea, PicturePushAction);

            // 範囲選択が終わった直後なら、作業エリアへ転送を実行する
            if (selectingState != null)
            {
                var finalArea = selectingState.GetSelectingArea();
                if (finalArea.HasValue)
                {
                    // ドラッグ範囲が確定している場合、その正確な範囲を転送
                    PicturePushAction?.Execute(finalArea.Value);
                    // 作業エリア側の選択状態も同期させる
                    vm.GlobalState.CursorArea = HalfBoxArea.Create(finalArea.Value.Position, finalArea.Value.Size);
                    
                    // 次回のカーソルサイズのためにサイズを保存
                    _cursorSize = finalArea.Value.Size;
                }
                else
                {
                    // 単なる右クリックの場合、スナップされた位置から固定サイズで転送
                    PicturePushAction?.Execute(new PictureArea(newArea.RealPosition, MinCursorSize));
                    vm.GlobalState.CursorArea = newArea;
                    // 必要ならここでも _cursorSize を MinCursorSize にリセットする？
                    // いや、前回の選択サイズを維持するなら変更しないほうがいいかもしれないが、
                    // クリックだけなら MinCursorSize を使うのが自然か。
                    // とりあえず今回は「選択完了後」の話なので、クリック時の挙動は変えない。
                }
                // 状態をリセットする（マウス追従と次の選択開始のため）
                _selectionState = CreateInitialState();
            }
            else
            {
                _selectionState = newState;
            }

            // リセット後の状態に合わせて _localCursorArea を再計算
            // HandlePointerRightButtonReleased が返す newArea は古いサイズ（ドラッグ開始時の32x32など）の可能性があるため
            // 新しい _cursorSize で作り直す
            _localCursorArea = HalfBoxArea.Create(_localCursorArea.RealPosition, _cursorSize);

            UpdateCursor();
        }

        private void OnPointerExited(object? sender, PointerEventArgs e)
        {
            VisibleCursor = false;
            UpdateCursor();
        }

        private Position PointToPosition(Point point)
        {
            if (_viewModel == null) return new((int)point.X, (int)point.Y);
            return new(_viewModel.Magnification.Minify((int)point.X), _viewModel.Magnification.Minify((int)point.Y));
        }
    }
}
