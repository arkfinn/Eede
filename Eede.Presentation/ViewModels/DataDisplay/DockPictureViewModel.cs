using Avalonia.Media.Imaging;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Enums;
using Eede.Presentation.Events;
using Eede.Presentation.Files;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using RxVoid = ReactiveUI.Primitives.RxVoid;

namespace Eede.Presentation.ViewModels.DataDisplay
{
#nullable enable

    public partial class DockPictureViewModel : ViewModelBase, IDisambiguatableItem
    {
        string IDisambiguatableItem.FilePath => FilePath != null ? FilePath.ToString() : "";
        private static int _untitledCounter = 0;
        public static void ResetUntitledCounter() => _untitledCounter = 0;

        [Reactive] public partial string BaseName { get; set; }
        [Reactive] public partial string DisambiguationSuffix { get; set; }
        [Reactive] public partial string ToolTipText { get; set; }

        public static DockPictureViewModel FromFile(Picture picture, FilePath filePath, GlobalState globalState, AnimationViewModel animationViewModel, IBitmapAdapter<Bitmap> bitmapAdapter, IPictureFileIO pictureFileIO)
        {
            DockPictureViewModel vm = new(globalState, animationViewModel, bitmapAdapter, pictureFileIO);
            vm.Initialize(picture, filePath);
            return vm;
        }

        public static DockPictureViewModel FromSize(PictureSize size, GlobalState globalState, AnimationViewModel animationViewModel, IBitmapAdapter<Bitmap> bitmapAdapter, IPictureFileIO pictureFileIO)
        {
            DockPictureViewModel vm = new(globalState, animationViewModel, bitmapAdapter, pictureFileIO);
            vm.Initialize(Picture.CreateEmpty(size), FilePath.Empty());
            return vm;
        }

        private Picture _pictureBuffer = null!;
        public Picture PictureBuffer
        {
            get => _pictureBuffer;
            set
            {
                this.RaiseAndSetIfChanged(ref _pictureBuffer, value);
                if (value != null && BitmapAdapter != null)
                {
                    PremultipliedBitmap = BitmapAdapter.ConvertToPremultipliedBitmap(value);
                }
            }
        }
        private Bitmap? _premultipliedBitmap;
        public Bitmap? PremultipliedBitmap
        {
            get => _premultipliedBitmap;
            set
            {
                if (_premultipliedBitmap != value)
                {
                    _premultipliedBitmap?.Dispose();
                }
                _ = this.RaiseAndSetIfChanged(ref _premultipliedBitmap, value);
            }
        }
        [Reactive] public partial string Id { get; set; }
        [Reactive] public partial PictureSize MinCursorSize { get; set; }
        [Reactive] public partial PictureSize CursorSize { get; set; }
        [Reactive] public partial Avalonia.Input.Cursor ActiveCursor { get; set; }
        [Reactive] public partial Avalonia.Input.Cursor? AnimationCursor { get; set; }
        [Reactive] public partial bool Enabled { get; set; }
        [Reactive] public partial bool Closable { get; set; }
        [Reactive] public partial string Subject { get; set; }
        [Reactive] public partial string Title { get; set; }
        [Reactive] public partial bool Edited { get; set; }
        [Reactive] public partial FilePath FilePath { get; set; }
        [Reactive] public partial SaveAlertResult SaveAlertResult { get; set; }
        [Reactive] public partial Magnification Magnification { get; set; }
        [Reactive] public partial double DisplayWidth { get; set; }
        [Reactive] public partial double DisplayHeight { get; set; }
        [Reactive] public partial BackgroundColor BackgroundColor { get; set; }
        [Reactive] public partial TouchDockMode TouchDockMode { get; set; }
        public ReactiveCommand<RxVoid, RxVoid> ZoomInCommand { get; }
        public ReactiveCommand<RxVoid, RxVoid> ZoomOutCommand { get; }
        public ReactiveCommand<float, RxVoid> SetMagnificationCommand { get; }
        public ReactiveCommand<RxVoid, bool> OnClosing { get; }
        public ReactiveCommand<RxVoid, bool> CloseCommand { get; }
        public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);
        public event AsyncEventHandler<PictureSaveEventArgs>? PictureSave;
        public event AsyncEventHandler<EventArgs>? RequestClose;

        public void ZoomIn()
        {
            float current = Magnification.Value;
            foreach (float step in ZoomHelper.MagnificationSteps)
            {
                if (step > current)
                {
                    Magnification = new Magnification(step);
                    return;
                }
            }
        }

