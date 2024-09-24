using BetterFog.Assets;
using HarmonyLib;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(IngamePlayerSettings))]
    public class IngamePlayerSettingsPatch //For when the player opens the settings menu in th epause menu in game
    {
        [HarmonyPatch("RefreshAndDisplayCurrentMicrophone")]
        [HarmonyPostfix]
        public static void RefreshAndDisplayCurrentMicrophonePatch()
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

        [HarmonyPatch("DiscardChangedSettings")]
        [HarmonyPostfix]
        public static void DiscardChangedSettingsPatch()
        {
            FogSettingsManager.Instance.EnableSettings();
            var fogSettingsManager = FogSettingsManager.Instance;
            if (fogSettingsManager != null && !fogSettingsManager.IsSettingsEnabled())
            {
                fogSettingsManager.EnableSettings();
            }
            else
            {
                BetterFog.mls.LogWarning("Settings are already open.");
            }
        }
    }
}
