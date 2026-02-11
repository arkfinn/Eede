using Eede.Domain.SharedKernel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;

namespace Eede.Presentation.ViewModels.Pages
{
    #nullable enable

    public class NewPictureWindowViewModel : ViewModelBase
    {
        [Reactive] public PictureSize Size { get; set; }
        public int Width
        {
            get => Size.Width;
            set => Size = new(value, Size.Height);
        }
        public int Height
        {
            get => Size.Height;
            set => Size = new(Size.Width, value);
        }

        public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

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
