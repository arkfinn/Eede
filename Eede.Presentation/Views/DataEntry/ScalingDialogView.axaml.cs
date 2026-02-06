using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Eede.Presentation.ViewModels.DataEntry;
using ReactiveUI;
using System;

namespace Eede.Presentation.Views.DataEntry
{
    public partial class ScalingDialogView : ReactiveWindow<ScalingDialogViewModel>
    {
        public ScalingDialogView()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(d =>
            {
                d(ViewModel.OkCommand.Subscribe(context => Close(context)));
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
