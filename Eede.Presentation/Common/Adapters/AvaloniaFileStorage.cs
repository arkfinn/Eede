using Avalonia.Platform.Storage;
using Eede.Application.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eede.Presentation.Common.Adapters
{
    public class AvaloniaFileStorage(IStorageProvider storageProvider) : IFileStorage
    {
        public readonly IStorageProvider StorageProvider = storageProvider;

        public async Task<Uri?> OpenFilePickerAsync()
        {
            FilePickerOpenOptions options = new()
            {
                AllowMultiple = false,
                FileTypeFilter = GetImageFileTypes(),
            };

            IReadOnlyList<IStorageFile> result = await StorageProvider.OpenFilePickerAsync(options);

            return result == null || result.Count == 0 ? null : result[0].Path;
        }

        public async Task<Uri?> OpenAnimationFilePickerAsync()
        {
            FilePickerOpenOptions options = new()
            {
                AllowMultiple = false,
                FileTypeFilter = GetAnimationFileTypes(),
            };

            IReadOnlyList<IStorageFile> result = await StorageProvider.OpenFilePickerAsync(options);

            return result == null || result.Count == 0 ? null : result[0].Path;
        }

        public async Task<Uri?> SaveAnimationFilePickerAsync()
        {
            FilePickerSaveOptions options = new()
            {
                FileTypeChoices = GetAnimationFileTypes(),
                SuggestedFileName = "animation_pattern.json"
            };
            IStorageFile? result = await StorageProvider.SaveFilePickerAsync(options);

            return result?.Path;
        }

        public async Task<Uri?> OpenPaletteFilePickerAsync()
        {
            FilePickerOpenOptions options = new()
            {
                AllowMultiple = false,
                FileTypeFilter = GetPaletteFileTypes(),
            };

            IReadOnlyList<IStorageFile> result = await StorageProvider.OpenFilePickerAsync(options);

            return result == null || result.Count == 0 ? null : result[0].Path;
        }

        public async Task<Uri?> SavePaletteFilePickerAsync()
        {
            FilePickerSaveOptions options = new()
            {
                FileTypeChoices = GetPaletteFileTypes(),
                SuggestedFileName = "palette.aact"
            };
            IStorageFile? result = await StorageProvider.SaveFilePickerAsync(options);

            return result?.Path;
        }

        private static List<FilePickerFileType> GetPaletteFileTypes()
        {
            return
            [
                 new("Palette File")
                 {
                    Patterns = ["*.aact", "*.act"],
                    MimeTypes = ["image/*"]
                 },
                 new("Palette File (RGBA)")
                 {
                    Patterns = ["*.aact"],
                 },
                  new("Palette File (RGB)")
                 {
                    Patterns = ["*.act"],
                 },
            ];
        }

        private static List<FilePickerFileType> GetAnimationFileTypes()
        {
            return
            [
                new("Animation Pattern")
                {
                    Patterns = ["*.json"],
                    MimeTypes = ["application/json"]
                }
            ];
        }

        private static List<FilePickerFileType> GetImageFileTypes()
        {
            return
            [
                new("All Images")
                {
                    Patterns = ["*.png", "*.bmp", "*.arv"],
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
                new("ARV Image")
                {
                    Patterns = ["*.arv"],
                    AppleUniformTypeIdentifiers = ["public.arv"],
                    MimeTypes = ["image/arv"]
                },
                new("All")
                {
                    Patterns = ["*.*"],
                    MimeTypes = ["*/*"]
                }
            ];
        }

        public async Task<Uri?> SaveFilePickerAsync()
        {
            FilePickerSaveOptions options = new()
            {
                FileTypeChoices =
                [
                    new("PNG Image")
                    {
                        Patterns = ["*.png"],
                        AppleUniformTypeIdentifiers = ["public.png"],
                        MimeTypes = ["image/png"]
                    }
                ]
            };
            IStorageFile? result = await StorageProvider.SaveFilePickerAsync(options);

            return result?.Path;
        }
    }
}
