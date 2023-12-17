using Eede.Actions;
using Eede.Application.Drawings;
using Eede.Domain.Colors;
using Eede.Domain.Scales;
using Reactive.Bindings;
using System;
using System.ComponentModel;
using System.Drawing;

namespace Eede
{
    internal class Form1ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ReactiveProperty<Magnification> Magnification { get; } = new ReactiveProperty<Magnification>();
        public ReactiveProperty<ArgbColor> PenColor { get; } = new ReactiveProperty<ArgbColor>();
        public ReactiveProperty<int> PenSize { get; } = new ReactiveProperty<int>();

        //public ReactiveCommand<DrawEventArgs> AddUndoDrawCommand { get; } = new();

        public Form1ViewModel()
        {
            Magnification.Value = new Magnification(4);
            PenColor.Value = new ArgbColor(255, 0, 0, 0);
            PenSize.Value = 1;

            // senderの渡し方を検討する（送信元でActionに詰めるなど）
            //AddUndoDrawCommand.Subscribe(new Action<DrawEventArgs>(x =>
            //{
            //    //DrawAction action = new(paintableBox1, e.PreviousPicture, e.NowPicture);
            //    //AddUndoItem(action);
            //}));
        }
    }
}
