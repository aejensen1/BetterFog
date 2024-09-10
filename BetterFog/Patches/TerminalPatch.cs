using BetterFog.Input;
using HarmonyLib;
using UnityEngine;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    public class TerminalPatch
    {
        [HarmonyPatch("BeginUsingTerminal")]
        [HarmonyPostfix]
        public static void BeginUsingTerminalPatch()
        {
            IngameKeybinds.DisableHotkeys();
            //BetterFog.mls.LogInfo("Terminal opened. Disabling hotkeys.");
        }

        [HarmonyPatch("QuitTerminal")]
        [HarmonyPostfix]
        public static void QuitTerminalPatch()
        {
            IngameKeybinds.EnableHotkeys();
            //BetterFog.mls.LogInfo("Terminal closed. Enabling hotkeys.");
        }
    }
}
