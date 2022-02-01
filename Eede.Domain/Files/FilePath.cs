using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.Files
{
    public class FilePath
    {
        public readonly string Path;
        public FilePath(string filePath)
        {
            Path = filePath;
        }

        public bool IsEmpty()
        {
            return Path == "";
        }
    }
}
