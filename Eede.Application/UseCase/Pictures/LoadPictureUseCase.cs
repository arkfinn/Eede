using Eede.Application.Infrastructure;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using System;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures
{
    public class LoadPictureUseCase : ILoadPictureUseCase
    {
        private readonly IPictureRepository _pictureRepository;
        private readonly ISettingsRepository _settingsRepository;

        public LoadPictureUseCase(IPictureRepository pictureRepository, ISettingsRepository settingsRepository)
        {
            _pictureRepository = pictureRepository;
            _settingsRepository = settingsRepository;
        }

        public async Task<Picture> ExecuteAsync(FilePath path)
        {
            var picture = await _pictureRepository.LoadAsync(path);
            var settings = await _settingsRepository.LoadAsync();
            settings.AddRecentFile(path.ToString(), DateTime.Now);
            await _settingsRepository.SaveAsync(settings);
            return picture;
        }
    }
}
