using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Settings
{
    class GlobalSetting
    {
        private static GlobalSetting _instance = new GlobalSetting();

        public static GlobalSetting Instance()
        {
            return _instance;
        }

        private GlobalSetting()
        {
            
        }

        public Size BoxSize = new Size(32, 32);

        public bool IsHalfMoveBox = true;
    }
}
