using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace BetterFog.Patches
{
    
    [HarmonyPatch(typeof(NetworkSceneManager))]
    public class NetworkSceneManagerPatch
    {
        [HarmonyPatch("OnSceneLoaded")]
        [HarmonyPostfix]
        public static void OnSceneLoadedPatch()
        {
            BetterFog.mls.LogInfo("OnSceneLoaded invoked. Applying fog settings to moon.");
            BetterFog.ApplyFogSettingsOnGameStart();
        }
    }
}
