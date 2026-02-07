using Avalonia.Media.Imaging;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Enums;
using Eede.Presentation.Events;
using Eede.Presentation.Files;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Eede.Presentation.ViewModels.DataDisplay
{
    public class DockPictureViewModel : ViewModelBase
    {

        public static DockPictureViewModel FromFile(Picture picture, FilePath filePath, GlobalState globalState, AnimationViewModel animationViewModel, IBitmapAdapter<Bitmap> bitmapAdapter, IPictureIOService pictureIOService)
        {
            DockPictureViewModel vm = new(globalState, animationViewModel, bitmapAdapter, pictureIOService);
            vm.Initialize(picture, filePath);
            return vm;
        }

        public static DockPictureViewModel FromSize(PictureSize size, GlobalState globalState, AnimationViewModel animationViewModel, IBitmapAdapter<Bitmap> bitmapAdapter, IPictureIOService pictureIOService)
        {
            DockPictureViewModel vm = new(globalState, animationViewModel, bitmapAdapter, pictureIOService);
            vm.Initialize(Picture.CreateEmpty(size), FilePath.Empty());
            return vm;
        }

        [Reactive] public Picture PictureBuffer { get; set; }
        private Bitmap _premultipliedBitmap;
        public Bitmap PremultipliedBitmap
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
        [Reactive] public string Id { get; private set; }
        [Reactive] public PictureSize MinCursorSize { get; set; }
        [Reactive] public PictureSize CursorSize { get; set; }
        [Reactive] public Avalonia.Input.Cursor ActiveCursor { get; set; }
        [Reactive] public Avalonia.Input.Cursor? AnimationCursor { get; set; }
        [Reactive] public bool Enabled { get; set; }
        [Reactive] public bool Closable { get; set; }
        [Reactive] public string Subject { get; private set; }
        [Reactive] public string Title { get; private set; }
        [Reactive] public bool Edited { get; set; }
        [Reactive] public FilePath FilePath { get; private set; }
        [Reactive] public SaveAlertResult SaveAlertResult { get; private set; }
        [Reactive] public Magnification Magnification { get; set; }
        [Reactive] public double DisplayWidth { get; private set; }
        [Reactive] public double DisplayHeight { get; private set; }
        public ReactiveCommand<Unit, Unit> ZoomInCommand { get; }
        public ReactiveCommand<Unit, Unit> ZoomOutCommand { get; }
        public ReactiveCommand<float, Unit> SetMagnificationCommand { get; }
        public ReactiveCommand<Unit, bool> OnClosing { get; }
        public ReactiveCommand<Unit, bool> CloseCommand { get; }
        public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);
        public event AsyncEventHandler<PictureSaveEventArgs> PictureSave;
        public event AsyncEventHandler<EventArgs> RequestClose;
        private static readonly float[] MagnificationSteps = [1f, 2f, 4f, 6f, 8f, 12f];

        public void ZoomIn()
        {
            float current = Magnification.Value;
            foreach (float step in MagnificationSteps)
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
            for (int i = MagnificationSteps.Length - 1; i >= 0; i--)
            {
                if (MagnificationSteps[i] < current)
                {
                    Magnification = new Magnification(MagnificationSteps[i]);
                    return;
                }
            }
        }

        public GlobalState GlobalState { get; }
        public AnimationViewModel AnimationViewModel { get; }
        private readonly IBitmapAdapter<Bitmap> BitmapAdapter;
        private readonly IPictureIOService PictureIOService;

        public DockPictureViewModel(GlobalState globalState, AnimationViewModel animationViewModel, IBitmapAdapter<Bitmap> bitmapAdapter, IPictureIOService pictureIOService)
        {
            GlobalState = globalState;
            AnimationViewModel = animationViewModel;
            BitmapAdapter = bitmapAdapter;
            PictureIOService = pictureIOService;

            Id = Guid.NewGuid().ToString();

            OnPicturePush = ReactiveCommand.Create<PictureArea>(ExecutePicturePush);
            OnPicturePull = ReactiveCommand.Create<Position>(ExecutePicturePull);
            OnPictureUpdate = ReactiveCommand.Create<Picture>(ExecutePictureUpdate);
            OnClosing = ReactiveCommand.CreateFromTask(ExecuteClosing);
            CloseCommand = ReactiveCommand.CreateFromTask(ExecuteClose);
            ZoomInCommand = ReactiveCommand.Create(ZoomIn);
            ZoomOutCommand = ReactiveCommand.Create(ZoomOut);
            SetMagnificationCommand = ReactiveCommand.Create<float>(v => Magnification = new Magnification(v));

            MinCursorSize = new PictureSize(32, 32);
            CursorSize = new PictureSize(32, 32);
            Magnification = new Magnification(1);
            ActiveCursor = Avalonia.Input.Cursor.Default;
            Enabled = true;
            Closable = true;

            SetupObservations();
            Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), FilePath.Empty());
        }

        private void SetupObservations()
        {
            SetupAnimationCursorObservation();
            SetupEditStateObservation();
            SetupTitleObservation();
            SetupBitmapObservation();
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
            _ = this.WhenAnyValue(x => x.Subject, x => x.Edited).Subscribe(_ =>
            {
                Title = Edited ? "●" + Subject : Subject;
            });
        }

        private void SetupBitmapObservation()
        {
            _ = this.WhenAnyValue(x => x.PictureBuffer)
                .Where(x => x != null)
                .Subscribe(_ =>
             {
                 PremultipliedBitmap = BitmapAdapter.ConvertToPremultipliedBitmap(PictureBuffer);
             });
        }

        public async Task Save()
        {
            if (PictureSave == null) return;
            PictureSaveEventArgs args = CreateSaveEventArgs();
            await PictureSave.Invoke(this, args);
            HandleSaveResult(args);
        }

        private PictureSaveEventArgs CreateSaveEventArgs()
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
            return new PictureSaveEventArgs(file);
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

            Edited = false;
        }

        public async Task<bool> ExecuteClose()
        {
            if (!Edited)
            {
                return true;
            }
            await RequestClose?.Invoke(this, EventArgs.Empty);
            return Closable;
        }


        public async Task<bool> ExecuteClosing()
        {
            if (!Edited) return true;
            await AutoSaveIfNeeded();
            UpdateClosableState();
            return Closable;
        }

        private async Task AutoSaveIfNeeded()
        {
            if (SaveAlertResult == SaveAlertResult.Save)
            {
                await Save();
            }
        }

        private void UpdateClosableState()
        {
            Closable = SaveAlertResult switch
            {
                SaveAlertResult.Cancel => false,
                _ => true,
            };
        }


        public ReactiveCommand<Picture, Unit> OnPictureUpdate { get; }
        public event EventHandler<PictureUpdateEventArgs> PictureUpdate;
        private void ExecutePictureUpdate(Picture picture)
        {
            PictureUpdate?.Invoke(this, new PictureUpdateEventArgs(picture));
            if (!Edited)
            {
                Edited = true;
            }
        }

        public ReactiveCommand<PictureArea, Unit> OnPicturePush { get; }
        public event EventHandler<PicturePushEventArgs> PicturePush;
        private void ExecutePicturePush(PictureArea area)
        {
            PicturePush?.Invoke(this, new PicturePushEventArgs(BringPictureBuffer(), area));
        }

        public ReactiveCommand<Position, Unit> OnPicturePull { get; }
        public event EventHandler<PicturePullEventArgs> PicturePull;
        private void ExecutePicturePull(Position position)
        {
            PicturePull?.Invoke(this, new PicturePullEventArgs(BringPictureBuffer(), position));
            if (!Edited)
            {
                Edited = true;
            }
        }

        private Picture BringPictureBuffer()
        {
            return PictureBuffer;
        }
    }
}