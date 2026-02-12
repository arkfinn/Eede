using Eede.Presentation.Files;

namespace Eede.Presentation.Common.Models
{
#nullable enable

    public record SaveImageResult
    {
        public bool IsSaved { get; }
        public bool IsCanceled { get; }
        public IImageFile? File { get; }

        private SaveImageResult(bool isSaved, bool isCanceled, IImageFile? file)
        {
            IsSaved = isSaved;
            IsCanceled = isCanceled;
            File = file;
        }

        public static SaveImageResult Saved(IImageFile file) => new(true, false, file);
        public static SaveImageResult Canceled() => new(false, true, null);
    }
}
