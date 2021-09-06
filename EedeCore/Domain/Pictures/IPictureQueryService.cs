﻿using Eede.Domain.Files;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.Pictures
{
    public interface IPictureQueryService
    {
       PrimaryPicture Fetch(FilePath filename);
    }
}
