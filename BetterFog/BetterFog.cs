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

namespace BetterFog
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class BetterFog : BaseUnityPlugin
    {
        public const string modGUID = "ironthumb.BetterFog";
        public const string modName = "BetterFog";
        public const string modVersion = "3.3.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource mls;
        private static BetterFog instance;

        // Hotkey variables
        public static bool hotkeysEnabled = true;
        private static ConfigEntry<string> nextPresetHotkeyConfig;
        public static ConfigEntry<bool> nextPresetHotKeyEnabled;
        private static ConfigEntry<string> nextModeHotkeyConfig;
        public static ConfigEntry<bool> nextModeHotKeyEnabled;
        private static ConfigEntry<string> refreshPresetHotkeyConfig;
        public static ConfigEntry<bool> refreshHotKeyEnabled;
        public static ConfigEntry<string> weatherScaleHotkeyConfig;
        public static ConfigEntry<bool> weatherScaleHotKeyEnabled;
        public static ConfigEntry<string> settingsHotkeyConfig;
        public static ConfigEntry<bool> settingsHotKeyEnabled;

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
        public static string currentWeatherType = "None";
        private static float moonDensityScale = 1f;
        private static float weatherDensityScale = 1f;
        public static float combinedDensityScale = 1f;
        public static List<WeatherScale> weatherScales;
        private static ConfigEntry<string> weatherScalesConfig;
        public static string currentLevel = "";
        public static List<MoonScale> moonScales;
        private static ConfigEntry<string> moonScalesConfig;

        // Fog preset variables
        private static ConfigEntry<string> defaultPresetName;
        public static List<FogConfigPreset> fogConfigPresets;
        private ConfigEntry<string>[] presetEntries;
        public static int currentPresetIndex;
        public static FogConfigPreset currentPreset;

        public static Dictionary<GameObject, LocalVolumetricFogArtistParameters> fogParameterChanges = new Dictionary<GameObject, LocalVolumetricFogArtistParameters>(); // Dictionary to store vanilla fog settings

        private static ConfigEntry<string> defaultMode;
        public static List<BetterFogMode> fogModes;
        public static int currentModeIndex;
        public static BetterFogMode currentMode;

        public static bool applyingFogSettings = false;

        public static PlayerControllerB player;

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

            // Initialize your fogConfigPresets list
            // Arguments: Preset Name, MeanFreePath, AlbedoR, AlbedoG, AlbedoB, AlbedoA
            fogConfigPresets = new List<FogConfigPreset>
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
                "Mean Free Path - Density of fog. The greater the number, the less dense. 0 is the minimum (opaque fog).\n");
            defaultMode = Config.Bind(section1, "Default Fog Mode", "Better Fog", "Name of the default fog mode. Options: Better Fog, No Fog, Vanilla");

            string section2 = "Key Bindings";
            settingsHotkeyConfig = Config.Bind(section2, "Settings Hotkey", "f1", "Hotkey to open the BetterFog settings menu.");
            settingsHotKeyEnabled = Config.Bind(section2, "Enable Settings Hotkey", true, "Enable or disable hotkey for opening the BetterFog settings menu.");
            nextPresetHotkeyConfig = Config.Bind(section2, "Next Preset Hotkey", "n", "Hotkey to switch to the next fog preset.");
            nextPresetHotKeyEnabled = Config.Bind(section2, "Enable Next Hotkey", true, "Enable or disable hotkeys for switching fog presets.");
            nextModeHotkeyConfig = Config.Bind(section2, "Next Mode Hotkey", "m", "Hotkey to switch to the next fog mode.");
            nextModeHotKeyEnabled = Config.Bind(section2, "Enable Next Mode Hotkey", false, "Enable or disable hotkey for switching fog modes.");
            refreshPresetHotkeyConfig = Config.Bind(section2, "Refresh Hotkey", "r", "Hotkey to refresh fog settings.");
            refreshHotKeyEnabled = Config.Bind(section2, "Enable Refresh Hotkey", true, "Enable or disable hotkey for refreshing fog settings.");
            weatherScaleHotkeyConfig = Config.Bind(section2, "Weather Scale Hotkey", "c", "Hotkey to toggle weather scaling.");
            weatherScaleHotKeyEnabled = Config.Bind(section2, "Enable Weather Scale Hotkey", false, "Enable or disable hotkey for Weather Scaling toggle.");

            string section3 = "Fog Settings";
            excludeShipFogDefault = Config.Bind(section3, "Apply to Fog Exclusion Zone", true, "Apply fog settings to the Fog Exclusion Zone (eg. inside of ship).");
            excludeShipFogEnabled = excludeShipFogDefault.Value;
            densityScaleEnabledDefault = Config.Bind(section3, "Weather Scale Enabled Default", true, "Enable density scaling for moons and weather by default when booting game.");
            densityScaleEnabled = densityScaleEnabledDefault.Value;
            verboseLoggingDefault = Config.Bind(section3, "Verbose Logging Enabled", false, "Enable or disable verbose logging for the mod. Can be used for debugging.");
            verboseLoggingEnabled = verboseLoggingDefault.Value;
            excludeEnemyFogDefault = Config.Bind(section3, "Exclude Enemy Fog", true, "Exclude enemy fog when applying better fog settings.");
            excludeEnemyFogEnabled = excludeEnemyFogDefault.Value;

            // Initialize the key bindings with the hotkey value
            IngameKeybinds.Instance.InitializeKeybindings(nextPresetHotkeyConfig.Value, nextModeHotkeyConfig.Value, refreshPresetHotkeyConfig.Value, weatherScaleHotkeyConfig.Value, settingsHotkeyConfig.Value);

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
                            fogConfigPresets[i].MeanFreePath = float.Parse(value);
                            break;
                        case "Red Hue":
                            fogConfigPresets[i].AlbedoR = float.Parse(value);
                            break;
                        case "Green Hue":
                            fogConfigPresets[i].AlbedoG = float.Parse(value);
                            break;
                        case "Blue Hue":
                            fogConfigPresets[i].AlbedoB = float.Parse(value);
                            break;
                    }
                }
            }

            string section4 = "Weather and Moon Density Scales";
            moonScalesConfig = Config.Bind(section4, "MoonScales", "71 Gordion=1,41 Experimentation=0.95,220 Assurance=0.9,56 Vow=0.8,21 Offense=0.9," +
                "61 March=0.75,20 Adamance=0.75,85 Rend=0.285,7 Dine=0.325,8 Titan=0.285,68 Artifice=0.9,5 Embrion=0.85,44 Liquidation=0.85",
                "Moon scales in the format {Gordion=1,41 Experimentation=0.95,220 Assurance=0.9,...}");

            moonScales = ParseMoonScales(moonScalesConfig.Value);

            weatherScalesConfig = Config.Bind(section4, "WeatherScales", "none=1,rainy=0.75,stormy=0.5,foggy=0.45,eclipsed=0.77,dust clouds=0.8,flooded=0.765",
            "Weather scales in the format {none=1,rainy=0.69,stormy=0.65,...}");

            weatherScales = ParseWeatherScales(weatherScalesConfig.Value);

            weatherScaleBlacklistConfig = Config.Bind(section4, "Weather Scaling Blacklist", "",
                "Enter weather names or moon names to trigger temporary disablement of fog WEATHER density scaling. This is only effective when WeatherScale is enabled. \n" +
                "Full moon or weather names must be typed in, comma separated. Example: {eclipsed,20 Adamance,85 Rend}");
            moonScaleBlacklistConfig = Config.Bind(section4, "Moon Scaling Blacklist", "",
                "Enter weather names or moon names to trigger temporary disablement of MOON fog density scaling. This is only effective when WeatherScale is enabled. \n" +
                "Full moon or weather names must be typed in, comma separated. Example: {eclipsed,20 Adamance,85 Rend}");

            weatherScaleBlacklist = ParseDensityScaleBlacklist(weatherScaleBlacklistConfig.Value);
            moonScaleBlacklist = ParseDensityScaleBlacklist(moonScaleBlacklistConfig.Value);

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
                    // Attempt to find the preset with the default name
                    //foreach(var preset in fogConfigPresets)
                    //{
                    //    mls.LogInfo(preset.ToString());
                    //}
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

                //harmony.Patch(original: AccessTools.Method(typeof(QuickMenuManager), "OpenQuickMenu"), postfix: new HarmonyMethod(typeof(QuickMenuManagerPatch), "OpenQuickMenuPatch"));
                //harmony.Patch(original: AccessTools.Method(typeof(QuickMenuManager), "CloseQuickMenu"), postfix: new HarmonyMethod(typeof(QuickMenuManagerPatch), "CloseQuickMenuPatch"));
                //mls.LogInfo("QuickMenuManager patches applied successfully.");

                //harmony.Patch(original: AccessTools.Method(typeof(IngamePlayerSettings), "RefreshAndDisplayCurrentMicrophone"), postfix: new HarmonyMethod(typeof(IngamePlayerSettingsPatch), "RefreshAndDisplayCurrentMicrophonePatch"));
                //harmony.Patch(original: AccessTools.Method(typeof(IngamePlayerSettings), "DiscardChangedSettings"), postfix: new HarmonyMethod(typeof(IngamePlayerSettingsPatch), "DiscardChangedSettingsPatch"));
                //mls.LogInfo("IngamePlayerSettings patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(Terminal), "BeginUsingTerminal"), postfix: new HarmonyMethod(typeof(TerminalPatch), "BeginUsingTerminalPatch"));
                harmony.Patch(original: AccessTools.Method(typeof(Terminal), "QuitTerminal"), postfix: new HarmonyMethod(typeof(TerminalPatch), "QuitTerminalPatch"));
                //mls.LogInfo("Terminal patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(MenuManager), "PlayCancelSFX"), postfix: new HarmonyMethod(typeof(MenuManagerPatch), "PlayCancelSFXPatch"));
                //mls.LogInfo("MenuManager patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(HUDManager), "EnableChat_performed"), postfix: new HarmonyMethod(typeof(HUDManagerPatch), "EnableChat_performedPatch"));
                harmony.Patch(original: AccessTools.Method(typeof(HUDManager), "SubmitChat_performed"), postfix: new HarmonyMethod(typeof(HUDManagerPatch), "SubmitChat_performedPatch"));
                harmony.Patch(original: AccessTools.Method(typeof(HUDManager), "OpenMenu_performed"), postfix: new HarmonyMethod(typeof(HUDManagerPatch), "OpenMenu_performedPatch"));
                //mls.LogInfo("HUDManager patches applied successfully.");

                harmony.Patch(original: AccessTools.Method(typeof(LocalVolumetricFog), "OnEnable"), postfix: new HarmonyMethod(typeof(LocalVolumetricFogPatch), "OnEnablePatch"));
                //mls.LogInfo("LocalVolumetricFog patches applied successfully.");

                harmony.Patch(AccessTools.Method(typeof(SceneManager), "LoadScene", new[] { typeof(string), typeof(LoadSceneParameters) }), postfix: new HarmonyMethod(typeof(SceneManagerPatch), "LoadScenePatch"));
                //mls.LogInfo("SceneManager patches applied successfully.");

                mls.LogInfo("BetterFog patches applied successfully!");
            }
            catch (Exception ex)
            {
                mls.LogError($"Failed to apply Harmony patches: {ex}");
                throw; // Rethrow the exception to indicate initialization failure
            }

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

        //--------------------------------- Start Fog Application Methods ---------------------------------

        public static void ApplyFogSettings()
        {
            moonDensityScale = 1; // Default to 1 if no moon is detected
            weatherDensityScale = 1; // Default to 1 if no weather is detected

            if (fogRefreshLock)
            {
                if (verboseLoggingEnabled)
                    mls.LogWarning("Fog settings refresh is locked. Skipping fog settings application.");
                return;
            }
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
                        if ((currentWeatherType == weatherScale.WeatherName))
                        {
                            weatherDensityScale = weatherScale.Scale;
                            if (verboseLoggingEnabled)
                                mls.LogInfo($"{currentWeatherType} weather type detected. Set weather density scale to " + weatherDensityScale);
                            break;
                        }
                        if (weatherScale.WeatherName == weatherScales[weatherScales.Count - 1].WeatherName)
                        {
                            if (verboseLoggingEnabled)
                                mls.LogWarning($"{currentWeatherType} weather type not found in records. Using scale of {weatherDensityScale}.");
                        }
                    }
                }
                else
                    mls.LogInfo("Blacklisted moon or weather detected. Setting weather density scale to 1.");
            }

            combinedDensityScale = moonDensityScale * weatherDensityScale;
            if (verboseLoggingEnabled)
            {
                mls.LogInfo($"Final density scale applied: {moonDensityScale} * {weatherDensityScale} = {combinedDensityScale}");
                mls.LogInfo($"Preset original MeanFreePath: {currentPreset.MeanFreePath}"); // Log the original MeanFreePath (density) value
                mls.LogInfo($"Scaled MeanFreePath: {currentPreset.MeanFreePath} * {combinedDensityScale} = {currentPreset.MeanFreePath * combinedDensityScale}"); // Log the scaled MeanFreePath (density) value
            }



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
                // Apply settings if:
                // the fog object is not null
                // FogExclusionZone is not excluded
                // Enemy fog is not excluded
                var enemyLayer = LayerMask.NameToLayer("Enemies");
                if (fogObject != null &&
                    !(fogObject.name == "FogExclusionZone" && excludeShipFogEnabled == true) &&
                    !(fogObject.gameObject.layer == enemyLayer && excludeEnemyFogEnabled == true) &&
                    !(currentMode.Name == "Vanilla"))
                {
                    // prepare the parameters object for modification
                    var parameters = fogObject.parameters;

                    if (densityScaleEnabled)
                    {
                        // Set new density with scaling applied
                        parameters.meanFreePath = currentPreset.MeanFreePath * combinedDensityScale;
                    }
                    else
                    {
                        //mls.LogInfo("Weather scaling is disabled. Using a scale of 1.");
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
                else if (currentMode.Name == "Vanilla") // attempt to use vanilla settings
                {
                    ResetFogToVanilla(fogObject.gameObject);
                }

                if (verboseLoggingEnabled) // Log settings if verbose logging is enabled.
                {
                    // Print details of the Fog object
                    Color fogColor = fogObject.parameters.albedo;
                    mls.LogInfo($"Found LocalVolumetricFog object: {fogObject.name}, MeanFreePath: {fogObject.parameters.meanFreePath}, AlbedoR: {fogColor.r}, AlbedoG: {fogColor.g}, AlbedoB: {fogColor.b}");
                }
            }

            if (currentMode.Name == "Better Fog")
                mls.LogInfo($"Applied Preset: {currentPreset.ToString()}, DensityScale={combinedDensityScale}"); // Log the applied preset values
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

                ApplyFogSettings(); // Apply fog settings

                // Wait for 1 second between applications
                yield return new WaitForSecondsRealtime(1);
            }
        }

        //--------------------------------- End Fog Application Methods ---------------------------------

        public static void NextPreset()
        {
            mls.LogInfo("Next preset hotkey pressed.");
            currentPresetIndex = fogConfigPresets.IndexOf(currentPreset);
            if (currentPresetIndex == fogConfigPresets.Count - 1)
            {
                currentPresetIndex = -1; // Reset to the first preset if the last preset is reached
            }
            currentPresetIndex ++;
            currentPreset = fogConfigPresets[currentPresetIndex];
            //mls.LogInfo("Current preset index: " + currentPresetIndex);
            mls.LogInfo($"Switched to preset: {currentPreset.PresetName}");
            ApplyFogSettings();

            // Notify FogSettingsManager to update dropdown
            FogSettingsManager.Instance.UpdateSettings();
        }

        public static void NextMode()
        {
            mls.LogInfo("Next mode hotkey pressed.");
            currentModeIndex = fogModes.IndexOf(currentMode);
            if (currentModeIndex == fogModes.Count - 1)
            {
                currentModeIndex = -1; // Reset to the first preset if the last preset is reached
            }
            currentModeIndex++;
            currentMode = fogModes[currentModeIndex];
            //mls.LogInfo("Current mode index: " + currentModeIndex);
            mls.LogInfo($"Switched to mode: {currentMode.Name}");
            Instance.UpdateMode();

            // Notify FogSettingsManager to update dropdown
            FogSettingsManager.Instance.UpdateSettings();
        }

        public void UpdateMode()
        {

            if (currentMode.Name == "No Fog")
            {
                mls.LogInfo("No Fog mode selected.");
                EnableFogPatch();
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
                    DisableVanillaPatches();
                    return;
                }
                else
                {
                    EnableVanillaPatches();
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

        private List<string> ParseDensityScaleBlacklist(string configString)
        {
            var blacklist = new List<string>();
            var items = configString.Split(',');

            foreach (var item in items)
            {
                blacklist.Add(item);
            }

            return blacklist;
        }

        //--------------------------------- Start No Fog Management ---------------------------------

        public void EnableFogPatch()
        {
            harmony.Patch(original: AccessTools.Method(typeof(Fog), "UpdateShaderVariablesGlobalCBFogParameters"), prefix: new HarmonyMethod(typeof(FogPatch), "Prefix"));
            mls.LogInfo("No Fog enabled successfully.");
        }

        public void DisableFogPatch()
        {
            var method = AccessTools.Method(typeof(Fog), "UpdateShaderVariablesGlobalCBFogParameters");
            harmony.Unpatch(method, HarmonyPatchType.All, modGUID);
            mls.LogInfo("No Fog patch disabled successfully.");
        }
        //--------------------------------- End No Fog Management ---------------------------------

        //--------------------------------- Start Vanilla Management ---------------------------------

        public void EnableVanillaPatches()
        {
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
            mls.LogInfo("Vanilla patches enabled successfully!");
        }

        public void DisableVanillaPatches()
        {
            var method = AccessTools.Method(typeof(AudioReverbTrigger), "changeVolume");
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
            mls.LogInfo("Vanilla patches disabled successfully!");
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
            if (fogParameterChanges.TryGetValue(fogObject, out var vanillaParams))
            {
                var fogComponent = fogObject.GetComponent<LocalVolumetricFog>();
                if (fogComponent != null)
                {
                    fogComponent.parameters = vanillaParams;
                    //mls.LogInfo($"Reverted fog parameters for {fogObject.name} to vanilla.");
                }
                else
                {
                    //mls.LogWarning($"No LocalVolumetricFog component found on {fogObject.name}");
                }
            }
            else
            {
                //mls.LogWarning($"No vanilla parameters found for {fogObject.name} in records");
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
                ApplyFogSettings();
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
                //playerController.disableInteract = true;
                //playerController.disableLookInput = true;
                //playerController.disableMoveInput = true;
                //playerController.inSpecialMenu = true;
                if (playerController.actualClientId == localClientId) // Compare the local client's ID
                {
                    mls.LogInfo($"Local player found: {playerController.name}");
                    return playerController;
                }
            }

            mls.LogError("Local player not found!");
            return null; // Handle cases where no local player is found
        }
    }
    //--------------------------------- End Class ---------------------------------
}
//--------------------------------- End File ---------------------------------
