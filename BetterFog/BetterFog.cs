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

namespace BetterFog
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class BetterFog : BaseUnityPlugin
    {
        public const string modGUID = "ironthumb.BetterFog";
        public const string modName = "BetterFog";
        public const string modVersion = "3.2.4";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource mls;
        private static BetterFog instance;

        public static bool hotkeysEnabled = true;
        private static ConfigEntry<string> nextPresetHotkeyConfig;
        public static ConfigEntry<bool> nextHotKeyEnabled;
        private static ConfigEntry<string> refreshPresetHotkeyConfig;
        public static ConfigEntry<bool> refreshHotKeyEnabled;
        public static ConfigEntry<string> weatherScaleHotkeyConfig;
        public static ConfigEntry<bool> weatherScaleHotKeyEnabled;

        private static ConfigEntry<bool> applyToFogExclusionZone;
        private static ConfigEntry<bool> noFogEnabled;
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
            // Arguments: Preset Name, MeanFreePath, AlbedoR, AlbedoG, AlbedoB, AlbedoA, NoFog
            FogConfigPresets = new List<FogConfigPreset>
            {
                new FogConfigPreset("Default", 250f, 0.441f, 0.459f, 0.500f, false),
                new FogConfigPreset("Heavy Fog", 50f, 0f, 0f, 0f, false),
                new FogConfigPreset("Light Fog", 9850f, 5f, 5f, 5f, false),
                new FogConfigPreset("Red Fog", 500f, 20f, 0f, 0f, false),
                new FogConfigPreset("Orange Fog", 500f, 20f, 9.33f, 4.5f, false),
                new FogConfigPreset("Yellow Fog", 1300f, 20f, 20f, 0f, false),
                new FogConfigPreset("Green Fog", 1300f, 0f, 20f, 0f, false),
                new FogConfigPreset("Blue Fog", 1300f, 37f, 155f, 218f, false),
                new FogConfigPreset("Purple Fog", 500f, 11.5f, 7.2f, 20f, false),
                new FogConfigPreset("Pink Fog", 500f, 20f, 4.75f, 20f, false),
                new FogConfigPreset("No Fog", 10000000f, 1f, 1f, 1f, true)
            };
            mls.LogInfo("FogConfigPresets initialized.");

            // Config bindings below
            // Bind each preset to the config
            string section1 = "Default Fog Preset";
            defaultPresetName =
                Config.Bind(section1, "Default Preset Name", "Default", "Name of the default fog preset (No value sets default to first in list).\n" +
                "Order of settings: Preset Name, Mean Free Path, Albedo Red, Albedo Green, Albedo Blue, NoFog\n" +
                "Mean Free Path - Density of fog. The greater the number, the less dense. 50000 is max (less fog) and 0 is min (more fog).\n" +
                "Albedo Color - Color of fog. 255 is max and 0 is min.\n" +
                "No Fog - Density is negligible, so no fog appears when set to true.\n");

            string section2 = "Key Bindings";
            nextPresetHotkeyConfig = Config.Bind(section2, "Next Preset Hotkey", "n", "Hotkey to switch to the next fog preset.");
            nextHotKeyEnabled = Config.Bind(section2, "Enable Next Hotkey", true, "Enable or disable hotkeys for switching fog presets.");
            refreshPresetHotkeyConfig = Config.Bind(section2, "Refresh Hotkey", "r", "Hotkey to refresh fog settings.");
            refreshHotKeyEnabled = Config.Bind(section2, "Enable Refresh Hotkey", true, "Enable or disable hotkey for refreshing fog settings.");
            weatherScaleHotkeyConfig = Config.Bind(section2, "Weather Scale Hotkey", "c", "Hotkey to toggle weather scaling.");
            weatherScaleHotKeyEnabled = Config.Bind(section2, "Enable Weather Sale Hotkey", false, "Enable or disable hotkey for Weather Scaling toggle.");

            string section3 = "Fog Settings";
            applyToFogExclusionZone = Config.Bind(section3, "Apply to Fog Exclusion Zone", false, "Apply fog settings to the Fog Exclusion Zone (eg. inside of ship).");
            noFogEnabled = Config.Bind(section3, "No Fog Enabled Default", false, "Set value to true to enable No Fog by default.");
            weatherScaleEnabled = Config.Bind(section3, "Weather Scale Enabled Default", true, "Enable weather scaling for fog presets.");
            guiEnabled = Config.Bind(section3, "GUI Enabled", true, "Enable or disable the GUI for the mod.");

            // Initialize the key bindings with the hotkey value
            IngameKeybinds.Instance.InitializeKeybindings(nextPresetHotkeyConfig.Value, refreshPresetHotkeyConfig.Value, weatherScaleHotkeyConfig.Value);

            // Create config entries for each preset
            presetEntries = new ConfigEntry<string>[FogConfigPresets.Count];
            for (int i = 0; i < FogConfigPresets.Count; i++)
            {
                var preset = FogConfigPresets[i];
                //presetEntries[i] = Config.Bind("Fog Presets", preset.PresetName, preset.ToString(), $"Preset {preset.PresetName}");
                presetEntries[i] = Config.Bind("Fog Presets", "Preset " + i, preset.ToString(), $"Preset {preset.PresetName}");
            }

            string section4 = "Weather Scales";
            moonScalesConfig = Config.Bind(section4, "MoonScales", "71 Gordion=1,41 Experimentation=0.95,220 Assurance=0.9,56 Vow=0.8,21 Offense=0.9," +
                "61 March=0.75,20 Adamance=0.75,85 Rend=0.285,7 Dine=0.325,8 Titan=0.285,68 Artifice=0.9,5 Embrion=0.85,44 Liquidation=0.85",
                "Moon scales in the format {Gordion=1,41 Experimentation=0.95,220 Assurance=0.9,...} Moon Scales are applied before weather fog density scales.");

            MoonScales = ParseMoonScales(moonScalesConfig.Value);

            weatherScalesConfig = Config.Bind(section4, "WeatherScales", "none=1,rainy=0.75,stormy=0.5,foggy=0.45,eclipsed=0.77,dust clouds=0.8,flooded=0.765",
            "Weather scales in the format {none=1,rainy=0.69,stormy=0.65,...} Weather Scales are applied after moon fog density scales.");

            WeatherScales = ParseWeatherScales(weatherScalesConfig.Value);

            mls.LogInfo("Finished Parsing");

            if (defaultPresetName == null) // If no default preset is set, use the first preset in the list
            {
                currentPreset = FogConfigPresets[0];
                currentPresetIndex = 0;
                //mls.LogInfo($"Default preset not found. Using the first preset in the list: {currentPreset.PresetName}");
            }
            else // Otherwise, find the preset with the default name
            {
                try
                {
                    // Attempt to find the preset with the default name
                    currentPreset = FogConfigPresets.Find(preset => preset.PresetName == defaultPresetName.Value);
                    currentPresetIndex = FogConfigPresets.IndexOf(currentPreset);
                    //mls.LogInfo($"Default preset found: {currentPreset.PresetName}");
                }
                catch (Exception ex)
                { // If the preset is not found, log an error and use the first preset in the list
                    mls.LogError($"Failed to find the default preset: {ex}");
                    currentPreset = FogConfigPresets[0];
                    currentPresetIndex = 0;
                }
            }
            currentPreset.NoFog = noFogEnabled.Value;
            isDensityScaleEnabled = weatherScaleEnabled.Value;

            // Apply the Harmony patches
            try
            {
                harmony.PatchAll(typeof(StartOfRoundPatch).Assembly);
                mls.LogInfo("StartOfRound patches applied successfully.");

                harmony.PatchAll(typeof(QuickMenuManagerPatch).Assembly);
                mls.LogInfo("QuickMenuManager patches applied successfully.");

                harmony.PatchAll(typeof(IngamePlayerSettingsPatch).Assembly);
                mls.LogInfo("IngamePlayerSettings patches applied successfully.");

                harmony.PatchAll(typeof(EntranceTeleportPatch).Assembly);
                mls.LogInfo("EntranceTeleport patches applied successfully.");

                harmony.PatchAll(typeof(TerminalPatch).Assembly);
                mls.LogInfo("Terminal patches applied successfully.");

                harmony.PatchAll(typeof(AudioReverbTriggerPatch).Assembly);
                mls.LogInfo("AudioReverb patches applied successfully.");

                //harmony.PatchAll(typeof(FogPatch).Assembly);
                //mls.LogInfo("Fog patches applied successfully.");
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
                    if (currentPreset.NoFog)
                    {
                        parameters.meanFreePath = 10000000f;
                        parameters.albedo = new Color(1f, 1f, 1f, 0f);
                    }
                    else
                    {
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
                    }
                    //parameters.anisotropy = currentPreset.NoFog;

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
            //    $"Albedo RGBA: ({currentPreset.AlbedoR}, {currentPreset.AlbedoG}, {currentPreset.AlbedoB}, {currentPreset.AlbedoA})\n" +
            //    $"Anisotropy: {currentPreset.Anisotropy}\n");
        }

        public static void ApplyFogSettingsGradually(float duration, float interval) // Duration in seconds, interval in seconds
        {
            applyingFogSettings = true;
            // Start the coroutine to apply fog settings gradually
            Instance.StartCoroutine(ApplyFogSettingsCoroutine(duration, interval));
        }

        private static IEnumerator ApplyFogSettingsCoroutine(float duration, float interval)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                // Apply fog settings at this step
                ApplyFogSettings();

                // Wait for the next interval
                yield return new WaitForSeconds(interval);
                elapsedTime += interval;
            }

            // Final application of settings at the end of the duration
            ApplyFogSettings();
            applyingFogSettings = false;
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
            FogSettingsManager.Instance.UpdateSettingsWithCurrentPreset();
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

        /*
        public static bool loggingCoroutineRunning = false;
        public static void LogMeanFreePath()
        {
            loggingCoroutineRunning = true;
            mls.LogInfo("Starting MeanFreePath logging coroutine.");
            Instance.StartCoroutine(LogMeanFreePathCoroutine());
        }

        //For logging the MeanFreePath value every second
        private static IEnumerator LogMeanFreePathCoroutine()
        {
            mls.LogInfo("MeanFreePath logging coroutine started.");
            while (true)
            {
                var fogObjects = Resources
                .FindObjectsOfTypeAll<LocalVolumetricFog>()
                .ToList();

                // Iterate through each fog object
                foreach (var fogObject in fogObjects)
                {
                    if (fogObject != null && !(fogObject.name == "FogExclusionZone" && applyToFogExclusionZone.Value == false))
                    {
                        var parameters = fogObject.parameters;
                        // Log the current MeanFreePath value
                        mls.LogInfo($"{fogObject.name} MeanFreePath: {parameters.meanFreePath}");

                        // Wait for 1 second before the next log

                    }
                }

                yield return new WaitForSeconds(2f);
                mls.LogInfo("Waiting for game to start...");
            }
        }*/
    }


}

