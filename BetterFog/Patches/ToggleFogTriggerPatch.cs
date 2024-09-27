using HarmonyLib;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(ToggleFogTrigger))]
    public class ToggleFogTriggerPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool UpdatePatch()
        {
            return false; // Skip the original method
        }

        [HarmonyPatch("OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool OnTriggerEnterPatch()
        {
            return false; // Skip the original method
        }

        [HarmonyPatch("OnTriggerExit")]
        [HarmonyPrefix]
        public static bool OnTriggerExitPatch()
        {
            return false; // Skip the original method
        }
    }
}


