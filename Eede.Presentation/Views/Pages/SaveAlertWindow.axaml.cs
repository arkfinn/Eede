using Avalonia.Controls;
using Avalonia.Interactivity;
using Eede.Presentation.Common.Enums;
using Eede.Presentation.ViewModels.Pages;
using System.Windows.Input;

namespace Eede.Views.Pages
{
#nullable enable

    public partial class SaveAlertWindow : Window
    {
        private readonly SaveAlertViewModel _viewModel;

        public SaveAlertWindow() : this("無題")
        {
        }

        public SaveAlertWindow(string fileName)
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
            _viewModel = new SaveAlertViewModel(fileName, r =>
            {
                Result = r;
                Close();
            });
            FileName = _viewModel.FileName;
            SaveCommand = _viewModel.SaveCommand;
            NoSaveCommand = _viewModel.NoSaveCommand;
            CancelCommand = _viewModel.CancelCommand;
            DataContext = _viewModel;
        }

        public SaveAlertResult Result { get; private set; } = SaveAlertResult.Cancel;
        public string FileName { get; }

        public ICommand SaveCommand { get; }
        public ICommand NoSaveCommand { get; }
        public ICommand CancelCommand { get; }

        public void OnSave(object? sender, RoutedEventArgs e) => SaveCommand.Execute(null);
        public void OnNoSave(object? sender, RoutedEventArgs e) => NoSaveCommand.Execute(null);
        public void OnCancel(object? sender, RoutedEventArgs e) => CancelCommand.Execute(null);
    }
}
