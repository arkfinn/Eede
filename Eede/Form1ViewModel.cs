using Eede.Actions;
using Eede.Application.Drawings;
using Eede.Common.Drawings;
using Eede.Common.Pictures.Actions;
using Eede.Domain.Colors;
using Eede.Domain.DrawStyles;
using Eede.Domain.Scales;
using Reactive.Bindings;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Drawing;

namespace Eede
{
    internal class Form1ViewModel : ReactiveObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ReactiveProperty<Magnification> Magnification { get; } = new ReactiveProperty<Magnification>();
        public ReactiveProperty<ArgbColor> PenColor { get; } = new ReactiveProperty<ArgbColor>();

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

        public ReactiveCommand<PictureActions> PictureActionCommand { get; } = new();

        //public ReactiveCommand<DrawEventArgs> AddUndoDrawCommand { get; } = new();

        public Form1ViewModel()
        {
            Magnification.Value = new Magnification(4);
            PenColor.Value = new ArgbColor(255, 0, 0, 0);

            // senderの渡し方を検討する（送信元でActionに詰めるなど）
            //AddUndoDrawCommand.Subscribe(new Action<DrawEventArgs>(x =>
            //{
            //    //DrawAction action = new(paintableBox1, e.PreviousPicture, e.NowPicture);
            //    //AddUndoItem(action);
            //}));
            PictureActionCommand.Subscribe(new Action<PictureActions>(action =>
            {
                switch (action)
                {
                    case PictureActions.ShiftUp:
                        Console.WriteLine("shift_up");
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
            }));
        }
    }
}
