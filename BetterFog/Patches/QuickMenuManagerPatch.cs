using HarmonyLib;
using UnityEngine;
using BetterFog.Assets;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(QuickMenuManager))]
    public class QuickMenuManagerPatch
    {
        [HarmonyPatch("OpenQuickMenu")]
        [HarmonyPostfix]
        public static void OpenQuickMenuPatch()
        {
            var fogSettingsManager = FogSettingsManager.Instance;
            BetterFog.mls.LogInfo("QuickMenuManagerPatch.OpenQuickMenuPatch");
            BetterFog.mls.LogInfo(fogSettingsManager.ToString());
            BetterFog.mls.LogInfo(fogSettingsManager.IsSettingsEnabled());
            if (fogSettingsManager != null && !fogSettingsManager.IsSettingsEnabled())
            {
                fogSettingsManager.EnableSettings();
            }
            else
            {
                BetterFog.mls.LogWarning("Settings are already open.");
            }
        }

        [HarmonyPatch("CloseQuickMenu")]
        [HarmonyPostfix]
        public static void CloseQuickMenuPatch()
        {
            var fogSettingsManager = FogSettingsManager.Instance;
            BetterFog.mls.LogInfo("QuickMenuManagerPatch.CloseQuickMenuPatch");
            BetterFog.mls.LogInfo(fogSettingsManager.ToString());
            BetterFog.mls.LogInfo(fogSettingsManager.IsSettingsEnabled());
            if (fogSettingsManager != null && fogSettingsManager.IsSettingsEnabled())
            {
                fogSettingsManager.DisableSettings();
            }
            else
            {
                BetterFog.mls.LogWarning("Settings are already closed.");
            }
        }
    }
}