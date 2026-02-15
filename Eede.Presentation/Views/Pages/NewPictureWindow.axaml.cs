using Avalonia.Controls;
using Eede.Presentation.ViewModels.Pages;
using ReactiveUI;
using System;

namespace Eede.Presentation.Views.Pages;

public partial class NewPictureWindow : Window
{
    public NewPictureWindow()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            if (DataContext is NewPictureWindowViewModel vm)
            {
                vm.Close = () => Close();
            }
        });
    }
}