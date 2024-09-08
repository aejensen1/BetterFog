using HarmonyLib;
using UnityEngine;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch
    {
        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        public static void StartGamePatch()
        {
            if (GameNetworkManager.Instance.gameHasStarted)
            {
                BetterFog.mls.LogInfo("Game has started. Applying fog settings to moon.");

                
                // Start applying fog settings gradually
                if (!BetterFog.applyingFogSettings)
                {
                    BetterFog.ApplyFogSettingsGradually(2f, 0.9f); // Add 2 seconds of fog update delay for when ship lands. May need to change if ship landing time changes.
                }             
            }
        }

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
                Debug.LogError($"Invalid weather index: {currentLevel.currentWeather}");
            }
        }

    }
}
