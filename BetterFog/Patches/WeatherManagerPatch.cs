using HarmonyLib;
using System.Collections.Generic;
using WeatherRegistry;
using WeatherRegistry.Definitions;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(WeatherManager))]
    public class WeatherManagerPatch // From WeatherRegistry by Mrov
    {
        [HarmonyPatch("GetWeather")]
        [HarmonyPrefix]
        public static void GetWeatherPatch(LevelWeatherType levelWeatherType)
        {
            //BetterFog.mls.LogInfo("GetWeather Activated");
            var allWeathers = WeatherManager.Weathers;
            if (!(allWeathers.Find((Weather weather) => weather.VanillaWeatherType == levelWeatherType).Name.ToLower() == BetterFog.currentWeatherType)) // If the weather has changed
            {
                BetterFog.currentWeatherType = allWeathers.Find((Weather weather) => weather.VanillaWeatherType == levelWeatherType).Name.ToLower();
                //BetterFog.currentWeatherType = WeatherManager.GetCurrentWeather(BetterFog.currentLevelType).Name.ToLower();
                if (BetterFog.verboseLoggingEnabled)
                    BetterFog.mls.LogInfo($"Weather changed to {BetterFog.currentWeatherType}");

                if (BetterFog.autoPresetModeEnabled)
                    BetterFog.ApplyFogSettings(true);
                else
                    BetterFog.ApplyFogSettings(false);
            }
        }
        
    }
}
