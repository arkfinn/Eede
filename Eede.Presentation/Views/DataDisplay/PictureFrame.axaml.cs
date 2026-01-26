using Avalonia;
using Avalonia.Controls;
using Dock.Model.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace Eede.Presentation.Views.DataDisplay
{
    public partial class PictureFrame : UserControl
    {
        public PictureFrame()
        {
            InitializeComponent();
            var factory = App.Services?.GetService<InjectableDockFactory>();
            if (factory != null)
            {
                DockControl.Factory = factory;
            }
        }

        public static readonly DirectProperty<PictureFrame, IList?> PicturesProperty =
            AvaloniaProperty.RegisterDirect<PictureFrame, IList?>(
                nameof(Pictures),
                o => o.Pictures,
                (o, v) => o.Pictures = v);

        private IList? pictures;
        public IList? Pictures
        {
            get => pictures;
            set => SetAndRaise(PicturesProperty, ref pictures, value);
        }

        public static readonly DirectProperty<PictureFrame, IDockable?> ActiveDockableProperty =
            AvaloniaProperty.RegisterDirect<PictureFrame, IDockable?>(
                nameof(ActiveDockable),
                o => o.ActiveDockable,
                (o, v) => o.ActiveDockable = v);

        private IDockable? activeDockable;
        public IDockable? ActiveDockable
        {
            get => activeDockable;
            set => SetAndRaise(ActiveDockableProperty, ref activeDockable, value);
        }
    }
}
