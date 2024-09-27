using HarmonyLib;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    public class TimeOfDayPatch
    {
        [HarmonyPrefix]
        public static bool SetWeatherBasedOnVariablesPatch()
        {
            //BetterFog.mls.LogInfo("TimeOfDay SetWeatherBasedOnVariables. Skipping fog density changes.");
            return false; // Skip the original method
        }
    }
}


