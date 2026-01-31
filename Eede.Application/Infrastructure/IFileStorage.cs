using System;
using System.Threading.Tasks;

namespace Eede.Application.Infrastructure;

public interface IFileStorage
{
    Task<Uri?> OpenFilePickerAsync();
    Task<Uri?> OpenAnimationFilePickerAsync();
    Task<Uri?> SaveAnimationFilePickerAsync();
    Task<Uri?> SaveFilePickerAsync();
}
