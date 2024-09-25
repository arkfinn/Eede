using System.Drawing;

namespace Eede.Presentation.Settings
{
    internal class GlobalSetting
    {
        private static readonly GlobalSetting _instance = new();

        public static GlobalSetting Instance()
        {
            return _instance;
        }

        private GlobalSetting()
        {

        }

        public Size BoxSize = new(32, 32);

        public bool IsHalfMoveBox = true;
    }
}
