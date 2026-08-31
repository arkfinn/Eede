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
        private readonly Dictionary<Uri, IStorageFile> _fileCache = [];

        public async Task<System.IO.Stream> OpenReadStreamAsync(Uri uri)
        {
            if (_fileCache.TryGetValue(uri, out var cachedFile))
            {
                return await cachedFile.OpenReadAsync();
            }
            var file = await StorageProvider.TryGetFileFromPathAsync(uri);
            if (file != null)
            {
                return await file.OpenReadAsync();
            }
            if (uri.IsFile && System.IO.File.Exists(uri.LocalPath))
            {
                return new System.IO.FileStream(uri.LocalPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            }
            throw new System.IO.FileNotFoundException($"Could not open file at {uri}");
        }

        public async Task<System.IO.Stream> OpenWriteStreamAsync(Uri uri)
        {
            if (_fileCache.TryGetValue(uri, out var cachedFile))
            {
                return await cachedFile.OpenWriteAsync();
            }
            var file = await StorageProvider.TryGetFileFromPathAsync(uri);
            if (file != null)
            {
                return await file.OpenWriteAsync();
            }
            if (uri.IsFile)
            {
                return new System.IO.FileStream(uri.LocalPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            }
            throw new System.IO.FileNotFoundException($"Could not open write stream at {uri}");
        }

        public async Task<Uri?> OpenFilePickerAsync()
        {
            FilePickerOpenOptions options = new()
            {
                AllowMultiple = false,
                FileTypeFilter = GetImageFileTypes(),
            };

            IReadOnlyList<IStorageFile> result = await StorageProvider.OpenFilePickerAsync(options);
            if (result == null || result.Count == 0) return null;

            var path = result[0].Path;
            _fileCache[path] = result[0];
            return path;
        }

        public async Task<Uri?> OpenAnimationFilePickerAsync()
        {
            FilePickerOpenOptions options = new()
            {
                AllowMultiple = false,
                FileTypeFilter = GetAnimationFileTypes(),
            };

            IReadOnlyList<IStorageFile> result = await StorageProvider.OpenFilePickerAsync(options);
            if (result == null || result.Count == 0) return null;

            var path = result[0].Path;
            _fileCache[path] = result[0];
            return path;
        }

        public async Task<Uri?> SaveAnimationFilePickerAsync()
        {
            FilePickerSaveOptions options = new()
            {
                FileTypeChoices = GetAnimationFileTypes(),
                SuggestedFileName = "animation_pattern.json"
            };
            IStorageFile? result = await StorageProvider.SaveFilePickerAsync(options);
            if (result == null) return null;

            _fileCache[result.Path] = result;
            return result.Path;
        }

        public async Task<Uri?> OpenPaletteFilePickerAsync()
        {
            FilePickerOpenOptions options = new()
            {
                AllowMultiple = false,
                FileTypeFilter = GetPaletteFileTypes(),
            };

            IReadOnlyList<IStorageFile> result = await StorageProvider.OpenFilePickerAsync(options);
            if (result == null || result.Count == 0) return null;

            var path = result[0].Path;
            _fileCache[path] = result[0];
            return path;
        }

        public async Task<Uri?> SavePaletteFilePickerAsync()
        {
            FilePickerSaveOptions options = new()
            {
                FileTypeChoices = GetPaletteFileTypes(),
                SuggestedFileName = "palette.aact"
            };
            IStorageFile? result = await StorageProvider.SaveFilePickerAsync(options);
            if (result == null) return null;

            _fileCache[result.Path] = result;
            return result.Path;
        }

        private static List<FilePickerFileType> GetPaletteFileTypes()
        {
            return
            [
                 new("Palette File")
                 {
                    Patterns = ["*.aact", "*.act"]
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
                    AppleUniformTypeIdentifiers = ["public.image"]
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
                    Patterns = ["*.*"]
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
            if (result == null) return null;

            _fileCache[result.Path] = result;
            return result.Path;
        }
    }
}
