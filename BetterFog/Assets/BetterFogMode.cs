using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterFog.Assets
{
    public class BetterFogMode
    {
        public BetterFogMode(string name) 
        {
            Name = name;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FogType { get; set; }
    }
}
