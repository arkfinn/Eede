using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Dock.Model.Avalonia.Controls;
using Eede.Presentation.Common.Enums;
using Eede.Views.Pages;
using System;
using System.Windows.Input;

namespace Eede.Presentation.Views.DataDisplay
{
    public class PictureDocument : Document
    {

        public static readonly StyledProperty<ICommand> ClosingActionProperty =
            AvaloniaProperty.Register<PictureDocument, ICommand>(nameof(ClosingAction));
        public ICommand ClosingAction
        {
            get => GetValue(ClosingActionProperty);
            set => SetValue(ClosingActionProperty, value);
        }

        public Action CloseAction { get; set; }

        public static readonly StyledProperty<bool> ClosableProperty =
            AvaloniaProperty.Register<PictureDocument, bool>(nameof(Closable));
        public bool Closable
        {
            get => GetValue(ClosableProperty);
            set => SetValue(ClosableProperty, value);
        }

        public static readonly StyledProperty<SaveAlertResult> SaveAlertResultProperty =
      AvaloniaProperty.Register<PictureDocument, SaveAlertResult>(nameof(SaveAlertResult), SaveAlertResult.Cancel, defaultBindingMode: BindingMode.TwoWay);
        public SaveAlertResult SaveAlertResult
        {
            get => GetValue(SaveAlertResultProperty);
            set => SetValue(SaveAlertResultProperty, value);
        }


        public override bool OnClose()
        {
            if (Closable)
            {
                CloseAction?.Invoke();
                return base.OnClose();
            }
            OpenSaveAlertDialog();
            return false;
        }

        private async void OpenSaveAlertDialog()
        {
            SaveAlertWindow window = new();

            Window mainWindow = ((IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current.ApplicationLifetime).MainWindow;

            //var parent = this.GetVisualRoot() as Window ?? new Window();
            await window.ShowDialog(mainWindow);
            SaveAlertResult = window.Result;
            ClosingAction?.Execute(null);
            if (Closable)
            {
                Factory?.CloseDockable(this);
            }
        }
    }
}
