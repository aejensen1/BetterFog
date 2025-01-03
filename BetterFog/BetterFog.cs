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
using UnityEngine.SceneManagement;
using System.Globalization;
using System.Security.Cryptography;

namespace BetterFog
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class BetterFog : BaseUnityPlugin
    {
        public const string modGUID = "ironthumb.BetterFog";
        public const string modName = "BetterFog";
        public const string modVersion = "3.3.8";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource mls;
        private static BetterFog instance;

        // Hotkey variables
        public static bool hotkeysEnabled = true;
        public static ConfigEntry<string> nextPresetHotkeyConfig;
        public static ConfigEntry<string> nextModeHotkeyConfig;
        public static ConfigEntry<string> refreshPresetHotkeyConfig;
        public static ConfigEntry<string> weatherScaleHotkeyConfig;
        public static ConfigEntry<string> settingsHotkeyConfig;
        public static ConfigEntry<string> autoPresetModeHotkeyConfig;

        // Fog settings variables
        public static ConfigEntry<bool> excludeShipFogDefault;
        public static bool excludeShipFogEnabled;
        public static ConfigEntry<bool> verboseLoggingDefault;
        public static bool verboseLoggingEnabled;
        public static ConfigEntry<bool> excludeEnemyFogDefault; // Exclude enemy fog when applying better fog settings
        public static bool excludeEnemyFogEnabled;

        public static bool fogRefreshLock = true; // set true to prevent fog settings from being applied

        // Weather and moon scaling variables
        private static List<string> weatherScaleBlacklist = new List<string> { };
        private static List<string> moonScaleBlacklist = new List<string> { };
        private static ConfigEntry<string> weatherScaleBlacklistConfig;
        private static ConfigEntry<string> moonScaleBlacklistConfig;
        private static ConfigEntry<bool> densityScaleEnabledDefault;
        public static bool densityScaleEnabled;
        public static string currentWeatherType = "none";
        private static float moonDensityScale = 1f;
        private static float weatherDensityScale = 1f;
        public static float combinedDensityScale = 1f;
        public const float maxDensitySliderValue = 1000; // Sets the max value possible for the density slider scale (inversed mean free path)
        public const float maxColorValue = 3f; // Sets the max value possible for the color sliders
        public static List<WeatherScale> weatherScales;
        private static ConfigEntry<string> weatherScalesConfig;
        public static bool weatherSaveLoaded = false;
        public static SelectableLevel currentLevelType;
        public static string currentLevel = "";
        public static List<MoonScale> moonScales;
        private static ConfigEntry<string> moonScalesConfig;

        // Auto Preset/Mode Settings
        private static List<AutoPresetMode> autoPresetModes;
        private static ConfigEntry<string> autoPresetModeConfig;
        private static ConfigEntry<bool> autoPresetModeEnabledDefault;
        public static bool autoPresetModeEnabled;
        public static bool autoPresetModeMatchFound = false;
        public static AutoPresetMode matchedPreset = null;

        // Fog preset variables
        private static ConfigEntry<string> defaultPresetName;
        public static List<FogConfigPreset> defaultFogConfigPresets;
        public static List<FogConfigPreset> fogConfigPresets;
        private ConfigEntry<string>[] presetEntries;
        public static int currentPresetIndex;
        public static FogConfigPreset currentPreset;
        public static bool lockPresetDropdownModification = false;
        public static bool lockPresetValueModification = false;

        public static Dictionary<GameObject, LocalVolumetricFogArtistParameters> fogParameterChanges = new Dictionary<GameObject, LocalVolumetricFogArtistParameters>(); // Dictionary to store vanilla fog settings

        private static ConfigEntry<string> defaultMode;
        public static List<BetterFogMode> fogModes;
        public static int currentModeIndex;
        public static BetterFogMode currentMode;
        public static bool lockModeDropdownModification = false;

        public static bool isFogSettingsActive = false;
        public static bool settingsHotkeyEnabled;
        public static bool applyingFogSettings = false;
        public static bool inTerminal = false;
        

        public static PlayerControllerB player;

        private static readonly object lockObject = new object();

        // Singleton pattern
        public static BetterFog Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<BetterFog>();
                        if (instance == null)
                        {
                            var gameObject = new GameObject("BetterFog");
                            DontDestroyOnLoad(gameObject);
                            instance = gameObject.AddComponent<BetterFog>();
                        }
                    }
                    return instance;
                }
            }
        }

        public void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            mls = base.Logger;
            if (mls == null)
            {
                Debug.LogWarning("Logger not initialized in BetterFog.");
            }

            // Override existing Unity or BepInEx locale configurations if applicable
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            // Initialize your fogConfigPresets list
            // Arguments: Preset Name, MeanFreePath, AlbedoR, AlbedoG, AlbedoB, AlbedoA
            fogConfigPresets = new List<FogConfigPreset>
            {
                new FogConfigPreset("Default", 735f, 0.19f, 0.28f, 0.37f),
                new FogConfigPreset("Heavy Fog", 870f, 0.19f, 0.28f, 0.37f),
                new FogConfigPreset("Light Fog", 620f, 0.19f, 0.28f, 0.37f),
                new FogConfigPreset("Mist", 0f, 1f, 1f, 1f),
                new FogConfigPreset("Red Fog", 550f, 0.5f, 0f, 0f),
                new FogConfigPreset("Thick Red", 735f, 0.5f, 0f, 0f),
                new FogConfigPreset("Orange Fog", 350f, 1f, 0.5f, 0f),
                new FogConfigPreset("Thick Orange", 735f, 1f, 0.5f, 0f),
                new FogConfigPreset("Yellow Fog", 250f, 1f, 0.75f, 0f),
                new FogConfigPreset("Thick Yellow", 735f, 1f, 0.75f, 0f),
                new FogConfigPreset("Green Fog", 450f, 0f, 0.5f, 0f),
                new FogConfigPreset("Thick Green", 735f, 0f, 0.5f, 0f),
                new FogConfigPreset("Blue Fog", 550f, 0f, 0f, 1f),
                new FogConfigPreset("Blue Fog", 735f, 0f, 0f, 1f),
                new FogConfigPreset("Purple Fog", 450f, 0.25f, 0f, 1f),
                new FogConfigPreset("Thick Purple", 735f, 0.25f, 0f, 1f),
                new FogConfigPreset("Pink Fog", 300f, 1, 0f, 0.5f),
                new FogConfigPreset("Thick Pink", 735f, 1, 0f, 0.5f),
                new FogConfigPreset("White Fog", 550, 1f, 1f, 1f),
                new FogConfigPreset("Thick White", 735f, 1f, 1f, 1f),
            };

            // Do a deep copy of the fogConfigPresets list to prevent reference issues
            defaultFogConfigPresets = fogConfigPresets.Select(preset => new FogConfigPreset(preset)).ToList();
            mls.LogInfo("fogConfigPresets initialized.");


            fogModes = new List<BetterFogMode>
            {
                new BetterFogMode("Better Fog"),
                new BetterFogMode("No Fog"),
                new BetterFogMode("Vanilla") // Vanilla has three tiers of recording values: 1. LoadScene 2. ChangeLevel 3. OnEnable
            };
            mls.LogInfo("fogModes initialized");

            // Config bindings below
            // Bind each preset to the config
            string section1 = "Default Fog Settings";
            defaultPresetName =
                Config.Bind(section1, "Default Preset Name", "Default", "Name of the default fog preset (No value sets default to first in list).\n" +
                "Order of settings: Preset Name, Mean Free Path, Albedo Red, Albedo Green, Albedo Blue\n" +
                "Density of fog. The greater the number, the more dense. Values range 0-1000.\n");
            defaultMode = Config.Bind(section1, "Default Fog Mode", "Better Fog", "Name of the default fog mode. Options: Better Fog, No Fog, Vanilla");
            
            string section2 = "Key Bindings";
            settingsHotkeyConfig = Config.Bind(section2, "Settings Hotkey", "f1", "Hotkey to open the BetterFog settings menu.");
            nextPresetHotkeyConfig = Config.Bind(section2, "Next Preset Hotkey", "n", "Hotkey to switch to the next fog preset.");
            nextModeHotkeyConfig = Config.Bind(section2, "Next Mode Hotkey", "m", "Hotkey to switch to the next fog mode.");
            refreshPresetHotkeyConfig = Config.Bind(section2, "Refresh Hotkey", "r", "Hotkey to refresh fog settings.");
            weatherScaleHotkeyConfig = Config.Bind(section2, "Weather Scale Hotkey", "c", "Hotkey to toggle weather scaling.");
            autoPresetModeHotkeyConfig = Config.Bind(section2, "Auto Sync Hotkey", "j", "Hotkey to toggle auto sync.");

            string section3 = "Fog Settings";
            excludeShipFogDefault = Config.Bind(section3, "Exclude Ship", true, "Enable or Disable fog settings to the Fog Exclusion Zone (eg. inside of ship).");
            excludeShipFogEnabled = excludeShipFogDefault.Value;
            densityScaleEnabledDefault = Config.Bind(section3, "Weather Scale Enabled Default", true, "Enable density scaling for moons and weather by default when booting game.");
            densityScaleEnabled = densityScaleEnabledDefault.Value;
            autoPresetModeEnabledDefault = Config.Bind(section3, "Auto Preset/Mode Enabled", false, "Enable or disable auto preset/mode settings by default.");
            autoPresetModeEnabled = autoPresetModeEnabledDefault.Value;
            verboseLoggingDefault = Config.Bind(section3, "Verbose Logging Enabled", false, "Enable or disable verbose logging for the mod. Can be used for debugging.");
            verboseLoggingEnabled = verboseLoggingDefault.Value;
            excludeEnemyFogDefault = Config.Bind(section3, "Exclude Enemy Fog", true, "Exclude enemy fog when applying better fog settings.");
            excludeEnemyFogEnabled = excludeEnemyFogDefault.Value;

            // Initialize the key bindings with the hotkey value
            IngameKeybinds.Instance.InitializeKeybindings(nextPresetHotkeyConfig.Value, nextModeHotkeyConfig.Value, refreshPresetHotkeyConfig.Value, weatherScaleHotkeyConfig.Value, settingsHotkeyConfig.Value, autoPresetModeHotkeyConfig.Value);
            // Create config entries for each preset
            presetEntries = new ConfigEntry<string>[fogConfigPresets.Count];

            for (int i = 0; i < fogConfigPresets.Count; i++)
            {
                var preset = fogConfigPresets[i];
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
                            fogConfigPresets[i].PresetName = value;
                            break;
                        case "Density":
                            float meanFreePath = float.Parse(value, CultureInfo.InvariantCulture);
                            if (meanFreePath < 0)
                                meanFreePath = 0;
                            else if (meanFreePath > maxDensitySliderValue)
                                meanFreePath = maxDensitySliderValue;
                            fogConfigPresets[i].MeanFreePath = meanFreePath;
                            break;
                        case "Red Hue":
                            float albedoR = float.Parse(value, CultureInfo.InvariantCulture);
                            if (albedoR < 0)
                                albedoR = 0;
                            else if (albedoR > maxColorValue)
                                albedoR = maxColorValue;
                            fogConfigPresets[i].AlbedoR = albedoR;
                            break;
                        case "Green Hue":
                            float albedoG = float.Parse(value, CultureInfo.InvariantCulture);
                            if (albedoG < 0)
                                albedoG = 0;
                            else if (albedoG > maxColorValue)
                                albedoG = maxColorValue;
                            fogConfigPresets[i].AlbedoG = albedoG;
                            break;
                        case "Blue Hue":
                            float albedoB = float.Parse(value, CultureInfo.InvariantCulture);
                            if (albedoB < 0)
                                albedoB = 0;
                            else if (albedoB > maxColorValue)
                                albedoB = maxColorValue;
                            fogConfigPresets[i].AlbedoB = albedoB;
                            break;
                    }
                }
            }

            if (verboseLoggingEnabled)
            {
                foreach (var preset in fogConfigPresets)
                {
                    mls.LogInfo($"Preset '{preset.PresetName}': Density={preset.MeanFreePath}, AlbedoR={preset.AlbedoR}, AlbedoG={preset.AlbedoG}, AlbedoB={preset.AlbedoB}");
                }
            }

            string section4 = "Weather and Moon Density Scales";
            moonScalesConfig = Config.Bind(section4, "MoonScales", "71 Gordion=1,41 Experimentation=0.95,220 Assurance=0.9,56 Vow=1.05,21 Offense=1," +
                "61 March=1.05,20 Adamance=1.05,85 Rend=1.25,7 Dine=1.25,8 Titan=1.165,68 Artifice=1.04,5 Embrion=1.04,44 Liquidation=1,Fallback=1",
                "Moon scales in the format {71 Gordion=1,41 Experimentation=0.998,220 Assurance=1,...}. When \"Fallback={number}\" is in the list, the density will default to \n" +
                "this value if no moon is detected.");

            moonScales = ParseMoonScales(moonScalesConfig.Value);

            weatherScalesConfig = Config.Bind(section4, "WeatherScales", "none=1,rainy=1.02,stormy=1.02,foggy=1.07,eclipsed=1.06,dust clouds=1.02,flooded=1.025,Fallback=1",
            "Weather scales in the format {none=1,rainy=1.01,stormy=1.02,...}. When \"Fallback={number}\" is in the list, the density will default to this value if no moon is detected.");

            weatherScales = ParseWeatherScales(weatherScalesConfig.Value);

            // check whether any combination of moon and weather scales exceed the maxDensitySliderValue for every preset
            foreach (var preset in fogConfigPresets)
            {
                foreach (var moonScale in moonScales)
                {
                    foreach (var weatherScale in weatherScales)
                    {
                        if ((preset.MeanFreePath * moonScale.Scale * weatherScale.Scale) > maxDensitySliderValue)
                        {
                            mls.LogWarning($"Preset '{preset.PresetName}' exceeds max density slider value with Moon '{moonScale.MoonName}' and Weather '{weatherScale.WeatherName}'. Decrease the scales or preset fog density to prevent a black screen...");
                        }
                    }
                }
            }

            weatherScaleBlacklistConfig = Config.Bind(section4, "Weather Scaling Blacklist", "",
                "Enter weather names or moon names to trigger temporary disablement of fog WEATHER density scaling. This is only effective when WeatherScale is enabled. \n" +
                "Full moon or weather names must be typed in, comma separated. Example: {eclipsed,20 Adamance,85 Rend}");
            moonScaleBlacklistConfig = Config.Bind(section4, "Moon Scaling Blacklist", "",
                "Enter weather names or moon names to trigger temporary disablement of MOON fog density scaling. This is only effective when WeatherScale is enabled. \n" +
                "Full moon or weather names must be typed in, comma separated. Example: {eclipsed,20 Adamance,85 Rend}");

            weatherScaleBlacklist = ParseDensityScaleBlacklist(weatherScaleBlacklistConfig.Value);
            moonScaleBlacklist = ParseDensityScaleBlacklist(moonScaleBlacklistConfig.Value);

            string section5 = "Auto Sync Preset/Mode Settings";
            autoPresetModeConfig = Config.Bind(section5, "Auto Sync Preset/Mode Settings", "85 Rend&eclipsed=Thick Red,7 Dine&Eclipsed=Thick Red,8 Titan&eclipsed=Thick Red,eclipsed=Red Fog,All=Default", "Automatically apply presets and modes to moons and weathers. On the left of = enter \n" +
                "a moon and/or weather name, and on the right enter a single preset or mode name. Entering a preset name on the right automatically sets the mode to \"Better Fog\". \n" +
                "To have a condition that requires both a moon and weather, enter \"&\" in between entries. This will override single entries if both moon and weather are present. \n" +
                "If a preset name is the same as a mode name, the mode will be set to \"Better Fog\" and that preset will be set. \n" +
                "Warning: If you create different conditions that conflict (such as none=mist,68 Artifice=No Fog and you land on Art with no weather), the leftmost condition will apply. \n" +
                "For that reason, put double conditions with the most specific condition first, and single condition last. Keyword \"All\" Matches all moons and whethers. Use this last as \n" +
                "a fallback value in case no matches are found. Example: 61 March=Light Fog,7 Dine&eclipsed=Orange Fog,7 Dine=Heavy Fog,eclipsed=Red Fog,8 Titan=Heavy Fog,none=Mist,none&8 Titan=No Fog,All=Default");
            autoPresetModes = ParseAutoPresetMode(autoPresetModeConfig.Value);

            mls.LogInfo("Finished parsing config entries");

            //--------------------------------- End Config Parsing ---------------------------------

            if (defaultPresetName == null) // If no default preset is set, use the first preset in the list
            {
                currentPreset = fogConfigPresets[0];
                currentPresetIndex = 0;
                mls.LogInfo($"Default preset not found. Using the first preset in the list: {currentPreset.PresetName}");
            }
            else // Otherwise, find the preset with the default name
            {
                try
                {
                    currentPreset = fogConfigPresets.Find(preset => preset.PresetName == defaultPresetName.Value);
                    currentPresetIndex = fogConfigPresets.IndexOf(currentPreset);
                    mls.LogInfo($"Default preset found: {currentPreset.PresetName}");
                }
                catch (Exception ex)
                { // If the preset is not found, log an error and use the first preset in the list
                    mls.LogError($"Failed to find the default preset: {ex}");
                    currentPreset = fogConfigPresets[0];
                    currentPresetIndex = 0;
                }
            }

            // Do the same for default mode initialization
            if (defaultMode == null) // If no default preset is set, use the first preset in the list
            {
                currentMode = fogModes[0];
                currentModeIndex = 0;
                //mls.LogInfo($"Default preset not found. Using the first preset in the list: {currentPreset.PresetName}");
            }
            else // Otherwise, find the preset with the default name
            {
                try
                {
                    // Attempt to find the preset with the default name
                    currentMode = fogModes.Find(mode => mode.Name == defaultMode.Value);
                    currentModeIndex = fogModes.IndexOf(currentMode);
                    mls.LogInfo($"Default mode found: {currentMode.Name}");

                    UpdateMode();
                }
                catch (Exception ex)
                { // If the preset is not found, log an error and use the first preset in the list
                    mls.LogError($"Failed to find the default mode: {ex}");
                    currentMode = fogModes[0];
                    currentModeIndex = 0;
                }
            }

            // Apply the Harmony patches
            try
            {
                harmony.Patch(original: AccessTools.Method(typeof(StartOfRound), "ChangeLevel"), postfix: new HarmonyMethod(typeof(StartOfRoundPatch), "ChangeLevelPatch"));
                //mls.LogInfo("StartOfRound patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(Terminal), "BeginUsingTerminal"), postfix: new HarmonyMethod(typeof(TerminalPatch), "BeginUsingTerminalPatch"));
                harmony.Patch(original: AccessTools.Method(typeof(Terminal), "QuitTerminal"), postfix: new HarmonyMethod(typeof(TerminalPatch), "QuitTerminalPatch"));
                harmony.Patch(original: AccessTools.Method(typeof(Terminal), "Start"), postfix: new HarmonyMethod(typeof(TerminalPatch), "StartPatch"));
                //mls.LogInfo("Terminal patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(QuickMenuManager), "OpenQuickMenu"), postfix: new HarmonyMethod(typeof(QuickMenuManagerPatch), "OpenQuickMenuPatch"));
                harmony.Patch(original: AccessTools.Method(typeof(QuickMenuManager), "CloseQuickMenu"), postfix: new HarmonyMethod(typeof(QuickMenuManagerPatch), "CloseQuickMenuPatch"));

                harmony.Patch(original: AccessTools.Method(typeof(HUDManager), "EnableChat_performed"), postfix: new HarmonyMethod(typeof(HUDManagerPatch), "EnableChat_performedPatch"));
                harmony.Patch(original: AccessTools.Method(typeof(HUDManager), "SubmitChat_performed"), postfix: new HarmonyMethod(typeof(HUDManagerPatch), "SubmitChat_performedPatch"));
                harmony.Patch(original: AccessTools.Method(typeof(HUDManager), "OpenMenu_performed"), postfix: new HarmonyMethod(typeof(HUDManagerPatch), "OpenMenu_performedPatch"));
                //mls.LogInfo("HUDManager patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(LocalVolumetricFog), "OnEnable"), postfix: new HarmonyMethod(typeof(LocalVolumetricFogPatch), "OnEnablePatch"));
                //mls.LogInfo("LocalVolumetricFog patches applied successfully.");

                // Specify parameters to avoid ambiguity between overloaded methods
                harmony.Patch(AccessTools.Method(typeof(SceneManager), "LoadScene", new[] { typeof(string), typeof(LoadSceneParameters) }), postfix: new HarmonyMethod(typeof(SceneManagerPatch), "LoadScenePatch"));
                //mls.LogInfo("SceneManager patches applied successfully.");

                mls.LogInfo("BetterFog patches applied successfully!");
            }
            catch (Exception ex)
            {
                mls.LogError($"Failed to apply Harmony patches: {ex}");
                throw; // Rethrow the exception to indicate initialization failure
            }

            if (settingsHotkeyEnabled)
            {
                // Check if the FogSettingsManager instance is valid
                if (FogSettingsManager.Instance != null)
                {
                    //mls.LogInfo(FogSettingsManager.Instance.ToString());
                    if (verboseLoggingEnabled)
                        mls.LogInfo("FogSettingsManager instance is valid.");
                }
                else
                {
                    if (verboseLoggingEnabled)
                        mls.LogError("FogSettingsManager instance is null.");
                }
            }
            else
            {
                if (verboseLoggingEnabled)
                    mls.LogInfo("Settings hotkey is disabled. Settings GUI will not be instantiated");
            }
        }

        //--------------------------------- Start Fog Application Methods ---------------------------------

        public static void ApplyFogSettings(bool activateAutoPresetMode) // Argument only true when autoPresetMode is being enabled/checked
        {
            if (verboseLoggingEnabled)
            {
                mls.LogInfo($"autoPresetModeEnabled: {autoPresetModeEnabled}");
                mls.LogInfo($"activateAutoPresetMode: {activateAutoPresetMode}");
            }

            if (fogRefreshLock && !autoPresetModeEnabled)
            {
                if (verboseLoggingEnabled)
                    mls.LogWarning("Fog settings refresh is locked. Skipping fog settings application.");
                return;
            }
            if (activateAutoPresetMode)
            {
                matchedPreset = null;

                foreach (var preset in autoPresetModes)
                {
                    if (verboseLoggingEnabled)
                    {
                        mls.LogInfo($"Checking Conditions: {string.Join(", ", preset.Conditions)} against Current weather: {currentWeatherType}, Current moon: {currentLevel}");
                    }
                    // Check if all conditions are met
                    var conditionsMet = preset.Conditions.All(condition =>
                    {
                        // Match against the current moon or weather
                        return condition.Equals(currentLevel) || condition.Equals(currentWeatherType) || condition.Equals("all");
                    });

                    if (conditionsMet)
                    {
                        if (verboseLoggingEnabled)
                        {
                            mls.LogInfo($"Preset match found between Conditions: {string.Join(", ", preset.Conditions)} and Current Moon: {currentLevel}, Current Weather: {currentWeatherType}");
                            mls.LogInfo($"Effect to apply: {preset.Effect}");
                        }
                        matchedPreset = preset;
                        break; // Stop at the first match
                    }
                }

                if (matchedPreset != null)
                {
                    autoPresetModeMatchFound = true;

                    var effect = matchedPreset.Effect;

                    // Check if the effect matches a preset in FogConfigPresets
                    var matchedPresetConfig = fogConfigPresets.FirstOrDefault(p => p.PresetName.ToLower() == effect);
                    if (matchedPresetConfig != null)
                    {
                        currentMode = fogModes.FirstOrDefault(m => m.Name == "Better Fog"); // Set the mode to "Better Fog"
                        currentModeIndex = fogModes.IndexOf(currentMode);
                        currentPreset = matchedPresetConfig; // Apply the preset
                        currentPresetIndex = fogConfigPresets.IndexOf(currentPreset);
                        Instance.UpdateMode();
                        if (settingsHotkeyEnabled && isFogSettingsActive)
                            FogSettingsManager.Instance.UpdateSettings();
                        if (verboseLoggingEnabled)
                            mls.LogInfo($"Preset applied: {currentPreset}, Mode set to {currentMode.Name}");
                    }
                    else
                    {
                        // Check if the effect matches a mode in FogModes
                        var matchedModeConfig = fogModes.FirstOrDefault(m => m.Name.ToLower() == effect);
                        if (matchedModeConfig != null)
                        {
                            currentMode = matchedModeConfig; // Set the mode
                            currentModeIndex = fogModes.IndexOf(currentMode);
                            Instance.UpdateMode();
                            if (settingsHotkeyEnabled && isFogSettingsActive)
                                FogSettingsManager.Instance.UpdateSettings();
                            if (verboseLoggingEnabled)
                                mls.LogInfo($"Mode set to {currentMode.Name}");
                        }
                    }
                }
                else
                {
                    autoPresetModeMatchFound = false;
                }
            }

            UpdateLockInteractionSettings();
            mls.LogInfo("Applying fog settings from ApplyFogSettings");

            SetWeatherScale();

            var fogObjects = Resources
                .FindObjectsOfTypeAll<LocalVolumetricFog>()
                .ToList();

            foreach (var fogObject in fogObjects)
            {
                ProcessFogObject(fogObject, activateAutoPresetMode);
            }

            //if (currentMode.Name == "Better Fog")
            //    mls.LogInfo($"Applied Preset: {currentPreset.ToString()}, DensityScale={combinedDensityScale}"); // Log the applied preset values
        }

        // New Function
        private static void ProcessFogObject(LocalVolumetricFog fogObject, bool activateAutoPresetMode) // Argument only true when autoPresetMode is being enabled/checked
        {
            var enemyLayer = LayerMask.NameToLayer("Enemies");

            if (fogObject != null &&
                !(fogObject.name == "FogExclusionZone" && excludeShipFogEnabled) &&
                !(fogObject.gameObject.layer == enemyLayer && excludeEnemyFogEnabled) &&
                !(currentMode.Name == "Vanilla"))
            {
                ApplyFogParameters(fogObject);
            }
            // If the mode is set to vanilla, or the fog object is in the enemies layer but needs to be set to vanilla, reset the fog to vanilla
            else if ((currentMode.Name == "Vanilla") ||
                ((fogObject != null &&
                !(fogObject.name == "FogExclusionZone" && excludeShipFogEnabled) &&
                (fogObject.gameObject.layer == enemyLayer && excludeEnemyFogEnabled) &&
                !(currentMode.Name == "Vanilla"))))
            {
                ResetFogToVanilla(fogObject.gameObject);
            }

            if (verboseLoggingEnabled)
            {
                Color fogColor = fogObject.parameters.albedo;
                mls.LogInfo($"Found LocalVolumetricFog object: {fogObject.name}, MeanFreePath: {fogObject.parameters.meanFreePath}, AlbedoR: {fogColor.r}, AlbedoG: {fogColor.g}, AlbedoB: {fogColor.b}");
            }
        }

        private static void ApplyFogParameters(LocalVolumetricFog fogObject)
        {
            var parameters = fogObject.parameters;

            //if (densityScaleEnabled)
            //{
            if ((currentPreset.MeanFreePath * combinedDensityScale) > maxDensitySliderValue) // Max density is as thick as the fog can get. Do not exceed this value.
            {
                parameters.meanFreePath = 0;
            }
            else
            {
                // The range 13000-15000 has the greatest effect on fog density so this is what is scaled
                parameters.meanFreePath = 0.262f * ((maxDensitySliderValue) - (currentPreset.MeanFreePath * combinedDensityScale));
            }
            
            // Ensure that the color values are within the acceptable range
            if (currentPreset.AlbedoR > maxColorValue)
                currentPreset.AlbedoR = maxColorValue;
            else if (currentPreset.AlbedoR < 0)
                currentPreset.AlbedoR = 0;

            if (currentPreset.AlbedoG > maxColorValue)
                currentPreset.AlbedoG = maxColorValue;
            else if (currentPreset.AlbedoG < 0)
                currentPreset.AlbedoG = 0;

            if (currentPreset.AlbedoB > maxColorValue)
                currentPreset.AlbedoB = maxColorValue;
            else if (currentPreset.AlbedoB < 0)
                currentPreset.AlbedoB = 0;

            parameters.albedo = new Color(
                currentPreset.AlbedoR,
                currentPreset.AlbedoG,
                currentPreset.AlbedoB,
                1f
            );

            fogObject.parameters = parameters;
        }

        private static void SetWeatherScale()
        {
            moonDensityScale = 1; // Default to 1 if no moon is detected
            weatherDensityScale = 1; // Default to 1 if no weather is detected

            if (densityScaleEnabled)
            {
                if (!moonScaleBlacklist.Contains(currentLevel) && !moonScaleBlacklist.Contains(currentWeatherType)) // don't scale moon density when moon/weather is blacklisted.
                {
                    // Handle Moon Scaling
                    foreach (MoonScale moonScale in moonScales)
                    {
                        //mls.LogInfo(weatherScale.WeatherName);
                        if ((currentLevel == moonScale.MoonName))
                        {
                            moonDensityScale = moonScale.Scale;
                            if (verboseLoggingEnabled)
                                mls.LogInfo($"{currentLevel} moon detected. Set moon density scale to " + moonDensityScale);
                            break;
                        }
                        else if (moonScale.MoonName == "Fallback")
                        {
                            moonDensityScale = moonScale.Scale;
                            if (verboseLoggingEnabled)
                                mls.LogInfo("Fallback moon scale detected. Set moon density scale to " + moonDensityScale);
                        }
                        if (moonScale.MoonName == moonScales[moonScales.Count - 1].MoonName)
                        {
                            if (verboseLoggingEnabled)
                                mls.LogWarning($"{currentLevel} moon not found in records. Using moon scale of {moonDensityScale}.");
                        }
                    }
                }
                else
                    mls.LogInfo("Blacklisted moon or weather detected. Setting moon density scale to 1.");

                if (!weatherScaleBlacklist.Contains(currentLevel) && !weatherScaleBlacklist.Contains(currentWeatherType)) // don't scale weather density when moon/weather is blacklisted.
                {
                    // Handle Weather Scaling
                    foreach (WeatherScale weatherScale in weatherScales)
                    {
                        //mls.LogInfo(weatherScale.WeatherName);
                        if (currentWeatherType == weatherScale.WeatherName)
                        {
                            weatherDensityScale = weatherScale.Scale;
                            if (verboseLoggingEnabled)
                                mls.LogInfo($"{currentWeatherType} weather type detected. Set weather density scale to " + weatherDensityScale);
                            break;
                        }
                        else if (weatherScale.WeatherName == "Fallback")
                        {
                            weatherDensityScale = weatherScale.Scale;
                            if (verboseLoggingEnabled)
                                mls.LogInfo("Fallback weather scale detected. Set weather density scale to " + weatherDensityScale);
                        }
                        if (weatherScale.WeatherName == weatherScales[weatherScales.Count - 1].WeatherName)
                        {
                            if (verboseLoggingEnabled)
                                mls.LogWarning($"{currentWeatherType} weather type not found in records. Using scale of {weatherDensityScale}.");
                        }
                    }
                }
                else if (verboseLoggingEnabled)
                    mls.LogInfo("Blacklisted moon or weather detected. Setting weather density scale to 1.");
            }

            combinedDensityScale = moonDensityScale * weatherDensityScale;
            if (verboseLoggingEnabled)
            {
                mls.LogInfo($"Final density scale applied: {moonDensityScale} * {weatherDensityScale} = {combinedDensityScale}");
                mls.LogInfo($"Preset original MeanFreePath: {currentPreset.MeanFreePath}"); // Log the original MeanFreePath (density) value
                mls.LogInfo($"Scaled MeanFreePath: 0.262 * ({maxDensitySliderValue} - {currentPreset.MeanFreePath * combinedDensityScale})  = {0.262f * (maxDensitySliderValue - (currentPreset.MeanFreePath * combinedDensityScale))}"); // Log the scaled MeanFreePath (density) value
                if ((currentPreset.MeanFreePath * combinedDensityScale) > maxDensitySliderValue)
                    mls.LogWarning($"MeanFreePath value exceeded max Density SliderValue. Setting density to {maxDensitySliderValue}");
            }
        }

        private static void ApplyAutoPresetMode(LocalVolumetricFog fogObject) // Apply auto preset/mode settings if they match current weather or moon
        {

            ApplyFogParameters(fogObject);
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
            int waitTimeLimit = 60;
            if (verboseLoggingEnabled)
                mls.LogInfo($"Waiting for game to start... ({waitTime}s)");

            // Wait until the game starts or timeout after 100 seconds
            while (GameNetworkManager.Instance != null && !GameNetworkManager.Instance.gameHasStarted && waitTime < waitTimeLimit)
            {
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
                if (verboseLoggingEnabled)
                    mls.LogInfo($"{waitTime}s Timeout reached, game has not started yet. Fog settings not applied.");
            }

            applyingFogSettings = false;
        }

        private static IEnumerator ApplyFogSettingsRepeated(int repeatCount)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                if (verboseLoggingEnabled)
                    mls.LogInfo($"Applying fog settings... ({i + 1}/{repeatCount})");

                ApplyFogSettings(false); // Apply fog settings

                // Wait for 1 second between applications
                yield return new WaitForSecondsRealtime(1);
            }
        }

        //--------------------------------- End Fog Application Methods ---------------------------------

        public static void NextPreset()
        {
            if (lockPresetDropdownModification)
            {
                mls.LogWarning("Cannot switch presets when preset interaction is disabled.");
                return;
            }
            currentPresetIndex = fogConfigPresets.IndexOf(currentPreset);
            if (currentPresetIndex == fogConfigPresets.Count - 1)
            {
                currentPresetIndex = -1; // Reset to the first preset if the last preset is reached
            }
            currentPresetIndex++;
            currentPreset = fogConfigPresets[currentPresetIndex];
            //mls.LogInfo("Current preset index: " + currentPresetIndex);
            mls.LogInfo($"Switched to preset: {currentPreset.PresetName}");
            ApplyFogSettings(false);

            // Notify FogSettingsManager to update dropdown
            if (isFogSettingsActive)
                FogSettingsManager.Instance.UpdateSettings();
        }

        public static void NextMode()
        {
            if (lockModeDropdownModification)
            {
                mls.LogWarning("Cannot switch modes when mode interaction is disabled.");
                return;
            }
            currentModeIndex = fogModes.IndexOf(currentMode);
            if (currentModeIndex == fogModes.Count - 1)
            {
                currentModeIndex = -1; // Reset to the first preset if the last preset is reached
            }
            currentModeIndex++;
            currentMode = fogModes[currentModeIndex];

            mls.LogInfo($"Switched to next mode: {currentMode.Name}");
            Instance.UpdateMode();

            // Lock the preset modification if mode is set to No Fog or Vanilla
            if (settingsHotkeyEnabled && (currentMode.Name == "No Fog" || currentMode.Name == "Vanilla") && autoPresetModeMatchFound)
            {
                lockPresetDropdownModification = true;
                lockPresetValueModification = true;
            }
            else if (settingsHotkeyEnabled)
                lockPresetDropdownModification = false;

            // Notify FogSettingsManager to update dropdown
            if (isFogSettingsActive)
            {
                FogSettingsManager.Instance.UpdateSettings();
                FogSettingsManager.Instance.LockPresetDropdownInteract(lockPresetDropdownModification);
                FogSettingsManager.Instance.LockPresetButtonInteract(lockPresetValueModification);
                FogSettingsManager.Instance.LockPresetValueInteract(lockPresetValueModification);
            }

            ApplyFogSettings(false);
        }

        public void UpdateMode()
        {
            if (currentMode.Name == "No Fog")
            {
                mls.LogInfo("No Fog mode selected.");
                EnableFogDisablePatch();
                fogRefreshLock = true;
                return;
            }
            else
            {
                fogRefreshLock = false;
                DisableFogPatch();
                if (currentMode.Name == "Vanilla")
                {
                    mls.LogInfo("Vanilla mode selected.");
                    DisableNonVanillaPatches();
                    return;
                }
                else
                {
                    EnableNonVanillaPatches();
                }
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
                    weatherScales.Add(new WeatherScale(keyValue[0].ToLower(), scaleValue));
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
                    if (scaleValue < 0)
                        scaleValue = 0;
                    moonScales.Add(new MoonScale(keyValue[0].ToLower(), scaleValue));
                }
            }
            return moonScales;
        }

        private List<string> ParseDensityScaleBlacklist(string configString)
        {
            var blacklist = new List<string>();
            var items = configString.Split(',');

            foreach (var item in items)
            {
                blacklist.Add(item.ToLower());
            }

            return blacklist;
        }

        // Function to parse the auto preset/mode settings
        private List<AutoPresetMode> ParseAutoPresetMode(string configString)
        {
            var autoPresetMode = new List<AutoPresetMode>();
            var pairs = configString.Split(',');

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    autoPresetMode.Add(new AutoPresetMode(keyValue[0].ToLower(), keyValue[1].ToLower()));
                }
            }
            return autoPresetMode;
        }

        //--------------------------------- Start No Fog Management ---------------------------------

        public void EnableFogDisablePatch()
        {
            var method = AccessTools.Method(typeof(Fog), "UpdateShaderVariablesGlobalCBFogParameters");
            var patchInfo = Harmony.GetPatchInfo(method);
            //mls.LogInfo($"PatchInfo: {patchInfo}");

            try
            {
                if (patchInfo == null)
                {
                    if (verboseLoggingEnabled)
                        mls.LogInfo("Fog disable patch is not active. Patching.");
                }
                else
                {
                    if (verboseLoggingEnabled)
                        mls.LogInfo($"A fog disable patch list is active. Checking for BetterFog patches.");

                    // Check for existing BetterFog patches
                    bool hasBetterFogPatch = patchInfo.Prefixes.Any(prefix => prefix.owner == modGUID);
                    if (hasBetterFogPatch)
                    {
                        if (verboseLoggingEnabled)
                            mls.LogInfo("BetterFog patches are already active. Skipping patching.");
                        return;
                    }

                    if(verboseLoggingEnabled)
                        mls.LogInfo("No BetterFog patches detected. Proceeding with patching.");
                }
            }
            catch (Exception ex)
            {
                if (verboseLoggingEnabled)
                    mls.LogError($"There was an issue in detecting disable fog patch. Skipping patching. {ex}");
                return;
            }

            harmony.Patch(original: AccessTools.Method(typeof(Fog), "UpdateShaderVariablesGlobalCBFogParameters"), prefix: new HarmonyMethod(typeof(FogPatch), "Prefix"));
            if (verboseLoggingEnabled)
                mls.LogInfo("Fog disable patch applied successfully.");
        }

        public void DisableFogPatch()
        {
            var method = AccessTools.Method(typeof(Fog), "UpdateShaderVariablesGlobalCBFogParameters");
            var patchInfo = Harmony.GetPatchInfo(method);
            //mls.LogInfo($"PatchInfo: {patchInfo}");

            try
            {
                if (patchInfo == null)
                {
                    if (verboseLoggingEnabled)
                        mls.LogInfo("Fog disable patch is not active. Skipping unpatching.");
                    return;
                }
                else
                {
                    if (verboseLoggingEnabled)
                        mls.LogInfo($"A fog disable patch list is active. Checking for BetterFog patches.");

                    // Check for existing BetterFog patches
                    bool hasBetterFogPatch = patchInfo.Prefixes.Any(prefix => prefix.owner == modGUID);
                    if (!hasBetterFogPatch)
                    {
                        if (verboseLoggingEnabled)
                            mls.LogInfo("No BetterFog patches detected. Skipping unpatching.");
                        return;
                    }

                    if (verboseLoggingEnabled)
                    {
                        mls.LogInfo("BetterFog patches detected. Proceeding with unpatching.");
                    }
                }
            }
            catch (Exception ex)
            {
                if (verboseLoggingEnabled)
                    mls.LogError($"There was an issue in detecting disable fog patch. Skipping patching. {ex}");
                return;
            }

            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            if (verboseLoggingEnabled)
                mls.LogInfo("Fog disable patch unpatched successfully.");
        }
        //--------------------------------- End No Fog Management ---------------------------------

        //--------------------------------- Start Vanilla Management ---------------------------------

        public void EnableNonVanillaPatches()
        {
            var method = AccessTools.Method(typeof(AudioReverbTrigger), "changeVolume");
            var patchInfo = Harmony.GetPatchInfo(method);
            //mls.LogInfo($"PatchInfo: {patchInfo}");

            try
            {
                if (patchInfo == null)
                {
                    if (verboseLoggingEnabled)
                        mls.LogInfo("Non-vanilla patch is not active. Patching.");
                }
                else
                {
                    if (verboseLoggingEnabled)
                        mls.LogInfo($"A non-vanilla patch list is active. Checking for BetterFog patches.");

                    // Check for existing BetterFog patches
                    bool hasBetterFogPatch = patchInfo.Prefixes.Any(prefix => prefix.owner == modGUID);
                    if (hasBetterFogPatch)
                    {
                        if (verboseLoggingEnabled)
                            mls.LogInfo("BetterFog patches are already active. Skipping patching.");
                        return;
                    }

                    if (verboseLoggingEnabled)
                        mls.LogInfo("No BetterFog patches detected. Proceeding with patching.");
                }
            }
            catch (Exception ex)
            {
                if (verboseLoggingEnabled)
                    mls.LogError($"There was an issue in detecting non-vanilla patch. Skipping patching. {ex}");
                return;
            }

            harmony.Patch(original: AccessTools.Method(typeof(AudioReverbTrigger), "changeVolume"), prefix: new HarmonyMethod(typeof(AudioReverbTriggerPatch), "changeVolumePrefix"));
            //mls.LogInfo("AudioReverb patches applied successfully.");

            harmony.Patch(original: AccessTools.Method(typeof(TimeOfDay), "SetWeatherBasedOnVariables"), prefix: new HarmonyMethod(typeof(TimeOfDayPatch), "SetWeatherBasedOnVariablesPatch"));
            //mls.LogInfo("TimeOfDay patches applied successfully.");

            harmony.Patch(original: AccessTools.Method(typeof(ToggleFogTrigger), "Update"), prefix: new HarmonyMethod(typeof(ToggleFogTriggerPatch), "UpdatePatch"));
            harmony.Patch(original: AccessTools.Method(typeof(ToggleFogTrigger), "OnTriggerEnter"), prefix: new HarmonyMethod(typeof(ToggleFogTriggerPatch), "OnTriggerEnterPatch"));
            harmony.Patch(original: AccessTools.Method(typeof(ToggleFogTrigger), "OnTriggerExit"), prefix: new HarmonyMethod(typeof(ToggleFogTriggerPatch), "OnTriggerExitPatch"));
            //mls.LogInfo("ToggleFogUpdate patches applied successfully.");

            harmony.Patch(original: AccessTools.Method(typeof(EntranceTeleport), "TeleportPlayer"), postfix: new HarmonyMethod(typeof(EntranceTeleportPatch), "TeleportPlayerPatch"));
            //mls.LogInfo("EntranceTeleport patches applied successfully.");

            harmony.Patch(original: AccessTools.Method(typeof(PlayerControllerB), "TeleportPlayer"), postfix: new HarmonyMethod(typeof(PlayerControllerBPatch), "TeleportPlayerPatch"));
            harmony.Patch(original: AccessTools.Method(typeof(PlayerControllerB), "SpectateNextPlayer"), postfix: new HarmonyMethod(typeof(PlayerControllerBPatch), "SpectateNextPlayerPatch"));
            //mls.LogInfo("PlayerControllerB patches applied successfully.");

            harmony.Patch(original: AccessTools.Method(typeof(NetworkSceneManager), "OnSceneLoaded"), postfix: new HarmonyMethod(typeof(NetworkSceneManagerPatch), "OnSceneLoadedPatch"));
            //mls.LogInfo("NetworkSceneManager patches applied successfully.");
            if (verboseLoggingEnabled)
                mls.LogInfo("Non-Vanilla patches enabled successfully!");
        }

        public void DisableNonVanillaPatches()
        {
            var method = AccessTools.Method(typeof(AudioReverbTrigger), "changeVolume");
            var patchInfo = Harmony.GetPatchInfo(method);
            //mls.LogInfo($"PatchInfo: {patchInfo}");

            try
            {
                if (patchInfo == null)
                {
                    if (verboseLoggingEnabled)
                        mls.LogInfo("Non-vanilla patch is not active. Skipping unpatching.");
                    return;
                }
                else
                {
                    if (verboseLoggingEnabled)
                        mls.LogInfo($"A non-vanilla patch list is active. Checking for BetterFog patches.");

                    // Check for existing BetterFog patches
                    bool hasBetterFogPatch = patchInfo.Prefixes.Any(prefix => prefix.owner == modGUID);
                    if (!hasBetterFogPatch)
                    {
                        if (verboseLoggingEnabled)
                            mls.LogInfo("No BetterFog patches detected. Skipping unpatching.");
                        return;
                    }

                    if (verboseLoggingEnabled)
                        mls.LogInfo("BetterFog patches detected. Proceeding with unpatching.");
                }
            }
            catch (Exception ex)
            {
                if (verboseLoggingEnabled)
                    mls.LogError($"There was an issue in detecting non-vanilla patch. Skipping patching. {ex}");
                return;
            }
            
            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            //mls.LogInfo("AudioReverb patches disabled successfully.");

            method = AccessTools.Method(typeof(TimeOfDay), "SetWeatherBasedOnVariables");
            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            //mls.LogInfo("TimeOfDay patches disabled successfully.");

            method = AccessTools.Method(typeof(ToggleFogTrigger), "Update");
            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            method = AccessTools.Method(typeof(ToggleFogTrigger), "OnTriggerEnter");
            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            method = AccessTools.Method(typeof(ToggleFogTrigger), "OnTriggerExit");
            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            //mls.LogInfo("ToggleFogUpdate patches disabled successfully.");

            method = AccessTools.Method(typeof(EntranceTeleport), "TeleportPlayer");
            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            //mls.LogInfo("EntranceTeleport patches disabled successfully.");

            method = AccessTools.Method(typeof(PlayerControllerB), "TeleportPlayer");
            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            method = AccessTools.Method(typeof(PlayerControllerB), "SpectateNextPlayer");
            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            //mls.LogInfo("PlayerControllerB patches disabled successfully.");

            method = AccessTools.Method(typeof(NetworkSceneManager), "OnSceneLoaded");
            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            //mls.LogInfo("NetworkSceneManager patches disabled successfully.");
            if (verboseLoggingEnabled)
                mls.LogInfo("Non-Vanilla patches disabled successfully!");
        }

        public static void CollectVanillaValues()
        {
            //fogRefreshLock = true; // Lock the fog refresh to prevent settings from being applied
            //mls.LogInfo("LoadScenePatch triggered");
            var fogObjects = Resources
                .FindObjectsOfTypeAll<LocalVolumetricFog>()
                .ToList();

            // Iterate through each fog object and log/capture vanilla parameters
            foreach (var fogObject in fogObjects)
            {
                // Capture vanilla parameters
                var fogParams = fogObject.parameters;
                Color fogColor = fogObject.parameters.albedo;
                if (verboseLoggingEnabled)
                    mls.LogInfo($"Found LocalVolumetricFog object: {fogObject.name}, MeanFreePath: {fogObject.parameters.meanFreePath}, AlbedoR: {fogColor.r}, AlbedoG: {fogColor.g}, AlbedoB: {fogColor.b}");

                // Store the vanilla values into the dictionary
                if (!fogParameterChanges.ContainsKey(fogObject.gameObject))
                {
                    fogParameterChanges[fogObject.gameObject] = fogParams;
                    if (verboseLoggingEnabled)
                        mls.LogInfo($"Captured vanilla fog parameters for {fogObject.gameObject.name}");
                }
            }

            // Unlock the fog refresh after capturing vanilla values
            //fogRefreshLock = false;
        }

        public static void ResetFogToVanilla(GameObject fogObject)
        {
            if (fogParameterChanges.TryGetValue(fogObject, out var vanillaParams)) // Check if the vanilla parameters are found
            {
                var fogComponent = fogObject.GetComponent<LocalVolumetricFog>();
                if (fogComponent != null) // Check if the LocalVolumetricFog component is found
                {
                    fogComponent.parameters = vanillaParams;
                    //mls.LogInfo($"Reverted fog parameters for {fogObject.name} to vanilla.");
                }
            }
        }

        //--------------------------------- End Vanilla Management ---------------------------------

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
                ApplyFogSettings(false);
            }
            else
            {
                mls.LogInfo("Game has not started yet. Refusing to apply fog settings.");
            }
        }

        public PlayerControllerB FindLocalPlayer(ulong localClientId)
        {
            PlayerControllerB[] allPlayers = FindObjectsOfType<PlayerControllerB>();

            foreach (PlayerControllerB playerController in allPlayers)
            {
                if (playerController.actualClientId == localClientId) // Compare the local client's ID
                {
                    if (verboseLoggingEnabled)
                        mls.LogInfo($"Local player found: {playerController.name}");
                    return playerController;
                }
            }

            mls.LogError("Local player not found!");
            return null; // Handle cases where no local player is found
        }

        // --------------------------------- Start Auto Preset/Mode Management ---------------------------------

        public static void ToggleAutoPresetMode()
        {
            autoPresetModeEnabled = !autoPresetModeEnabled;
            if (autoPresetModeEnabled)
            {
                mls.LogInfo("Auto Preset/Mode enabled.");
                ApplyFogSettings(true);
            }
            else
            {
                mls.LogInfo("Auto Preset/Mode disabled.");
                ApplyFogSettings(false);
            }
            if (isFogSettingsActive)
            {
                FogSettingsManager.Instance.UpdateSettings();
            }
        }

        public static void ToggleWeatherScaling()
        {
            if (lockPresetDropdownModification)
            {
                mls.LogWarning("Cannot toggle weather when preset interaction is disabled.");
                return;
            }
            densityScaleEnabled = !densityScaleEnabled;
            ApplyFogSettings(false);
            if (isFogSettingsActive)
            {
                FogSettingsManager.Instance.UpdateSettings();
            }
        }

        public static void UpdateLockInteractionSettings()
        {
            if (autoPresetModeMatchFound & autoPresetModeEnabled)
            {
                mls.LogInfo($"AutoPresetMode Match Found: {matchedPreset.Effect}");
                lockModeDropdownModification = true;
                lockPresetDropdownModification = true;
                lockPresetValueModification = (currentMode.Name == "No Fog" || currentMode.Name == "Vanilla");
            }
            else if (currentMode.Name == "No Fog" || currentMode.Name == "Vanilla")
            {
                mls.LogInfo($"AutoPresetMode Match Not Found. Mode: {currentMode.Name}");
                lockModeDropdownModification = false;
                lockPresetDropdownModification = true;
                lockPresetValueModification = true;
            }
            else
            {
                mls.LogInfo($"AutoPresetMode Match Not Found. Mode: {currentMode.Name}");
                lockModeDropdownModification = false;
                lockPresetDropdownModification = false;
                lockPresetValueModification = false;
            }

            if (isFogSettingsActive) // Update settings GUI, if applicable
            {
                FogSettingsManager.Instance.LockPresetDropdownInteract(lockPresetDropdownModification);
                FogSettingsManager.Instance.LockPresetButtonInteract(lockPresetValueModification);
                FogSettingsManager.Instance.LockPresetValueInteract(lockPresetValueModification);
                FogSettingsManager.Instance.LockModeDropdownInteract(lockModeDropdownModification);
            }
        }
    }
    //--------------------------------- End Class ---------------------------------
}
//--------------------------------- End File ---------------------------------
