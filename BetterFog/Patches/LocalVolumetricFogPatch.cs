using HarmonyLib;
using UnityEngine.Rendering.HighDefinition;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(LocalVolumetricFog))] // Assuming OnEnable is when the fog is created or initialized. Used to capture vanilla fog settings for later use.
    public static class LocalVolumetricFogPatch
    {
        static void OnEnablePatch(LocalVolumetricFog __instance)
        {
            if (BetterFog.verboseLoggingEnabled)
                BetterFog.mls.LogInfo("LocalVolumetricFog created, capturing vanilla values.");

            // Capture vanilla parameters
            var fogParams = __instance.parameters;

            // Store the vanilla values into the dictionary
            if (!BetterFog.fogParameterChanges.ContainsKey(__instance.gameObject))
            {
                BetterFog.fogParameterChanges[__instance.gameObject] = fogParams;
                if (BetterFog.verboseLoggingEnabled)
                    BetterFog.mls.LogInfo($"Captured vanilla fog parameters for {__instance.gameObject.name}");
            }
        }
    }
}
