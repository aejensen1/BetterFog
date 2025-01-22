using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterFog.Assets
{
    public class DropdownRowData
    {
        public DropdownRowData(string presetName, int delay)
        {
            PresetName = presetName;
            Delay = delay;
        }
        public string PresetName { get; set; }
        public int Delay { get; set; }
    }

    //use: private List<DropdownData> dropdownDataList = new List<DropdownData>();
}
