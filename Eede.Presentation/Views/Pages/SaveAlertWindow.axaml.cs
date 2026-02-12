using Avalonia.Controls;
using Avalonia.Interactivity;
using Eede.Presentation.Common.Enums;

namespace Eede.Views.Pages
{
#nullable enable

    public partial class SaveAlertWindow : Window
    {
        public SaveAlertWindow() : this("無題")
        {
        }

        public SaveAlertWindow(string fileName)
        {
            InitializeComponent();
            FileName = fileName;
            DataContext = this;
        }

        public SaveAlertResult Result { get; private set; } = SaveAlertResult.Cancel;
        public string FileName { get; }

        public void OnSave(object? sender, RoutedEventArgs e)
        {
            Result = SaveAlertResult.Save;
            Close();
        }

        public void OnNoSave(object? sender, RoutedEventArgs e)
        {
            Result = SaveAlertResult.NoSave;
            Close();
        }

        public void OnCancel(object? sender, RoutedEventArgs e)
        {
            Result = SaveAlertResult.Cancel;
            Close();
        }
    }
}
