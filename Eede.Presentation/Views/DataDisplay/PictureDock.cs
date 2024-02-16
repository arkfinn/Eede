using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Eede.Presentation.Views.DataDisplay;
using Eede.ViewModels.DataDisplay;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;


namespace Eede.Views.DataDisplay
{
    public class PictureDock : DocumentDock
    {
        public static readonly DirectProperty<PictureDock, IList?> PicturesProperty =
            AvaloniaProperty.RegisterDirect<PictureDock, IList?>(
                nameof(Pictures),
                o => o.Pictures,
                (o, v) => o.Pictures = v);

        public PictureDock()
        {
            ((IAvaloniaList<IDockable>)VisibleDockables!).CollectionChanged += VisibleDockables_CollectionChanged; ;
        }

        IList? pictures;

        public IList? Pictures
        {
            get => pictures;
            set => SetAndRaise(PicturesProperty, ref pictures, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == PicturesProperty)
            {
                if (change.OldValue is INotifyCollectionChanged oldValue)
                {
                    oldValue.CollectionChanged -= Pictures_CollectionChanged;
                }
                if (change.NewValue is INotifyCollectionChanged newValue)
                {
                    newValue.CollectionChanged += Pictures_CollectionChanged;
                }
            }
            base.OnPropertyChanged(change);
        }

        // TODO: この部分の処理は間違っている。Documentをクローズした場合にのみ呼び出されてほしい
        void VisibleDockables_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var vm = e.OldItems?.Cast<Document>().FirstOrDefault()?.DataContext as DockPictureViewModel;
                Pictures!.Remove(vm);
            }
        }

        void Pictures_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (DocumentTemplate?.Content is null)
                        {
                            return;
                        }

                        var data = e.NewItems?.Count == 1 ? e.NewItems[0] : null;
                        if (data is not DockPictureViewModel vm)
                        {
                            return;
                        }
                        var document = new PictureDocument
                        {
                            DataContext = vm,
                            CanFloat = false,
                            Title = vm.Subject,
                            Content = DocumentTemplate.Content
                        };
                        document.Bind(PictureDocument.ClosingActionProperty, new Binding
                        {
                            Source = vm,
                            Path = nameof(vm.OnClosing)
                        });
                        document.Bind(PictureDocument.ClosableProperty, new Binding
                        {
                            Source = vm,
                            Path = nameof(vm.Closable)
                        });
                        document.Bind(PictureDocument.SaveAlertResultProperty, new Binding
                        {
                            Source = vm,
                            Path = nameof(vm.SaveAlertResult)
                        });
                        document.Bind(TitleProperty, new Binding
                        {
                            Source = vm,
                            Path = nameof(vm.Subject)
                        });
                        document.Factory = Factory;

                        Factory?.AddDockable(this, document);
                        Factory?.SetActiveDockable(document);
                        Factory?.SetFocusedDockable(this, document);
                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                    {
                        var data = e.OldItems?.Count == 1 ? e.OldItems[0] : null;
                        if (data is null || data is not DockPictureViewModel vm)
                        {
                            break;
                        }

                        vm.Enabled = false;

                        //var document = VisibleDockables?.Cast<Document>().Where(d => ReferenceEquals(d.DataContext, vm)).FirstOrDefault();
                        //if (document is not null)
                        //{
                        //    Owner!.Factory!.CloseDockable(document);
                        //}
                        break;
                    }
            }
        }
    }
}