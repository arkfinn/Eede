using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eede.Application.Pictures;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace Eede.ViewModels.DataDisplay
{
    public class DockPictureViewModel : ViewModelBase
    {
        public static DockPictureViewModel FromUri(Uri path)
        {
            var fullPath = HttpUtility.UrlDecode(path.AbsolutePath);
            return new DockPictureViewModel()
            {
                Subject = fullPath,
                Bitmap = new Bitmap(fullPath),
                Path = path
            };
        }

        public DockPictureViewModel()
        {
            OnPicturePush = ReactiveCommand.Create<PictureArea>(ExecutePicturePush);
            OnPicturePull = ReactiveCommand.Create<Position>(ExecutePicturePull);
            Bitmap ??= new WriteableBitmap(new PixelSize(32, 32), new Vector(96, 96), PixelFormat.Bgra8888);
        }

        public void Save()
        {
            if (Path == null)
            {

            }
            else
            {
                var fullPath = HttpUtility.UrlDecode(Path.AbsolutePath);
                Bitmap.Save(fullPath);
            }
        }

        public Uri? Path { get; private set; }

        public string Subject { get; private set; } = "新しいファイル";

        [Reactive] public Bitmap Bitmap { get; set; }

        public ReactiveCommand<PictureArea, Unit> OnPicturePush { get; }
        public event EventHandler<PicturePushEventArgs>? PicturePush;
        private void ExecutePicturePush(PictureArea area)
        {
            PicturePush?.Invoke(this, new PicturePushEventArgs(BringPictureBuffer(), area));
        }

        public ReactiveCommand<Position, Unit> OnPicturePull { get; }
        public event EventHandler<PicturePullEventArgs>? PicturePull;
        private void ExecutePicturePull(Position position)
        {
            PicturePull?.Invoke(this, new PicturePullEventArgs(BringPictureBuffer(), position));
        }

        private Picture BringPictureBuffer()
        {
            return Picture.CreateEmpty(new PictureSize(64, 64));
        }
    }
}
