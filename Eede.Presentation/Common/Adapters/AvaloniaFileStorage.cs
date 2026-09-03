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
        private static readonly Dictionary<string, IStorageFile> StaticFileCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, byte[]> StaticDataCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> StaticNameCache = new(StringComparer.OrdinalIgnoreCase);

        public static void CacheFile(IStorageFile? file)
        {
            if (file == null) return;
            string? name = file.Name;
            if (file.Path != null)
            {
                RegisterCacheKey(file.Path.ToString(), file, name);
                RegisterCacheKey(file.Path.OriginalString, file, name);
                if (file.Path.IsAbsoluteUri)
                {
                    if (file.Path.IsFile)
                    {
                        RegisterCacheKey(file.Path.LocalPath, file, name);
                    }
                    RegisterCacheKey(file.Path.AbsoluteUri, file, name);
                }
            }
            if (!string.IsNullOrEmpty(name))
            {
                RegisterCacheKey(name, file, name);
                RegisterCacheKey("/" + name, file, name);
                RegisterCacheKey("\\" + name, file, name);
            }
        }

        private static void RegisterCacheKey(string key, IStorageFile file, string? name)
        {
            StaticFileCache[key] = file;
            if (!string.IsNullOrEmpty(name))
            {
                StaticNameCache[key] = name;
            }
        }

        public static string? TryGetOriginalFileName(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (StaticNameCache.TryGetValue(path, out var name)) return name;
            string fileName = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(fileName) && StaticNameCache.TryGetValue(fileName, out name)) return name;
            return null;
        }

        public static async Task<System.IO.Stream?> TryOpenReadStreamStaticAsync(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            // 1. メモリキャッシュにあれば即座に MemoryStream を返す（ブラウザでの再読み込み・多重オープンを100%保証）
            if (StaticDataCache.TryGetValue(path, out var cachedBytes))
            {
                return new System.IO.MemoryStream(cachedBytes);
            }
            string fileName = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(fileName) && StaticDataCache.TryGetValue(fileName, out cachedBytes))
            {
                return new System.IO.MemoryStream(cachedBytes);
            }

            // 2. IStorageFile キャッシュから読み込み、全バイトをメモリキャッシュする
            if (StaticFileCache.TryGetValue(path, out var file) ||
                (!string.IsNullOrEmpty(fileName) && StaticFileCache.TryGetValue(fileName, out file)) ||
                (!string.IsNullOrEmpty(fileName) && StaticFileCache.TryGetValue("/" + fileName, out file)))
            {
                try
                {
                    await using var stream = await file.OpenReadAsync();
                    using var ms = new System.IO.MemoryStream();
                    await stream.CopyToAsync(ms);
                    byte[] bytes = ms.ToArray();

                    StaticDataCache[path] = bytes;
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        StaticDataCache[fileName] = bytes;
                    }
                    if (!string.IsNullOrEmpty(file.Name))
                    {
                        StaticDataCache[file.Name] = bytes;
                        StaticDataCache["/" + file.Name] = bytes;
                        StaticDataCache["\\" + file.Name] = bytes;
                    }
                    if (file.Path != null)
                    {
                        StaticDataCache[file.Path.ToString()] = bytes;
                        StaticDataCache[file.Path.OriginalString] = bytes;
                    }

                    return new System.IO.MemoryStream(bytes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AvaloniaFileStorage] Error reading static cached file: {ex}");
                }
            }
            return null;
        }

        public static async Task<System.IO.Stream?> TryOpenWriteStreamStaticAsync(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            string fileName = System.IO.Path.GetFileName(path);
            if (StaticFileCache.TryGetValue(path, out var file) ||
                (!string.IsNullOrEmpty(fileName) && StaticFileCache.TryGetValue(fileName, out file)) ||
                (!string.IsNullOrEmpty(fileName) && StaticFileCache.TryGetValue("/" + fileName, out file)))
            {
                return await file.OpenWriteAsync();
            }
            return null;
        }

        private static bool TryGetCachedFile(Uri uri, out IStorageFile? file)
        {
            string uriStr = uri.ToString();
            string origStr = uri.OriginalString;
            string fileName = System.IO.Path.GetFileName(origStr);

            if (StaticFileCache.TryGetValue(uriStr, out file) ||
                StaticFileCache.TryGetValue(origStr, out file) ||
                (!string.IsNullOrEmpty(fileName) && StaticFileCache.TryGetValue(fileName, out file)) ||
                (!string.IsNullOrEmpty(fileName) && StaticFileCache.TryGetValue("/" + fileName, out file)) ||
                (uri.IsAbsoluteUri && uri.IsFile && StaticFileCache.TryGetValue(uri.LocalPath, out file)))
            {
                return true;
            }
            file = null;
            return false;
        }

        public async Task<System.IO.Stream> OpenReadStreamAsync(Uri uri)
        {
            if (TryGetCachedFile(uri, out var cachedFile) && cachedFile != null)
            {
                return await cachedFile.OpenReadAsync();
            }
            try
            {
                var file = await StorageProvider.TryGetFileFromPathAsync(uri);
                if (file != null)
                {
                    return await file.OpenReadAsync();
                }
            }
            catch
            {
                // StorageProvider unsupported or failed
            }

            if (uri.IsAbsoluteUri && uri.IsFile && System.IO.File.Exists(uri.LocalPath))
            {
                return new System.IO.FileStream(uri.LocalPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            }
            if (System.IO.File.Exists(uri.OriginalString))
            {
                return new System.IO.FileStream(uri.OriginalString, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            }
            throw new System.IO.FileNotFoundException($"Could not open file at {uri}");
        }

        public async Task<System.IO.Stream> OpenWriteStreamAsync(Uri uri)
        {
            if (TryGetCachedFile(uri, out var cachedFile) && cachedFile != null)
            {
                return await cachedFile.OpenWriteAsync();
            }
            try
            {
                var file = await StorageProvider.TryGetFileFromPathAsync(uri);
                if (file != null)
                {
                    return await file.OpenWriteAsync();
                }
            }
            catch
            {
                // StorageProvider unsupported or failed
            }

            if (uri.IsAbsoluteUri && uri.IsFile)
            {
                return new System.IO.FileStream(uri.LocalPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            }
            return new System.IO.FileStream(uri.OriginalString, System.IO.FileMode.Create, System.IO.FileAccess.Write);
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

            CacheFile(result[0]);
            return result[0].Path;
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

            CacheFile(result[0]);
            return result[0].Path;
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

            CacheFile(result);
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

            CacheFile(result[0]);
            return result[0].Path;
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

            CacheFile(result);
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
                SuggestedFileName = "image.png",
                DefaultExtension = "png",
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
            CacheFile(result);
            return result.Path;
        }
    }
}
