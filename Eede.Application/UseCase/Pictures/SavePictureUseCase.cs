using Eede.Application.Infrastructure;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using System;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures
{
    public class SavePictureUseCase : ISavePictureUseCase
    {
        private readonly IPictureRepository _pictureRepository;
        private readonly ISettingsRepository _settingsRepository;

        public SavePictureUseCase(IPictureRepository pictureRepository, ISettingsRepository settingsRepository)
        {
            _pictureRepository = pictureRepository;
            _settingsRepository = settingsRepository;
        }

        public async Task ExecuteAsync(Picture picture, FilePath path)
        {
            await _pictureRepository.SaveAsync(picture, path);
            var settings = await _settingsRepository.LoadAsync();
            settings.AddRecentFile(path.ToString(), DateTime.Now);
            await _settingsRepository.SaveAsync(settings);
        }
    }
}
