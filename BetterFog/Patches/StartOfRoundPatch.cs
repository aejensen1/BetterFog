using HarmonyLib;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch
    {
        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        public static void StartGame_MyPatch()
        {
            BetterFog.mls.LogInfo("Game has started - FOG");
            if (GameNetworkManager.Instance.gameHasStarted)
            {
                BetterFog.ApplyFogSettings();
            }
        }
    }
}
