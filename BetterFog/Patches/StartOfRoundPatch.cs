﻿using HarmonyLib;

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
                if (!BetterFog.loggingCoroutineRunning)
                {
                    BetterFog.LogMeanFreePath();
                }
                
            }
        }
    }
}
