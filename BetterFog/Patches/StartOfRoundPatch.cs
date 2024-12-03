using HarmonyLib;
using UnityEngine;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch
    {
        [HarmonyPatch("ChangeLevel")]
        [HarmonyPostfix]
        public static void ChangeLevelPatch(StartOfRound __instance, int levelID)
        {
            //BetterFog.mls.LogInfo("ChangeLevelPatch Activated");
            // Access the current level
            BetterFog.currentLevelType = __instance.currentLevel;
            BetterFog.currentLevel = BetterFog.currentLevelType.PlanetName.ToLower();
            BetterFog.CollectVanillaValues();

            if (BetterFog.weatherSaveLoaded)
            {
                if ((int)BetterFog.currentLevelType.currentWeather >= 0)
                {
                    WeatherEffect weatherEffect = TimeOfDay.Instance.effects[(int)BetterFog.currentLevelType.currentWeather];
                    BetterFog.mls.LogInfo($"Weather changed from {BetterFog.currentWeatherType} to {weatherEffect.name.ToLower()}");
                    BetterFog.currentWeatherType = weatherEffect.name.ToLower();
                }
                else
                {
                    BetterFog.currentWeatherType = "none";
                    BetterFog.mls.LogWarning($"Invalid weather index: {BetterFog.currentLevelType.currentWeather}. Set to none");
                }
                
                if(BetterFog.autoPresetModeEnabled)
                    BetterFog.ApplyFogSettings(true);
                else
                    BetterFog.ApplyFogSettings(false);
            }
        }
    }
}
