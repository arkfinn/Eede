using Eede.Common.Pictures.Actions;
using Eede.Domain.Colors;
using Eede.Domain.DrawStyles;
using Eede.Domain.Scales;
using Eede.Domain.Systems;
using Eede.Presentation.Common.Services;
using Eede.ViewModels.DataDisplay;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;

namespace Eede
{
    internal class Form1ViewModel : ReactiveObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [Reactive] public Magnification Magnification { get; set; }

        public ObservableCollection<DockPictureViewModel> Pictures { get; } = new ObservableCollection<DockPictureViewModel>();

        private ArgbColor _penColor = new ArgbColor(255, 0, 0, 0);
        public ArgbColor PenColor
        {
            get => _penColor;
            set => this.RaiseAndSetIfChanged(ref _penColor, value);
        }

        private int _penSize = 1;
        public int PenSize
        {
            get => _penSize;
            set => this.RaiseAndSetIfChanged(ref _penSize, value);
        }

        private IDrawStyle _drawStyle = new FreeCurve();
        public IDrawStyle DrawStyle
        {
            get => _drawStyle;
            set => this.RaiseAndSetIfChanged(ref _drawStyle, value);
        }

        public ReactiveCommand<PictureActions, Unit> PictureActionCommand { get; }

        //public ReactiveCommand<DrawEventArgs> AddUndoDrawCommand { get; } = new();

        public Form1ViewModel()
        {
            Magnification = new Magnification(4);

            // senderの渡し方を検討する（送信元でActionに詰めるなど）
            //AddUndoDrawCommand.Subscribe(new Action<DrawEventArgs>(x =>
            //{
            //    //DrawAction action = new(paintableBox1, e.PreviousPicture, e.NowPicture);
            //    //AddUndoItem(action);
            //}));
            PictureActionCommand = ReactiveCommand.Create<PictureActions>(ExecutePictureAction);
        }

        private void ExecutePictureAction(PictureActions action)
        {
            switch (action)
            {
                case PictureActions.ShiftUp:
                    break;
                case PictureActions.ShiftDown:
                    break;
                case PictureActions.ShiftLeft:
                    break;
                case PictureActions.ShiftRight:
                    break;
                default:
                    break;
            }
        }
    }
}
