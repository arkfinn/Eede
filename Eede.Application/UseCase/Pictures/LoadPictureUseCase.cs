using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures
{
    public class LoadPictureUseCase : ILoadPictureUseCase
    {
        private readonly IPictureRepository _pictureRepository;

        public LoadPictureUseCase(IPictureRepository pictureRepository)
        {
            _pictureRepository = pictureRepository;
        }

        public async Task<Picture> ExecuteAsync(FilePath path)
        {
            return await _pictureRepository.LoadAsync(path);
        }
    }
}
