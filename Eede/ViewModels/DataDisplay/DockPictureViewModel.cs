using Avalonia.Controls.Shapes;
using Avalonia.Media.Imaging;
using Dock.Model.Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Eede.ViewModels.DataDisplay
{
    public class DockPictureViewModel : ViewModelBase
    {
        public DockPictureViewModel(Uri path)
        {
            Path = path;
            var fullPath = HttpUtility.UrlDecode(path.AbsolutePath);
            Subject = fullPath;
            Picture = new Bitmap(fullPath);
        }

        public DockPictureViewModel(string subject, Bitmap picture)
        {
            Subject = subject;
            Picture = picture;
        }

        public Uri? Path { get; private set; } 

        public string Subject { get; private set; }

        public Bitmap Picture { get; set; }
    }
}
