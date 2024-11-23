using HarmonyLib;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    public class EntranceTeleportPatch
    {
        [HarmonyPatch("TeleportPlayer")]
        [HarmonyPostfix]
        public static void TeleportPlayerPatch()
        {
            if (GameNetworkManager.Instance.gameHasStarted)
            {
                BetterFog.mls.LogInfo("Exited dungeon. Applying fog settings to moon.");
                BetterFog.ApplyFogSettings(false);
            }
        }
    }
}
