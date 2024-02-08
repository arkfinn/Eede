using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eede.Application.Pictures;
using Eede.Common.Enums;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
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
            Bitmap ??= new WriteableBitmap(new PixelSize(32, 32), new Vector(96, 96), PixelFormat.Bgra8888);
            MinCursorSize = new PictureSize(32, 32);
            CursorSize = new PictureSize(32, 32);
            this.WhenAnyValue(x => x.CursorSize).Subscribe(size =>
            {
                CursorArea = HalfBoxArea.Create(size, new Position(0, 0));
            });
            Subject = "新しいファイル";
            Enabled = true;
            Closable = true;
            Edited = false;
            this.WhenAnyValue(x => x.Edited)
               .Subscribe(_ =>
               {
                   Closable = !Edited;
               });
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
                Subject = fullPath;
            }
        }

        [Reactive] public bool Edited { get; set; }

        public Uri? Path { get; private set; }

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
                    Save();
                    Closable = true;
                    break;
                default:
                    Closable = true;
                    break;
            }
        }

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
