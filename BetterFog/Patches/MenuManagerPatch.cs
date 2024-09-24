using HarmonyLib;
using BetterFog.Assets;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    public class MenuManagerPatch //For when the settings menu in main menu is closed
    {
        [HarmonyPatch("PlayCancelSFX")]
        [HarmonyPostfix]
        public static void PlayCancelSFXPatch()
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