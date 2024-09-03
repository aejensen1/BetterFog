using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterFog.Assets
{
    public class WeatherScale
    {
        public string WeatherName { get; set; }
        public float Scale { get; set; }

        public WeatherScale() { }

        public WeatherScale(string weatherName, float scale)
        {
            WeatherName = weatherName;
            Scale = scale;
        }

        public override string ToString()
        {
            return $"{Scale}";
        }

        public static WeatherScale FromString(string data)
        {
            return new WeatherScale(
                data,
                float.Parse(data)
            );
        }
    }
}
