using Avalonia.Controls;
using Avalonia.Interactivity;
using Eede.Common.Enums;

namespace Eede.Views.Pages
{
    public partial class SaveAlertWindow : Window
    {
        public SaveAlertWindow()
        {
            InitializeComponent();
        }

        public SaveAlertResult Result { get; private set; } = SaveAlertResult.Cancel;

        public void OnSave(object sender, RoutedEventArgs e)
        {
            Result = SaveAlertResult.Save;
            Close();
        }

        public void OnNoSave(object sender, RoutedEventArgs e)
        {
            Result = SaveAlertResult.NoSave;
            Close();
        }

        public void OnCancel(object sender, RoutedEventArgs e)
        {
            Result = SaveAlertResult.Cancel;
            Close();
        }
    }
}
