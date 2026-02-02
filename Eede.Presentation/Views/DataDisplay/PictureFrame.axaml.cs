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

        public static readonly StyledProperty<Eede.Domain.ImageEditing.Blending.IImageBlender> ImageBlenderProperty =
            AvaloniaProperty.Register<PictureFrame, Eede.Domain.ImageEditing.Blending.IImageBlender>(nameof(ImageBlender), new Eede.Domain.ImageEditing.Blending.DirectImageBlender());
        public Eede.Domain.ImageEditing.Blending.IImageBlender ImageBlender
        {
            get => GetValue(ImageBlenderProperty);
            set => SetValue(ImageBlenderProperty, value);
        }

        public static readonly StyledProperty<Eede.Domain.Palettes.ArgbColor> BackgroundColorProperty =
            AvaloniaProperty.Register<PictureFrame, Eede.Domain.Palettes.ArgbColor>(nameof(BackgroundColor), new Eede.Domain.Palettes.ArgbColor(0, 0, 0, 0));
        public Eede.Domain.Palettes.ArgbColor BackgroundColor
        {
            get => GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        private IDockable? activeDockable;
        public IDockable? ActiveDockable
        {
            get => activeDockable;
            set => SetAndRaise(ActiveDockableProperty, ref activeDockable, value);
        }
    }
}
