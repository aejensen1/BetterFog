using HarmonyLib;
using System.Text.RegularExpressions;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch
    {
        [HarmonyPatch("ChangeLevel")]
        [HarmonyPostfix]
        public static void ChangeLevelPatch(StartOfRound __instance, int levelID)
        {
            BetterFog.mls.LogInfo("ChangeLevelPatch Activated");
            // Access the current level
            BetterFog.currentLevelType = __instance.currentLevel;
            // Remove the starting numbers and spaces from the planet name
            BetterFog.currentLevel = Regex.Replace(BetterFog.currentLevelType.PlanetName, @"^[\d\s]+", "").ToLower();
            BetterFog.CollectVanillaValues();

            if (BetterFog.weatherSaveLoaded)
            {
                int currentWeatherIndex = (int)BetterFog.currentLevelType.currentWeather;
                if (currentWeatherIndex >= 0 && currentWeatherIndex < TimeOfDay.Instance.effects.Length)
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

                if (BetterFog.autoPresetModeEnabled)
                    BetterFog.ApplyFogSettings(true);
                else
                    BetterFog.ApplyFogSettings(false);
            }
        }

        [HarmonyPatch("SetPlanetsWeather")]
        [HarmonyPostfix]
        public static void SetPlanetsWeatherPatch(StartOfRound __instance)
        {
            BetterFog.currentLevelType = __instance.currentLevel;
            // Remove the starting numbers and spaces from the planet name
            BetterFog.currentLevel = Regex.Replace(BetterFog.currentLevelType.PlanetName, @"^[\d\s]+", "").ToLower();
            BetterFog.CollectVanillaValues();

            if (BetterFog.currentLevelType != null)
            {
                LevelWeatherType weatherType = BetterFog.currentLevelType.currentWeather;
                string weatherName = weatherType == LevelWeatherType.None ? "none" : weatherType.ToString().ToLower();

                BetterFog.currentWeatherType = weatherName;

                if (BetterFog.verboseLoggingEnabled)
                    BetterFog.mls.LogInfo($"[BetterFog] Weather pulled from current level: {BetterFog.currentLevelType.PlanetName} | Weather: {weatherName}");
            }
            else
            {
                BetterFog.mls.LogWarning("[BetterFog] currentLevel was null. Could not read weather.");
            }

            if (BetterFog.autoPresetModeEnabled)
                BetterFog.ApplyFogSettings(true);
            else
                BetterFog.ApplyFogSettings(false);
        }
    }
}
