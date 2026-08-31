using System;
using System.Threading.Tasks;

namespace Eede.Application.Infrastructure;

public interface IFileStorage
{
    Task<Uri?> OpenFilePickerAsync();
    Task<Uri?> OpenAnimationFilePickerAsync();
    Task<Uri?> OpenPaletteFilePickerAsync();
    Task<Uri?> SaveAnimationFilePickerAsync();
    Task<Uri?> SavePaletteFilePickerAsync();
    Task<Uri?> SaveFilePickerAsync();
    Task<System.IO.Stream> OpenReadStreamAsync(Uri uri);
    Task<System.IO.Stream> OpenWriteStreamAsync(Uri uri);
}
