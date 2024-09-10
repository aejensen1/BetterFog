using System.Collections.Generic;

namespace BetterFog.Assets
{
    public class WeatherScale
    {
        public string WeatherName { get; set; }
        public float Scale { get; set; }

        public WeatherScale(string weatherName, float weatherScale)
        {
            WeatherName = weatherName;
            Scale = weatherScale;
        }

        public List<WeatherScale> ParseWeatherScales(string configString)
        {
            var weatherScales = new List<WeatherScale>();
            var pairs = configString.Split(';');

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2 && float.TryParse(keyValue[1], out float scaleValue))
                {
                    weatherScales.Add(new WeatherScale(keyValue[0], scaleValue));
                }
            }
            return weatherScales;
        }

    }
}
