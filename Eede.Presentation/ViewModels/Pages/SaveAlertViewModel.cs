using Eede.Presentation.Common.Enums;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace Eede.Presentation.ViewModels.Pages;

#nullable enable

public class SaveAlertViewModel : ReactiveObject
{
    public SaveAlertViewModel(string fileName, Action<SaveAlertResult>? onClose = null)
    {
        FileName = fileName ?? string.Empty;
        SaveCommand = ReactiveCommand.Create(() => { Result = SaveAlertResult.Save; onClose?.Invoke(Result); });
        NoSaveCommand = ReactiveCommand.Create(() => { Result = SaveAlertResult.NoSave; onClose?.Invoke(Result); });
        CancelCommand = ReactiveCommand.Create(() => { Result = SaveAlertResult.Cancel; onClose?.Invoke(Result); });
    }

    public string FileName { get; }
    public SaveAlertResult Result { get; private set; } = SaveAlertResult.Cancel;
    public ICommand SaveCommand { get; }
    public ICommand NoSaveCommand { get; }
    public ICommand CancelCommand { get; }
}
