using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageTransfers;

namespace Eede.Presentation.Views.Navigation
{
    public partial class LayerStyleMenu : UserControl
    {
        public LayerStyleMenu()
        {
            InitializeComponent();
            UpdateChecked();
        }

        public static readonly StyledProperty<IImageTransfer> ImageTransferProperty =
            AvaloniaProperty.Register<LayerStyleMenu, IImageTransfer>(
                nameof(ImageTransfer), new DirectImageTransfer(), defaultBindingMode: BindingMode.TwoWay);
        public IImageTransfer ImageTransfer
        {
            get => GetValue(ImageTransferProperty);
            set => SetValue(ImageTransferProperty, value);
        }

        public static readonly StyledProperty<IImageBlender> ImageBlenderProperty =
            AvaloniaProperty.Register<LayerStyleMenu, IImageBlender>(
                nameof(ImageBlender), new DirectImageBlender(), defaultBindingMode: BindingMode.TwoWay);
        public IImageBlender ImageBlender
        {
            get => GetValue(ImageBlenderProperty);
            set => SetValue(ImageBlenderProperty, value);
        }

        public void SetRgbLayer(object sender, RoutedEventArgs e)
        {
            ImageTransfer = new RGBToneImageTransfer();
            ImageBlender = new RGBOnlyImageBlender();
            UpdateChecked();
        }

        public void SetAlphaLayer(object sender, RoutedEventArgs e)
        {
            ImageTransfer = new AlphaToneImageTransfer();
            ImageBlender = new AlphaOnlyImageBlender();
            UpdateChecked();
        }

        public void SetArgbLayer(object sender, RoutedEventArgs e)
        {
            ImageTransfer = new DirectImageTransfer();
            ImageBlender = new DirectImageBlender();
            UpdateChecked();
        }

        private void UpdateChecked()
        {
            ButtonRgb.IsChecked = ImageTransfer is RGBToneImageTransfer;
            ButtonAlpha.IsChecked = ImageTransfer is AlphaToneImageTransfer;
            ButtonArgb.IsChecked = ImageTransfer is DirectImageTransfer;
        }

        /*
         *             paintableBox1.ChangeImageTransfer(new RGBToneImageTransfer());
            paintableBox1.ChangeImageBlender(new RGBOnlyImageBlender());
            toolStripButton9.Checked = true;
            toolStripButton10.Checked = false;
            toolStripButton11.Checked = false;
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            paintableBox1.ChangeImageTransfer(new AlphaToneImageTransfer());
            paintableBox1.ChangeImageBlender(new AlphaOnlyImageBlender());
            toolStripButton9.Checked = false;
            toolStripButton10.Checked = true;
            toolStripButton11.Checked = false;
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            paintableBox1.ChangeImageTransfer(new DirectImageTransfer());
            paintableBox1.ChangeImageBlender(new DirectImageBlender());
        */

    }
}
