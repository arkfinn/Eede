using Avalonia.Media.Imaging;
using Eede.Application.Pictures;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Enums;
using Eede.Presentation.Events;
using Eede.Presentation.Files;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace Eede.Presentation.ViewModels.DataDisplay
{
    public class DockPictureViewModel : ViewModelBase
    {

        public static DockPictureViewModel FromFile(IImageFile file)
        {
            DockPictureViewModel vm = new();
            vm.Initialize(file);
            return vm;
        }

        public static DockPictureViewModel FromSize(PictureSize size)
        {
            DockPictureViewModel vm = new();
            vm.Initialize(BitmapFileReader.CreateEmptyBitmapFile(size));
            return vm;
        }

        [Reactive] public Picture PictureBuffer { get; set; }
        [Reactive] public Bitmap PremultipliedBitmap { get; set; }
        [Reactive] public PictureSize MinCursorSize { get; set; }
        [Reactive] public PictureSize CursorSize { get; set; }
        [Reactive] public HalfBoxArea CursorArea { get; set; }
        [Reactive] public bool Enabled { get; set; }
        [Reactive] public bool Closable { get; set; }
        [Reactive] public string Subject { get; private set; }
        [Reactive] public string Title { get; private set; }
        [Reactive] public bool Edited { get; set; }
        [Reactive] public IImageFile ImageFile { get; private set; }
        [Reactive] public SaveAlertResult SaveAlertResult { get; private set; }
        public ReactiveCommand<Unit, bool> OnClosing { get; }
        public ReactiveCommand<Unit, bool> CloseCommand { get; }
        public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);
        public event AsyncEventHandler<PictureSaveEventArgs> PictureSave;
        public event AsyncEventHandler<EventArgs> RequestClose;

        public DockPictureViewModel()
        {
            OnPicturePush = ReactiveCommand.Create<PictureArea>(ExecutePicturePush);
            OnPicturePull = ReactiveCommand.Create<Position>(ExecutePicturePull);
            OnClosing = ReactiveCommand.CreateFromTask(ExecuteClosing);
            CloseCommand = ReactiveCommand.CreateFromTask(ExecuteClose);

            MinCursorSize = new PictureSize(32, 32);
            CursorSize = new PictureSize(32, 32);
            _ = this.WhenAnyValue(x => x.CursorSize).Subscribe(size =>
            {
                CursorArea = HalfBoxArea.Create(new Position(0, 0), size);
            });
            Enabled = true;
            Closable = true;
            _ = this.WhenAnyValue(x => x.Edited).Subscribe(_ =>
            {
                Closable = !Edited;
            });
            _ = this.WhenAnyValue(x => x.Subject, x => x.Edited).Subscribe(_ =>
            {
                Title = Edited ? "●" + Subject : Subject;
            });
            Initialize(BitmapFileReader.CreateEmptyBitmapFile(new PictureSize(32, 32)));
            _ = this.WhenAnyValue(x => x.PictureBuffer).Subscribe(_ =>
             {
                 PremultipliedBitmap = PictureBitmapAdapter.ConvertToPremultipliedBitmap(PictureBuffer);
             });
        }

        public async Task Save()
        {
            if (PictureSave == null)
            {
                return;
            }
            Bitmap bitmap = PictureBitmapAdapter.ConvertToBitmap(PictureBuffer);
            PictureSaveEventArgs args = new(ImageFile.WithBitmap(bitmap));
            await PictureSave.Invoke(this, args);
            if (!args.IsCanceled)
            {
                Initialize(args.File);
            }

        }

        public void Initialize(IImageFile file)
        {
            ImageFile = file;
            PictureBuffer = PictureBitmapAdapter.ConvertToPicture(file.Bitmap);
            Subject = file.Subject();

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
            if (!Edited)
            {
                return true;
            }
            if (SaveAlertResult == SaveAlertResult.Save)
            {
                await Save();
            }

            Closable = SaveAlertResult switch
            {
                SaveAlertResult.Cancel => false,
                SaveAlertResult.Save => true,
                _ => true,
            };

            return Closable;
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


