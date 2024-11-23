using HarmonyLib;

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
            SelectableLevel currentLevel = __instance.currentLevel;

            //// Check if the currentWeather value is within the bounds of the effects array
            //if ((int)currentLevel.currentWeather >= 0 && (int)currentLevel.currentWeather < TimeOfDay.Instance.effects.Length)
            //{
            //    BetterFog.mls.LogInfo($"Weather changed from {BetterFog.currentWeatherType} to {TimeOfDay.Instance.effects[(int)currentLevel.currentWeather].name.ToLower()}");
            //    WeatherEffect weatherEffect = TimeOfDay.Instance.effects[(int)currentLevel.currentWeather];

            //    // Log the current weather type
                
            //    BetterFog.currentWeatherType = weatherEffect.name.ToLower();
            //}
            // Check if the currentWeather value is within the bounds of the effects array
            
            
            //if ((int)currentLevel.currentWeather >= 0)
            //{
            //    BetterFog.mls.LogInfo($"Weather changed from {BetterFog.currentWeatherType} to {TimeOfDay.Instance.effects[(int)currentLevel.currentWeather].name.ToLower()}");
            //    WeatherEffect weatherEffect = TimeOfDay.Instance.effects[(int)currentLevel.currentWeather];

            //    // Log the current weather type

            //    BetterFog.currentWeatherType = weatherEffect.name.ToLower();
            //}
            //else
            //{
            //    BetterFog.currentWeatherType = "none";
            //    BetterFog.mls.LogWarning($"Invalid weather index: {currentLevel.currentWeather}. Set to none");
            //}
            BetterFog.currentLevel = currentLevel.PlanetName.ToLower();
            BetterFog.CollectVanillaValues();
            BetterFog.ApplyFogSettings(false);
        }
    }
}
