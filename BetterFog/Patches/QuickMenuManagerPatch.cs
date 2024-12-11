using BetterFog.Input;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(QuickMenuManager))]
    public class QuickMenuManagerPatch // For when the player opens the quick menu in game (ESC)
    {
        [HarmonyPatch("OpenQuickMenu")]
        [HarmonyPostfix]
        public static void OpenQuickMenuPatch()
        {
            if (BetterFog.verboseLoggingEnabled)
                BetterFog.mls.LogInfo("Quick menu opened. Disabling hotkeys");

            IngameKeybinds.DisableHotkeys(); // Disable hotkeys when the quick menu is open
        }
        [HarmonyPatch("CloseQuickMenu")]
        [HarmonyPostfix]
        public static void CloseQuickMenuPatch()
        {
            if (BetterFog.verboseLoggingEnabled)
                BetterFog.mls.LogInfo("Quick menu closed. Enabling hotkeys");

            IngameKeybinds.EnableHotkeys(); // Enable hotkeys when the quick menu is closed
        }
    }
}
