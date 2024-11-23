using HarmonyLib;
using System.Collections.Generic;
using WeatherRegistry;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(WeatherManager))]
    public class WeatherManagerPatch // From WeatherRegistry by Mrov
    {
        [HarmonyPatch("GetWeather")]
        [HarmonyPrefix]
        public static void GetWeatherPatch(LevelWeatherType levelWeatherType)
        {
            BetterFog.mls.LogInfo("GetWeather Activated");
            List<Weather> allWeathers = WeatherManager.Weathers;
            BetterFog.currentWeatherType = allWeathers.Find((Weather weather) => weather.VanillaWeatherType == levelWeatherType).Name.ToLower();
            BetterFog.mls.LogInfo($"Weather changed to {BetterFog.currentWeatherType}");

            if(BetterFog.autoPresetModeEnabled)
                BetterFog.ApplyFogSettings(true);
            else
                BetterFog.ApplyFogSettings(false);
        }
    }
}
