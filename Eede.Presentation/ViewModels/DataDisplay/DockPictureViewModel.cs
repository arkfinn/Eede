using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eede.Application.Pictures;
using Eede.Domain.Files;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Enums;
using Eede.Presentation.Common.Services;
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

        public DockPictureViewModel()
        {
            OnPicturePush = ReactiveCommand.Create<PictureArea>(ExecutePicturePush);
            OnPicturePull = ReactiveCommand.Create<Position>(ExecutePicturePull);
            OnClosing = ReactiveCommand.Create(ExecuteClosing);
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

        public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);
        public event AsyncEventHandler<PictureSaveEventArgs> PictureSave;

        public async void Save(StorageService storage)
        {
            if (PictureSave == null)
            {
                return;
            }
            var bitmap = PictureBitmapAdapter.ConvertToBitmap(PictureBuffer);
            var args = new PictureSaveEventArgs(ImageFile.WithBitmap(bitmap), storage);
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




        [Reactive] public SaveAlertResult SaveAlertResult { get; private set; }

        public ReactiveCommand<Unit, Unit> OnClosing { get; }

        public void ExecuteClosing()
        {
            if (!Edited)
            {
                return;
            }
            if (SaveAlertResult == SaveAlertResult.Save)
            {
                // Save();
            }

            Closable = SaveAlertResult switch
            {
                SaveAlertResult.Cancel => false,
                SaveAlertResult.Save => true,
                _ => true,
            };
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
            return Picture.CreateEmpty(new PictureSize(64, 64));
        }
    }
}


