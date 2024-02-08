using Avalonia.Platform.Storage;

namespace Eede.Presentation.Common.Services
{
    public class StorageService
    {
        public readonly IStorageProvider StorageProvider;
        public StorageService(IStorageProvider storageProvider)
        {
            StorageProvider = storageProvider;
        }
    }
}
