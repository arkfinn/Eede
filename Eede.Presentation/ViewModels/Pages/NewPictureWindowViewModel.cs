using Eede.Domain.SharedKernel;
using ReactiveUI;
using System;
using System.Reactive;
using RxVoid = ReactiveUI.Primitives.RxVoid;

namespace Eede.Presentation.ViewModels.Pages
{
    public class NewPictureWindowViewModel : ViewModelBase
    {
        public PictureSize Size
        {
            get => new(_width, _height);
            set
            {
                _width = value.Width;
                _height = value.Height;
                this.RaisePropertyChanged(nameof(Size));
                this.RaisePropertyChanged(nameof(Width));
                this.RaisePropertyChanged(nameof(Height));
            }
        }

        private int _width = 32;
        public int Width
        {
            get => Size.Width;
            set => Size = new(value, Size.Height);
        }

        private int _height = 32;
        public int Height
        {
            get => Size.Height;
            set => Size = new(Size.Width, value);
        }

        public ReactiveCommand<RxVoid, RxVoid> ConfirmCommand { get; }
        public ReactiveCommand<RxVoid, RxVoid> CancelCommand { get; }

        public bool Result { get; private set; }
        public Action? Close { get; set; }

        public NewPictureWindowViewModel()
        {
            Size = new PictureSize(256, 256);
            ConfirmCommand = ReactiveCommand.Create(OnConfirm);
            CancelCommand = ReactiveCommand.Create(OnCancel);
        }

        private void OnConfirm()
        {
            Result = true;
            Close?.Invoke();
        }

        private void OnCancel()
        {
            Result = false;
            Close?.Invoke();
        }
    }
}
