using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Eede.ViewModels.DataDisplay;
using System;

namespace Eede.Views.DataEntry
{
    public partial class PictureContainer : UserControl
    {
        public PictureContainer()
        {
            InitializeComponent();
            DataContextChanged += PictureContainer_DataContextChanged;
        }

        private DockPictureViewModel? FetchViewModel()
        {
            if (DataContext is DockPictureViewModel vm)
            {
                return vm;
            }
            if(DataContext is StyledElement e)
            {
                return e.DataContext as DockPictureViewModel;
            }
            return null;
        }
        private void PictureContainer_DataContextChanged(object? sender, EventArgs e)
        {
            var vm = FetchViewModel();
            if (vm == null)
            {
                return;
            }
            var bitmap = vm.Picture;
            var background = this.FindControl<Canvas>("background");
            if (background != null)
            {
                background.Width = bitmap.Size.Width;
                background.Height = bitmap.Size.Height;
            }
            var canvas = this.FindControl<Canvas>("canvas");
            if (canvas != null)
            {
                canvas.Width = bitmap.Size.Width;
                canvas.Height = bitmap.Size.Height;
                canvas.Background = new ImageBrush(bitmap);
            }
        }
    }
}

