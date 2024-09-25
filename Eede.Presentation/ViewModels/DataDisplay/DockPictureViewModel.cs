using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eede.Application.Pictures;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Common.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Web;
using Eede.Presentation.Files;
using Eede.Domain.Files;
using Eede.Presentation.Events;
using Eede.Presentation.Common.Services;

namespace Eede.ViewModels.DataDisplay
{
    public class DockPictureViewModel : ViewModelBase
    {

        public static DockPictureViewModel FromFile(BitmapFile file)
        {
            var vm = new DockPictureViewModel();
            vm.Initialize(file);
            return vm;
        }

        public static DockPictureViewModel FromSize(PictureSize size)
        {
            var vm = new DockPictureViewModel();
            vm.Initialize(new BitmapFile(
                new WriteableBitmap(new PixelSize(size.Width, size.Height), new Vector(96, 96), PixelFormat.Bgra8888),
                FilePath.Empty()));
            return vm;
        }

        [Reactive] public Bitmap Bitmap { get; set; }
        [Reactive] public PictureSize MinCursorSize { get; set; }
        [Reactive] public PictureSize CursorSize { get; set; }
        [Reactive] public HalfBoxArea CursorArea { get; set; }
        [Reactive] public bool Enabled { get; set; }
        [Reactive] public bool Closable { get; set; }

        public DockPictureViewModel()
        {
            OnPicturePush = ReactiveCommand.Create<PictureArea>(ExecutePicturePush);
            OnPicturePull = ReactiveCommand.Create<Position>(ExecutePicturePull);
            OnClosing = ReactiveCommand.Create(ExecuteClosing);
            MinCursorSize = new PictureSize(32, 32);
            CursorSize = new PictureSize(32, 32);
            this.WhenAnyValue(x => x.CursorSize).Subscribe(size =>
            {
                CursorArea = HalfBoxArea.Create(size, new Position(0, 0));
            });
            Enabled = true;
            Closable = true;
            this.WhenAnyValue(x => x.Edited).Subscribe(_ =>
            {
                Closable = !Edited;
            });
            Initialize(new BitmapFile(
                new WriteableBitmap(new PixelSize(32, 32), new Vector(96, 96), PixelFormat.Bgra8888),
                FilePath.Empty()));
        }

        public event EventHandler<PictureSaveEventArgs> PictureSave;

        public void Save(StorageService storage)
        {
            PictureSave?.Invoke(this, new PictureSaveEventArgs(new BitmapFile(Bitmap, Path), storage));
        }

        public void Initialize(BitmapFile file)
        {
            Bitmap = file.Bitmap;
            var isEmpty = file.Path.IsEmpty();
            Path = isEmpty ? FilePath.Empty() : file.Path;
            Subject = isEmpty ? "新しいファイル" : file.Path.Path;
            Edited = false;
        }

        [Reactive] public bool Edited { get; set; }

        private FilePath Path = FilePath.Empty();

        [Reactive] public string Subject { get; private set; }
        [Reactive] public SaveAlertResult SaveAlertResult { get; private set; }

        public ReactiveCommand<Unit, Unit> OnClosing { get; }

        public void ExecuteClosing()
        {
            if (!Edited) return;
            switch (SaveAlertResult)
            {
                case SaveAlertResult.Cancel:
                    Closable = false;
                    break;
                case SaveAlertResult.Save:
                    //Save();
                    Closable = true;
                    break;
                default:
                    Closable = true;
                    break;
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
                Subject = "●" + Subject;
            }
        }

        private Picture BringPictureBuffer()
        {
            return Picture.CreateEmpty(new PictureSize(64, 64));
        }
    }
}
