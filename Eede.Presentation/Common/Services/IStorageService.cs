using System;
using System.Threading.Tasks;

namespace Eede.Presentation.Common.Services;

public interface IStorageService
{
    Task<Uri?> OpenFilePickerAsync();
    Task<Uri?> OpenAnimationFilePickerAsync();
    Task<Uri?> SaveAnimationFilePickerAsync();
    Task<Uri?> SaveFilePickerAsync();
}
