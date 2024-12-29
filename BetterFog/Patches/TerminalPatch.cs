using BetterFog.Input;
using HarmonyLib;
using System.Reflection;
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
            BetterFog.inTerminal = true;
            IngameKeybinds.DisableHotkeys();
            //BetterFog.mls.LogInfo("Terminal opened. Disabling hotkeys.");
        }

        [HarmonyPatch("QuitTerminal")]
        [HarmonyPostfix]
        public static void QuitTerminalPatch()
        {
            BetterFog.inTerminal = false;
            IngameKeybinds.EnableHotkeys();
            BetterFog.mls.LogInfo("Terminal closed. Enabling hotkeys.");
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPatch(Terminal __instance)
        {
            // Your logic after the original Start() runs
            BetterFog.mls.LogInfo("Terminal Start() has finished!");
            SelectableLevel currentLevel = BetterFog.currentLevelType;
            string weather = ((currentLevel.currentWeather == LevelWeatherType.None) ? "none" : ("" + currentLevel.currentWeather).ToLower());
            BetterFog.currentWeatherType = weather;
            if (BetterFog.verboseLoggingEnabled)
                BetterFog.mls.LogInfo($"Weather pulled from save file: Current level: {currentLevel.PlanetName} | Weather: {weather}");

            if (BetterFog.autoPresetModeEnabled)
                BetterFog.ApplyFogSettings(true);
            else
                BetterFog.ApplyFogSettings(false);

            BetterFog.weatherSaveLoaded = true; // Set to true after loading the weather from the save file. Allow StartOfRoundPatch to change weather now.

            IngameKeybinds.EnableHotkeys();
        }
    }
}
