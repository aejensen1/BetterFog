using System;
using HarmonyLib;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using BepInEx.Configuration;
using BetterFog.Patches;
using BetterFog.Assets;
using System.Collections.Generic;
using BetterFog.Input;
using UnityEngine.Rendering.HighDefinition;
using System.Linq;
using System.Collections;
using GameNetcodeStuff;
using Unity.Netcode;
using static IngamePlayerSettings;
using UnityEngine.PlayerLoop;

namespace BetterFog
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class BetterFog : BaseUnityPlugin
    {
        public const string modGUID = "ironthumb.BetterFog";
        public const string modName = "BetterFog";
        public const string modVersion = "3.2.7";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource mls;
        private static BetterFog instance;

        public static bool hotkeysEnabled = true;
        private static ConfigEntry<string> nextPresetHotkeyConfig;
        public static ConfigEntry<bool> nextPresetHotKeyEnabled;
        private static ConfigEntry<string> nextModeHotkeyConfig;
        public static ConfigEntry<bool> nextModeHotKeyEnabled;
        private static ConfigEntry<string> refreshPresetHotkeyConfig;
        public static ConfigEntry<bool> refreshHotKeyEnabled;
        public static ConfigEntry<string> weatherScaleHotkeyConfig;
        public static ConfigEntry<bool> weatherScaleHotKeyEnabled;

        private static ConfigEntry<bool> applyToFogExclusionZone;
        //private static ConfigEntry<bool> noFogEnabled;
        public static ConfigEntry<bool> guiEnabled;

        private static ConfigEntry<bool> weatherScaleEnabled;
        public static bool isDensityScaleEnabled;
        public static string currentWeatherType = "None";
        private static float currentDensityScale = 1f;
        public static List<WeatherScale> WeatherScales;
        private static ConfigEntry<string> weatherScalesConfig;
        public static string currentLevel = "";
        public static List<MoonScale> MoonScales;
        private static ConfigEntry<string> moonScalesConfig;

        private static ConfigEntry<string> defaultPresetName;
        public static List<FogConfigPreset> FogConfigPresets;
        private ConfigEntry<string>[] presetEntries;
        public static int currentPresetIndex;
        public static FogConfigPreset currentPreset;

        private static ConfigEntry<string> defaultMode;
        public static List<BetterFogMode> FogModes;
        //private ConfigEntry<string>[] presetEntries;
        public static int currentModeIndex;
        public static BetterFogMode currentMode;

        public static bool applyingFogSettings = false;

        // Singleton pattern
        public static BetterFog Instance
        {
            get
            {
                if (instance == null)
                {
                    // Find existing instances
                    instance = FindObjectOfType<BetterFog>();

                    if (instance == null)
                    {
                        // Create a new instance if none found
                        var gameObject = new GameObject("BetterFog");
                        DontDestroyOnLoad(gameObject);
                        instance = gameObject.AddComponent<BetterFog>();
                    }
                }
                return instance;
            }
        }

        public void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this.gameObject);

            mls = base.Logger;

            // Initialize your FogConfigPresets list
            // Arguments: Preset Name, MeanFreePath, AlbedoR, AlbedoG, AlbedoB, AlbedoA
            FogConfigPresets = new List<FogConfigPreset>
            {
                new FogConfigPreset("Default", 250f, 0.441f, 0.459f, 0.500f),
                new FogConfigPreset("Heavy Fog", 50f, 0f, 0f, 0f),
                new FogConfigPreset("Light Fog", 9850f, 5f, 5f, 5f),
                new FogConfigPreset("Mist", 10000000f, 1f, 1f, 1f),
                new FogConfigPreset("Red Fog", 500f, 20f, 0f, 0f),
                new FogConfigPreset("Orange Fog", 500f, 20f, 9.33f, 4.5f),
                new FogConfigPreset("Yellow Fog", 1300f, 20f, 20f, 0f),
                new FogConfigPreset("Green Fog", 1300f, 0f, 20f, 0f),
                new FogConfigPreset("Blue Fog", 1300f, 0f, 0f, 20f),
                new FogConfigPreset("Purple Fog", 500f, 11.5f, 7.2f, 20f),
                new FogConfigPreset("Pink Fog", 500f, 20f, 4.75f, 20f),
            };
            mls.LogInfo("FogConfigPresets initialized.");

            FogModes = new List<BetterFogMode>
            {
                new BetterFogMode("Default"),
                new BetterFogMode("No Fog")
            };
            mls.LogInfo("FogModes initialized");

            // Config bindings below
            // Bind each preset to the config
            string section1 = "Default Fog Settings";
            defaultPresetName =
                Config.Bind(section1, "Default Preset Name", "Default", "Name of the default fog preset (No value sets default to first in list).\n" +
                "Order of settings: Preset Name, Mean Free Path, Albedo Red, Albedo Green, Albedo Blue\n" +
                "Mean Free Path - Density of fog. The greater the number, the less dense. 0 is the minimum (opaque fog).\n");
            defaultMode = Config.Bind(section1, "Default Fog Mode", "Default", "Name of the default fog mode. Options: Default, No Fog");

            string section2 = "Key Bindings";
            nextPresetHotkeyConfig = Config.Bind(section2, "Next Preset Hotkey", "n", "Hotkey to switch to the next fog preset.");
            nextPresetHotKeyEnabled = Config.Bind(section2, "Enable Next Hotkey", true, "Enable or disable hotkeys for switching fog presets.");
            nextModeHotkeyConfig = Config.Bind(section2, "Next Mode Hotkey", "m", "Hotkey to switch to the next fog mode.");
            nextModeHotKeyEnabled = Config.Bind(section2, "Enable Next Mode Hotkey", false, "Enable or disable hotkey for switching fog modes.");
            refreshPresetHotkeyConfig = Config.Bind(section2, "Refresh Hotkey", "r", "Hotkey to refresh fog settings.");
            refreshHotKeyEnabled = Config.Bind(section2, "Enable Refresh Hotkey", true, "Enable or disable hotkey for refreshing fog settings.");
            weatherScaleHotkeyConfig = Config.Bind(section2, "Weather Scale Hotkey", "c", "Hotkey to toggle weather scaling.");
            weatherScaleHotKeyEnabled = Config.Bind(section2, "Enable Weather Scale Hotkey", false, "Enable or disable hotkey for Weather Scaling toggle.");

            string section3 = "Fog Settings";
            applyToFogExclusionZone = Config.Bind(section3, "Apply to Fog Exclusion Zone", false, "Apply fog settings to the Fog Exclusion Zone (eg. inside of ship).");
            //noFogEnabled = Config.Bind(section3, "No Fog Enabled Default", false, "Set value to true to enable No Fog by default.");
            weatherScaleEnabled = Config.Bind(section3, "Weather Scale Enabled Default", true, "Enable density scaling for moons and weather by default when booting game.");
            guiEnabled = Config.Bind(section3, "GUI Enabled", true, "Enable or disable the GUI for the mod.");

            // Initialize the key bindings with the hotkey value
            IngameKeybinds.Instance.InitializeKeybindings(nextPresetHotkeyConfig.Value, nextModeHotkeyConfig.Value, refreshPresetHotkeyConfig.Value, weatherScaleHotkeyConfig.Value);

            // Create config entries for each preset
            presetEntries = new ConfigEntry<string>[FogConfigPresets.Count];

            for (int i = 0; i < FogConfigPresets.Count; i++)
            {
                var preset = FogConfigPresets[i];
                presetEntries[i] = Config.Bind("Fog Presets", "Preset " + i, preset.ToString(), $"Preset {preset.PresetName}");

                // Split the entry by commas to get each key-value pair
                string[] presetData = presetEntries[i].Value.Split(',');

                // Iterate through the key-value pairs and parse them
                foreach (string data in presetData)
                {
                    string[] keyValue = data.Split('=');

                    if (keyValue.Length != 2)
                        continue; // Ignore invalid entries

                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();

                    // Assign values based on the key
                    switch (key)
                    {
                        case "PresetName":
                            FogConfigPresets[i].PresetName = value;
                            break;
                        case "Density":
                            FogConfigPresets[i].MeanFreePath = float.Parse(value);
                            break;
                        case "Red Hue":
                            FogConfigPresets[i].AlbedoR = float.Parse(value);
                            break;
                        case "Green Hue":
                            FogConfigPresets[i].AlbedoG = float.Parse(value);
                            break;
                        case "Blue Hue":
                            FogConfigPresets[i].AlbedoB = float.Parse(value);
                            break;
                    }
                }
            }

            string section4 = "Weather and Moon Density Scales";
            moonScalesConfig = Config.Bind(section4, "MoonScales", "71 Gordion=1,41 Experimentation=0.95,220 Assurance=0.9,56 Vow=0.8,21 Offense=0.9," +
                "61 March=0.75,20 Adamance=0.75,85 Rend=0.285,7 Dine=0.325,8 Titan=0.285,68 Artifice=0.9,5 Embrion=0.85,44 Liquidation=0.85",
                "Moon scales in the format {Gordion=1,41 Experimentation=0.95,220 Assurance=0.9,...} Moon Scales are applied before weather fog density scales.");

            MoonScales = ParseMoonScales(moonScalesConfig.Value);

            weatherScalesConfig = Config.Bind(section4, "WeatherScales", "none=1,rainy=0.75,stormy=0.5,foggy=0.45,eclipsed=0.77,dust clouds=0.8,flooded=0.765",
            "Weather scales in the format {none=1,rainy=0.69,stormy=0.65,...} Weather Scales are applied after moon fog density scales.");

            WeatherScales = ParseWeatherScales(weatherScalesConfig.Value);

            mls.LogInfo("Finished parsing config entries");

            //--------------------------------- End Config Parsing ---------------------------------

            if (defaultPresetName == null) // If no default preset is set, use the first preset in the list
            {
                currentPreset = FogConfigPresets[0];
                currentPresetIndex = 0;
                mls.LogInfo($"Default preset not found. Using the first preset in the list: {currentPreset.PresetName}");
            }
            else // Otherwise, find the preset with the default name
            {
                try
                {
                    // Attempt to find the preset with the default name
                    //foreach(var preset in FogConfigPresets)
                    //{
                    //    mls.LogInfo(preset.ToString());
                    //}
                    currentPreset = FogConfigPresets.Find(preset => preset.PresetName == defaultPresetName.Value);
                    currentPresetIndex = FogConfigPresets.IndexOf(currentPreset);
                    mls.LogInfo($"Default preset found: {currentPreset.PresetName}");
                }
                catch (Exception ex)
                { // If the preset is not found, log an error and use the first preset in the list
                    mls.LogError($"Failed to find the default preset: {ex}");
                    currentPreset = FogConfigPresets[0];
                    currentPresetIndex = 0;
                }
            }

            // Do the same for default mode initialization
            if (defaultMode == null) // If no default preset is set, use the first preset in the list
            {
                currentMode = FogModes[0];
                currentModeIndex = 0;
                //mls.LogInfo($"Default preset not found. Using the first preset in the list: {currentPreset.PresetName}");
            }
            else // Otherwise, find the preset with the default name
            {
                try
                {
                    // Attempt to find the preset with the default name
                    currentMode = FogModes.Find(mode => mode.Name == defaultMode.Value);
                    currentModeIndex = FogModes.IndexOf(currentMode);
                    mls.LogInfo($"Default mode found: {currentMode.Name}");

                    UpdateMode();
                }
                catch (Exception ex)
                { // If the preset is not found, log an error and use the first preset in the list
                    mls.LogError($"Failed to find the default mode: {ex}");
                    currentMode = FogModes[0];
                    currentModeIndex = 0;
                }
            }
            isDensityScaleEnabled = weatherScaleEnabled.Value;

            // Apply the Harmony patches
            try
            {
                //harmony.Patch(original: AccessTools.Method(typeof(StartOfRound), "StartGame"), postfix: new HarmonyMethod(typeof(StartOfRoundPatch), "StartGamePatch"));
                harmony.Patch(original: AccessTools.Method(typeof(StartOfRound), "ChangeLevel"), postfix: new HarmonyMethod(typeof(StartOfRoundPatch), "ChangeLevelPatch"));
                mls.LogInfo("StartOfRound patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(QuickMenuManager), "OpenQuickMenu"), postfix: new HarmonyMethod(typeof(QuickMenuManagerPatch), "OpenQuickMenuPatch"));
                harmony.Patch(original: AccessTools.Method(typeof(QuickMenuManager), "CloseQuickMenu"), postfix: new HarmonyMethod(typeof(QuickMenuManagerPatch), "CloseQuickMenuPatch"));
                mls.LogInfo("QuickMenuManager patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(IngamePlayerSettings), "RefreshAndDisplayCurrentMicrophone"), postfix: new HarmonyMethod(typeof(IngamePlayerSettingsPatch), "RefreshAndDisplayCurrentMicrophonePatch"));
                harmony.Patch(original: AccessTools.Method(typeof(IngamePlayerSettings), "DiscardChangedSettings"), postfix: new HarmonyMethod(typeof(IngamePlayerSettingsPatch), "DiscardChangedSettingsPatch"));
                mls.LogInfo("IngamePlayerSettings patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(EntranceTeleport), "TeleportPlayer"), postfix: new HarmonyMethod(typeof(EntranceTeleportPatch), "TeleportPlayerPatch"));
                mls.LogInfo("EntranceTeleport patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(Terminal), "BeginUsingTerminal"), postfix: new HarmonyMethod(typeof(TerminalPatch), "BeginUsingTerminalPatch"));
                harmony.Patch(original: AccessTools.Method(typeof(Terminal), "QuitTerminal"), postfix: new HarmonyMethod(typeof(TerminalPatch), "QuitTerminalPatch"));
                mls.LogInfo("Terminal patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(AudioReverbTrigger), "changeVolume"), prefix: new HarmonyMethod(typeof(AudioReverbTriggerPatch), "changeVolumePrefix"));
                mls.LogInfo("AudioReverb patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(PlayerControllerB), "TeleportPlayer"), postfix: new HarmonyMethod(typeof(PlayerControllerBPatch), "TeleportPlayerPatch"));
                harmony.Patch(original: AccessTools.Method(typeof(PlayerControllerB), "SpectateNextPlayer"), postfix: new HarmonyMethod(typeof(PlayerControllerBPatch), "SpectateNextPlayerPatch"));
                mls.LogInfo("PlayerControllerB patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(NetworkSceneManager), "OnSceneLoaded"), postfix: new HarmonyMethod(typeof(NetworkSceneManagerPatch), "OnSceneLoadedPatch"));
                mls.LogInfo("NetworkSceneManager patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(MenuManager), "PlayCancelSFX"), postfix: new HarmonyMethod(typeof(MenuManagerPatch), "PlayCancelSFXPatch"));
                mls.LogInfo("MenuManager patches applied successfully.");
            }
            catch (Exception ex)
            {
                mls.LogError($"Failed to apply Harmony patches: {ex}");
                throw; // Rethrow the exception to indicate initialization failure
            }

            // Check if the FogSettingsManager instance is valid
            if (FogSettingsManager.Instance != null)
            {
                mls.LogInfo(FogSettingsManager.Instance.ToString());
            }
            else
            {
                mls.LogError("FogSettingsManager instance is null.");
            }
        }

        public static void ApplyFogSettings()
        {
            // Find all LocalVolumetricFog objects
            var fogObjects = Resources
                .FindObjectsOfTypeAll<LocalVolumetricFog>()
                .ToList();
            // Add for name filter if desired:
            //.Where(fog => fog.name == "Foggy")
            //.ToList()
            //.FirstOrDefault();

            // Iterate through each fog object
            foreach (var fogObject in fogObjects)
            {
                // Apply settings if the fog object is not null and not the FogExclusionZone object (if applyToFogExclusionZone is false)
                if (fogObject != null && !(fogObject.name == "FogExclusionZone" && applyToFogExclusionZone.Value == false))
                {
                    // Print details of the Fog object
                    //mls.LogInfo($"Found LocalVolumetricFog object: {fogObject.name}");
                    // You can also print other properties of the Fog object if needed
                    //mls.LogInfo($"Fog Object Details: {fogObject.ToString()}");
                    // Apply the current preset settings
                    var parameters = fogObject.parameters;

                    // Example modifications (ensure these properties exist and are accessible)
                    if (isDensityScaleEnabled)
                    {
                        // Handle Moon Scaling
                        currentDensityScale = 1;
                        foreach (MoonScale moonScale in MoonScales)
                        {
                            //mls.LogInfo(weatherScale.WeatherName);
                            if (currentLevel == moonScale.MoonName)
                            {
                                currentDensityScale = moonScale.Scale;
                                mls.LogInfo($"{currentLevel} moon detected. Set density scale to " + currentDensityScale);
                                break;
                            }
                            if (moonScale.MoonName == MoonScales[MoonScales.Count - 1].MoonName)
                            {
                                mls.LogWarning($"{currentLevel} moon not found in records. Using scale of {currentDensityScale}.");
                            }
                        }

                        // Handle Weather Scaling
                        foreach (WeatherScale weatherScale in WeatherScales)
                        {
                            //mls.LogInfo(weatherScale.WeatherName);
                            if (currentWeatherType == weatherScale.WeatherName)
                            {
                                currentDensityScale = currentDensityScale * weatherScale.Scale;
                                mls.LogInfo($"{currentWeatherType} weather type detected. Set density scale to " + currentDensityScale);
                                break;
                            }
                            if (weatherScale.WeatherName == WeatherScales[WeatherScales.Count - 1].WeatherName)
                            {
                                mls.LogWarning($"{currentWeatherType} weather type not found in records. Using scale of {currentDensityScale}.");
                            }
                        }

                        // Set new density with scaling applied
                        parameters.meanFreePath = currentPreset.MeanFreePath * currentDensityScale;
                    }
                    else
                    {
                        mls.LogInfo("Weather scaling is disabled. Using a scale of 1.");
                        parameters.meanFreePath = currentPreset.MeanFreePath;
                    }
                    parameters.albedo = new Color(
                        currentPreset.AlbedoR,
                        currentPreset.AlbedoG,
                        currentPreset.AlbedoB,
                        1f
                    );

                    // Optionally, apply changes if the parameters object needs to be reassigned
                    fogObject.parameters = parameters;
                }             
                else
                {
                    //mls.LogError("Found a null LocalVolumetricFog object.");
                }
            }
            // Log the applied settings
            //mls.LogInfo($"New fog settings applied from preset {currentPreset.PresetName}:\n" +
            //    $"Mean Free Path: {currentPreset.MeanFreePath}\n" +
            //    $"Albedo RGBA: ({currentPreset.AlbedoR}, {currentPreset.AlbedoG}, {currentPreset.AlbedoB}, {currentPreset.AlbedoA})\n");
        }

        public static void ApplyFogSettingsOnGameStart() // Duration in seconds, interval in seconds
        {
            applyingFogSettings = true;

            // Check if the game has already started before starting the coroutine
            if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.gameHasStarted)
            {
                mls.LogInfo("Game has already started. Applying fog settings immediately.");
                Instance.StartCoroutine(ApplyFogSettingsRepeated(8)); // Apply settings 8 times
                applyingFogSettings = false;
                return;
            }

            // Start the coroutine to wait for game start and then apply fog settings
            Instance.StartCoroutine(WaitForGameStartAndApplyFog());
        }

        private static IEnumerator WaitForGameStartAndApplyFog()
        {
            int waitTime = 0;

            // Wait until the game starts or timeout after 100 seconds
            while (GameNetworkManager.Instance != null && !GameNetworkManager.Instance.gameHasStarted && waitTime < 60)
            {
                mls.LogInfo($"Waiting for game to start... ({waitTime}s)");

                // Wait for 1 second
                yield return new WaitForSecondsRealtime(1);
                waitTime++;
            }

            if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.gameHasStarted)
            {
                mls.LogWarning("Game has started. Applying fog settings.");
                Instance.StartCoroutine(ApplyFogSettingsRepeated(8)); // Apply settings 8 times
            }
            else
            {
                mls.LogInfo("Timeout reached, game has not started yet. Fog settings not applied.");
            }

            applyingFogSettings = false;
        }

        private static IEnumerator ApplyFogSettingsRepeated(int repeatCount)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                mls.LogInfo($"Applying fog settings... ({i + 1}/{repeatCount})");

                ApplyFogSettings(); // Apply fog settings

                // Wait for 1 second between applications
                yield return new WaitForSecondsRealtime(1);
            }
        }

        public static void NextPreset()
        {
            mls.LogInfo("Next preset hotkey pressed.");
            mls.LogInfo(FogSettingsManager.Instance.ToString());
            currentPresetIndex = FogConfigPresets.IndexOf(currentPreset);
            if (currentPresetIndex == FogConfigPresets.Count - 1)
            {
                currentPresetIndex = -1; // Reset to the first preset if the last preset is reached
            }
            currentPresetIndex ++;
            currentPreset = FogConfigPresets[currentPresetIndex];
            mls.LogInfo("Current preset index: " + currentPresetIndex);
            mls.LogInfo($"Current preset: {currentPreset.PresetName}");
            ApplyFogSettings();

            // Notify FogSettingsManager to update dropdown
            FogSettingsManager.Instance.UpdateSettings();
        }

        public static void NextMode()
        {
            mls.LogInfo("Next mode hotkey pressed.");
            currentModeIndex = FogModes.IndexOf(currentMode);
            if (currentModeIndex == FogModes.Count - 1)
            {
                currentModeIndex = -1; // Reset to the first preset if the last preset is reached
            }
            currentModeIndex++;
            currentMode = FogModes[currentModeIndex];
            mls.LogInfo("Current mode index: " + currentModeIndex);
            mls.LogInfo($"Current preset: {currentMode.Name}");
            Instance.UpdateMode();

            // Notify FogSettingsManager to update dropdown
            FogSettingsManager.Instance.UpdateSettings();
        }

        private void UpdateMode()
        {
            if (currentMode.Name == "No Fog")
            {
                mls.LogInfo("No Fog mode selected.");
                EnableFogPatch();
            }
            else
            {
                mls.LogInfo("Default mode selected.");
                DisableFogPatch();
            }
        }

        // Function to parse the weather scales
        private List<WeatherScale> ParseWeatherScales(string configString)
        {
            var weatherScales = new List<WeatherScale>();
            var pairs = configString.Split(',');

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2 && float.TryParse(keyValue[1], out float scaleValue))
                {
                    weatherScales.Add(new WeatherScale(keyValue[0], scaleValue));
                }
            }
            return weatherScales;
        }

        private List<MoonScale> ParseMoonScales(string configString)
        {
            var moonScales = new List<MoonScale>();
            var pairs = configString.Split(',');

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2 && float.TryParse(keyValue[1], out float scaleValue))
                {
                    moonScales.Add(new MoonScale(keyValue[0], scaleValue));
                }
            }
            return moonScales;
        }

        //--------------------------------- Start No Fog Management ---------------------------------

        public void EnableFogPatch()
        {
            harmony.Patch(
            original: AccessTools.Method(typeof(Fog), "UpdateShaderVariablesGlobalCBFogParameters"),
            prefix: new HarmonyMethod(typeof(FogPatch), "Prefix")
        );
            mls.LogInfo("Fog patches enabled successfully.");
        }

        public void DisableFogPatch()
        {
            var method = AccessTools.Method(typeof(Fog), "UpdateShaderVariablesGlobalCBFogParameters");
            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            mls.LogInfo("Fog patches disabled successfully.");
        }
        //--------------------------------- End No Fog Management ---------------------------------

        public void WaitToApplySettings(float seconds)
        {
            StartCoroutine(DelayAndApplySettings(seconds));
        }

        IEnumerator DelayAndApplySettings(float seconds)
        {
            // Log that the delay has started
            mls.LogInfo($"Waiting for {seconds} seconds...");

            // Wait for the specified number of seconds
            yield return new WaitForSeconds(seconds);

            // Log after the wait is complete
            mls.LogInfo($"Waited {seconds} seconds.");

            // Check if the game has started and apply fog settings accordingly
            if (GameNetworkManager.Instance.gameHasStarted)
            {
                mls.LogInfo("Game has started. Applying fog settings to moon.");
                ApplyFogSettings();
            }
            else
            {
                mls.LogInfo("Game has not started yet. Refusing to apply fog settings.");
            }
        }

    }
    //--------------------------------- End Class ---------------------------------
}
//--------------------------------- End File ---------------------------------
