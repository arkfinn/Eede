using Avalonia;
using Avalonia.Controls;
using System.Collections;
using System.Collections.Specialized;

namespace Eede.Views.DataDisplay
{
    public partial class PictureFrame : UserControl
    {
        public PictureFrame()
        {
            InitializeComponent();
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
    }
}
