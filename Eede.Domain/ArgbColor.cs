using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain
{
    public class ArgbColor
    {
        public readonly byte Alpha; 
        public readonly byte Red;
        public readonly byte Green; 
        public readonly byte Blue;

        public ArgbColor(byte alpha, byte red, byte green, byte blue)
        {
            Alpha = alpha;
            Blue = blue;
            Green = green;
            Red = red;
        }
    }
}
