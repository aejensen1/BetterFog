using BetterFog.Input;
using HarmonyLib;
using UnityEngine;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    public class PlayerControllerBPatch
    {
        [HarmonyPatch("TeleportPlayer")]
        [HarmonyPostfix]
        public static void TeleportPlayerPatch()
        {
            if (GameNetworkManager.Instance.gameHasStarted)
            {
                BetterFog.mls.LogInfo("Teleported player. Applying fog settings to moon.");
                BetterFog.ApplyFogSettings();
            }
        }

        [HarmonyPatch("SpectateNextPlayer")]
        [HarmonyPostfix]
        public static void SpectateNextPlayerPatch()
        {
            if (GameNetworkManager.Instance.gameHasStarted)
            {
                BetterFog.mls.LogInfo("Spectating the next player. Applying fog settings to moon.");
                BetterFog.ApplyFogSettings();
            }
        }
    }
}