        public void ZoomOut()
        {
            float current = Magnification.Value;
            for (int i = ZoomHelper.MagnificationSteps.Length - 1; i >= 0; i--)
            {
                if (ZoomHelper.MagnificationSteps[i] < current)
                {
                    Magnification = new Magnification(ZoomHelper.MagnificationSteps[i]);
                    return;
                }
            }
        }

        public GlobalState GlobalState { get; }
        public AnimationViewModel AnimationViewModel { get; }
        private readonly IBitmapAdapter<Bitmap> BitmapAdapter;
        private readonly IPictureFileIO _pictureFileIO;

        public DockPictureViewModel(GlobalState globalState, AnimationViewModel animationViewModel, IBitmapAdapter<Bitmap> bitmapAdapter, IPictureFileIO pictureFileIO)
        {
            GlobalState = globalState;
            AnimationViewModel = animationViewModel;
            BitmapAdapter = bitmapAdapter;
            _pictureFileIO = pictureFileIO;

            Id = Guid.NewGuid().ToString();

            OnPicturePush = ReactiveCommand.Create<Position>(ExecutePicturePush);
            OnPicturePull = ReactiveCommand.Create<PictureArea>(ExecutePicturePull);
            OnPictureUpdate = ReactiveCommand.Create<Picture>(ExecutePictureUpdate);
            OnClosing = ReactiveCommand.CreateFromTask(ExecuteClosing);
            CloseCommand = ReactiveCommand.CreateFromTask(ExecuteClose);
            ZoomInCommand = ReactiveCommand.Create(ZoomIn);
            ZoomOutCommand = ReactiveCommand.Create(ZoomOut);
            SetMagnificationCommand = ReactiveCommand.Create<float>(v => Magnification = new Magnification(v));

            MinCursorSize = new PictureSize(32, 32);
            CursorSize = new PictureSize(32, 32);
            Magnification = new Magnification(1);
            BackgroundColor = BackgroundColor.Default;
            ActiveCursor = Avalonia.Input.Cursor.Default;
            Enabled = true;
            Closable = true;

            PictureBuffer = null!;
            Subject = null!;
            Title = null!;
            FilePath = null!;
            BaseName = "";
            DisambiguationSuffix = "";
            ToolTipText = "";

            SetupObservations();
            Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), FilePath.Empty());
        }

        private void SetupObservations()
        {
            SetupAnimationCursorObservation();
            SetupEditStateObservation();
            SetupTitleObservation();
            SetupDisplaySizeObservation();
        }

        private void SetupDisplaySizeObservation()
        {
            this.WhenAnyValue(x => x.Magnification, x => x.PictureBuffer)
                .Subscribe(x =>
                {
                    if (x.Item2 != null)
                    {
                        DisplayWidth = x.Item1.Magnify(x.Item2.Size.Width);
                        DisplayHeight = x.Item1.Magnify(x.Item2.Size.Height);
                    }
                });
        }

        private void SetupAnimationCursorObservation()
        {
            this.WhenAnyValue(x => x.AnimationViewModel.IsAnimationMode, x => x.AnimationCursor)
                .Subscribe(x =>
                {
                    ActiveCursor = x.Item1 ? (x.Item2 ?? Avalonia.Input.Cursor.Default) : Avalonia.Input.Cursor.Default;
                });
        }

        private void SetupEditStateObservation()
        {
            _ = this.WhenAnyValue(x => x.Edited).Subscribe(_ =>
            {
                Closable = !Edited;
            });
        }

        private void SetupTitleObservation()
        {
            _ = this.WhenAnyValue(x => x.BaseName, x => x.DisambiguationSuffix, x => x.Edited).Subscribe(_ =>
            {
                string shortName = TitleTrimmer.TruncateMiddle(BaseName, 18);
                string dirtyMark = Edited ? " *" : "";
                Title = $"{shortName}{DisambiguationSuffix}{dirtyMark}";
            });

            _ = this.WhenAnyValue(x => x.BaseName, x => x.FilePath, x => x.PictureBuffer, x => x.Edited).Subscribe(_ =>
            {
                UpdateToolTipText();
            });
        }

        private void UpdateToolTipText()
        {
            string pathStr = FilePath != null ? FilePath.ToString() : "";
            string statusStr = Edited ? "未保存の変更あり (*)" : "保存済み";
            int w = PictureBuffer != null ? PictureBuffer.Width : 0;
            int h = PictureBuffer != null ? PictureBuffer.Height : 0;

            if (string.IsNullOrEmpty(pathStr))
            {
                ToolTipText = $"{BaseName}\nサイズ: {w} × {h} px\n状態: {statusStr}";
            }
            else
            {
                string? dir = System.IO.Path.GetDirectoryName(pathStr);
                ToolTipText = $"{BaseName}\n{dir}\nサイズ: {w} × {h} px\n状態: {statusStr}";
            }
        }


        public async Task<bool> Save()
        {
            if (PictureSave == null) return false;
            PictureSaveEventArgs args = CreateSaveEventArgs(false);
            try
            {
                await PictureSave.Invoke(this, args);
                HandleSaveResult(args);
                return !args.IsCanceled;
            }
            finally
            {
                args.File?.Bitmap?.Dispose();
            }
        }

        public async Task SaveAs()
        {
            if (PictureSave == null) return;
            PictureSaveEventArgs args = CreateSaveEventArgs(true);
            try
            {
                await PictureSave.Invoke(this, args);
                HandleSaveResult(args);
            }
            finally
            {
                args.File?.Bitmap?.Dispose();
            }
        }

        private PictureSaveEventArgs CreateSaveEventArgs(bool isSaveAs)
        {
            Bitmap bitmap = BitmapAdapter.ConvertToBitmap(PictureBuffer);
            IImageFile file = FilePath.IsEmpty() ? new NewFile(bitmap) :
                             FilePath.GetExtension() switch
                             {
                                 ".png" => new PngFile(bitmap, FilePath),
                                 ".bmp" => new BitmapFile(bitmap, FilePath),
                                 ".arv" => new ArvFile(bitmap, FilePath),
                                 _ => new BitmapFile(bitmap, FilePath) // フォールバック
                             };
            return new PictureSaveEventArgs(file, isSaveAs);
        }

        private void HandleSaveResult(PictureSaveEventArgs args)
        {
            if (args.IsCanceled) return;
            Initialize(BitmapAdapter.ConvertToPicture(args.File.Bitmap), args.File.Path);
        }

        public void Initialize(Picture picture, FilePath path)
        {
            FilePath = path;
            PictureBuffer = picture;
            Subject = path.ToString();

            if (path.IsEmpty())
            {
                if (string.IsNullOrEmpty(BaseName))
                {
                    BaseName = $"無題-{System.Threading.Interlocked.Increment(ref _untitledCounter)}";
                }
            }
            else
            {
                BaseName = System.IO.Path.GetFileName(path.ToString());
            }

            Edited = false;
            UpdateToolTipText();
        }

        public async Task<bool> ExecuteClose()
        {
            if (!Edited)
            {
                return true;
            }
            if (RequestClose != null)
            {
                await RequestClose.Invoke(this, EventArgs.Empty);
            }
            return Closable;
        }


        public async Task<bool> ExecuteClosing()
        {
            if (!Edited) return true;
            bool autoSaveSucceeded = await AutoSaveIfNeeded();
            UpdateClosableState(autoSaveSucceeded);
            return Closable;
        }

        private async Task<bool> AutoSaveIfNeeded()
        {
            if (SaveAlertResult == SaveAlertResult.Save)
            {
                return await Save();
            }
            return true;
        }

        private void UpdateClosableState(bool autoSaveSucceeded)
        {
            Closable = SaveAlertResult switch
            {
                SaveAlertResult.Save => autoSaveSucceeded,
                SaveAlertResult.NoSave => true,
                SaveAlertResult.Cancel => false,
                _ => false,
            };
        }


        public ReactiveCommand<Picture, RxVoid> OnPictureUpdate { get; }
        public event EventHandler<PictureUpdateEventArgs>? PictureUpdate;
        private void ExecutePictureUpdate(Picture picture)
        {
            PictureBuffer = picture;
            PictureUpdate?.Invoke(this, new PictureUpdateEventArgs(picture));
            if (!Edited)
            {
                Edited = true;
            }
        }

        public ReactiveCommand<Position, RxVoid> OnPicturePush { get; }
        public event EventHandler<PicturePushEventArgs>? PicturePush;
        private void ExecutePicturePush(Position position)
        {
            PicturePush?.Invoke(this, new PicturePushEventArgs(BringPictureBuffer(), position));
            if (!Edited)
            {
                Edited = true;
            }
        }

        public ReactiveCommand<PictureArea, RxVoid> OnPicturePull { get; }
        public event EventHandler<PicturePullEventArgs>? PicturePull;
        private void ExecutePicturePull(PictureArea area)
        {
            PicturePull?.Invoke(this, new PicturePullEventArgs(BringPictureBuffer(), area));
        }

        private Picture BringPictureBuffer()
        {
            return PictureBuffer;
        }
    }
}

