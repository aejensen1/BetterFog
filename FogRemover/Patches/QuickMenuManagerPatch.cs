using HarmonyLib;
using UnityEngine;
using FogRemover.Components;

namespace FogRemover.Patches
{
    [HarmonyPatch(typeof(QuickMenuManager))]
    public class QuickMenuManagerPatch
    {
        [HarmonyPatch("LeaveGameConfirm")]
        [HarmonyPostfix]
        public static void LeaveGameConfirmPatch()
        {
            if (GameNetworkManager.Instance != null && !HUDManager.Instance.retrievingSteamLeaderboard)
            {
                FogRemover.Bruh.GetComponent<RemoveFog>().TerminateFogObjects();
                Debug.Log("Destroying all fog objects - entered main menu");
            }
        }
    }
}
