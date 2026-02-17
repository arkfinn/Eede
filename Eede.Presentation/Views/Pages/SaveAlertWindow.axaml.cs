using Avalonia.Controls;
using Avalonia.Interactivity;
using Eede.Presentation.Common.Enums;
using ReactiveUI;
using System.Reactive;
using System.Windows.Input;

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
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
            FileName = fileName;
            SaveCommand = ReactiveCommand.Create(() => { Result = SaveAlertResult.Save; Close(); });
            NoSaveCommand = ReactiveCommand.Create(() => { Result = SaveAlertResult.NoSave; Close(); });
            CancelCommand = ReactiveCommand.Create(() => { Result = SaveAlertResult.Cancel; Close(); });
            DataContext = this;
        }

        public SaveAlertResult Result { get; private set; } = SaveAlertResult.Cancel;
        public string FileName { get; }

        public ICommand SaveCommand { get; }
        public ICommand NoSaveCommand { get; }
        public ICommand CancelCommand { get; }

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
