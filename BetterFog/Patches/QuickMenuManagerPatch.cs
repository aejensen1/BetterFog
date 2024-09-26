using HarmonyLib;
using BetterFog.Assets;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(QuickMenuManager))]
    public class QuickMenuManagerPatch // For when the player opens the quick menu in game (ESC)
    {
        [HarmonyPatch("OpenQuickMenu")]
        [HarmonyPostfix]
        public static void OpenQuickMenuPatch()
        {
            var fogSettingsManager = FogSettingsManager.Instance;
            if (fogSettingsManager != null && !fogSettingsManager.IsSettingsEnabled())
            {
                fogSettingsManager.EnableSettings();
                fogSettingsManager.UpdateSettings();
            }
            else
            {
                //BetterFog.mls.LogWarning("Settings are already open.");
            }
        }

        [HarmonyPatch("CloseQuickMenu")]
        [HarmonyPostfix]
        public static void CloseQuickMenuPatch()
        {
            var fogSettingsManager = FogSettingsManager.Instance;
            if (fogSettingsManager != null && fogSettingsManager.IsSettingsEnabled())
            {
                fogSettingsManager.DisableSettings();
            }
            else
            {
                //BetterFog.mls.LogWarning("Settings are already closed.");
            }
        }
    }
}