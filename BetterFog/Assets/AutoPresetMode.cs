using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BetterFog.Assets
{
    public class AutoPresetMode
    {
        public AutoPresetMode(string condition, string effect)
        {
            var conditions = condition.Split('&');
            // Remove all leading numbers and spaces from the conditions
            Conditions = conditions
                .Select(c => Regex.Replace(c.TrimStart(), @"^[\d\s]+", "").ToLower())
                .ToList();
            Effect = effect;
        }

        public List<string> Conditions { get; set; }
        public string Effect { get; set; }
    }
}
