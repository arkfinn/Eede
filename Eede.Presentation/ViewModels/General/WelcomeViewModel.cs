using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Eede.Presentation.ViewModels.General;

public class WelcomeViewModel : Eede.Presentation.ViewModels.ViewModelBase
{
    public ObservableCollection<RecentFile> RecentFiles { get; } = new();

    public string Version => GetVersion();

    public ReactiveCommand<Unit, Unit> CreateNewPictureCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenPictureCommand { get; }
    public ReactiveCommand<string, Unit> OpenRecentFileCommand { get; }
    public ReactiveCommand<string, Unit> OpenUrlCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadRecentFilesCommand { get; }

    private readonly ISettingsRepository _settingsRepository;

    public WelcomeViewModel(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;

        CreateNewPictureCommand = ReactiveCommand.Create(() => { });
        OpenPictureCommand = ReactiveCommand.Create(() => { });
        OpenRecentFileCommand = ReactiveCommand.Create<string>(path => { });
        OpenUrlCommand = ReactiveCommand.Create<string>(url =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        });

        LoadRecentFilesCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var settings = await _settingsRepository.LoadAsync();
            RecentFiles.Clear();
            foreach (var file in settings.RecentFiles)
            {
                RecentFiles.Add(file);
            }
        });
    }

    private string GetVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return fvi.FileVersion ?? "0.0.0";
    }
}
