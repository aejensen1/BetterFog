﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterFog.Assets
{
    public class AutoPresetMode
    {
        public AutoPresetMode(string condition, string effect)
        {
            var conditions = condition.Split('&');
            Conditions = conditions.ToList();
            Effect = effect;
        }

        public List<string> Conditions { get; set; }
        public string Effect { get; set; }
    }
}
