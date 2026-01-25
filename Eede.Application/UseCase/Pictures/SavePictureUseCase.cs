using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures
{
    public class SavePictureUseCase
    {
        private readonly IPictureRepository _pictureRepository;

        public SavePictureUseCase(IPictureRepository pictureRepository)
        {
            _pictureRepository = pictureRepository;
        }

        public async Task ExecuteAsync(Picture picture, FilePath path)
        {
            await _pictureRepository.SaveAsync(picture, path);
        }
    }
}
