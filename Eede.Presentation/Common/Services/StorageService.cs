using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eede.Presentation.Common.Services
{
    public class StorageService(IStorageProvider storageProvider)
    {
        public readonly IStorageProvider StorageProvider = storageProvider;

        public async Task<Uri> OpenFilePickerAsync()
        {
            FilePickerOpenOptions options = new()
            {
                AllowMultiple = false,
                FileTypeFilter = GetImageFileTypes(),
                //        Title = Title,
            };

            IReadOnlyList<IStorageFile> result = await StorageProvider.OpenFilePickerAsync(options);

            return result == null || result.Count == 0 ? null : result[0].Path;
        }

        private static List<FilePickerFileType> GetImageFileTypes()
        {
            return
            [
                new("All Images")
                {
                    Patterns = ["*.png", "*.bmp"],
                    AppleUniformTypeIdentifiers = ["public.image"],
                    MimeTypes = ["image/*"]
                },
                new("PNG Image")
                {
                    Patterns = ["*.png"],
                    AppleUniformTypeIdentifiers = ["public.png"],
                    MimeTypes = ["image/png"]
                },
                new("BMP Image")
                {
                    Patterns = ["*.bmp"],
                    AppleUniformTypeIdentifiers = ["public.bmp"],
                    MimeTypes = ["image/bmp"]
                },
                new("All")
                {
                    Patterns = ["*.*"],
                    MimeTypes = ["*/*"]
                }
            ];
        }

        public async Task<Uri> SaveFilePickerAsync()
        {
            FilePickerSaveOptions options = new()
            {

            };
            IStorageFile result = await StorageProvider.SaveFilePickerAsync(options);

            return result?.Path;
        }
    }
}
