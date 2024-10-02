using BetterFog.Input;
using HarmonyLib;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    public class HUDManagerPatch // Managing the hotkeys when the player opens or closes the chat
    {
        [HarmonyPatch("EnableChat_performed")]
        [HarmonyPostfix]
        public static void EnableChat_performedPatch()
        {
            IngameKeybinds.DisableHotkeys();
            //BetterFog.mls.LogInfo("Chat enabled. Disabling hotkeys.");
        }

        [HarmonyPatch("SubmitChat_performed")]
        [HarmonyPostfix]
        public static void SubmitChat_performedPatch()
        {
            IngameKeybinds.EnableHotkeys();
            //BetterFog.mls.LogInfo("Chat submitted. Enabling hotkeys.");
        }

        [HarmonyPatch("OpenMenu_performed")]
        [HarmonyPostfix]
        public static void OpenMenu_performedPatch()
        {
            IngameKeybinds.EnableHotkeys();
            //BetterFog.mls.LogInfo("Menu Opened. Enabling hotkeys.");
        }
    }
}
