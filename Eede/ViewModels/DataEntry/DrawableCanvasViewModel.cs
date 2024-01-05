using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using System;

namespace Eede.ViewModels.DataEntry
{
    public class DrawableCanvasViewModel: ViewModelBase
    {
        private Bitmap _bitmap = null;
        public Bitmap MyBitmap
        {
            get => _bitmap;
            set
            {
                if (_bitmap != value) { 
                    _bitmap?.Dispose(); 
                }
                this.RaiseAndSetIfChanged(ref _bitmap, value);
            }
        }
    }
}
