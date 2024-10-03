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
            // Access the current level
            SelectableLevel currentLevel = __instance.currentLevel;

            // Check if the currentWeather value is within the bounds of the effects array
            if ((int)currentLevel.currentWeather >= 0 && (int)currentLevel.currentWeather < TimeOfDay.Instance.effects.Length)
            {
                WeatherEffect weatherEffect = TimeOfDay.Instance.effects[(int)currentLevel.currentWeather];

                // Log the current weather type
                
                BetterFog.currentWeatherType = weatherEffect.name;
            }
            else
            {
                BetterFog.currentWeatherType = "none";
                BetterFog.mls.LogWarning($"Invalid weather index: {currentLevel.currentWeather}. Set to none");
            }
            BetterFog.currentLevel = currentLevel.PlanetName;
            BetterFog.CollectVanillaValues();
        }
    }
}
