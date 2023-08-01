using Eede.Application.Pictures;
using Eede.Domain.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures
{
    public class LoadPictureUseCase
    {
        private readonly FilePath filePath;

        public LoadPictureUseCase(FilePath filePath)
        {
            this.filePath = filePath;
        }

        //public PictureFile Execute(PictureFileReader reader) {
        //    using (var picture = reader.Read())
        //    {

        //   new PictureWindow(filename, picture, paintableBox1);
        //    }
        //}
    }
}
