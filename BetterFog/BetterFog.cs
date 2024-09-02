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
        public const string modVersion = "3.1.6";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource mls;
        private static BetterFog instance;

        private static ConfigEntry<string> nextPresetHotkeyConfig;
        public static ConfigEntry<bool> hotKeysEnabled;
        private static ConfigEntry<bool> applyToFogExclusionZone;
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
                new FogConfigPreset("Default", 11000f, 0.441f, 0.459f, 0.500f, 1f, false),
                new FogConfigPreset("Heavy Fog", 10f, 0f, 0f, 0f, 0f, false),
                new FogConfigPreset("Light Fog", 9850f, 5f, 5f, 5f, 0.1f, false),
                new FogConfigPreset("Red Fog", 9800f, 67f, 24f, 24f, 0.5f, false),
                new FogConfigPreset("Orange Fog", 9800f, 228f, 128f, 27f, 0.1f, false),
                new FogConfigPreset("Yellow Fog", 9800f, 183f, 187f, 0f, 0.1f, false),
                new FogConfigPreset("Green Fog", 9800f, 17f, 83f, 38f, 0.1f, false),
                new FogConfigPreset("Blue Fog", 9800f, 37f, 155f, 218f, 0.1f, false),
                new FogConfigPreset("Purple Fog", 9800f, 121f, 40f, 170f, 0.8f, false),
                new FogConfigPreset("Pink Fog", 9800f, 224f, 12f, 219f, 0.8f, false),
                new FogConfigPreset("No Fog", 10000000f, 1f, 1f, 1f, 0f, true)
            };
            mls.LogInfo("FogConfigPresets initialized.");

            // Bind each preset to the config
            string section1 = "Default Fog Preset";
            defaultPresetName =
                Config.Bind(section1, "Default Preset Name", "Default", "Name of the default fog preset (No value sets default to first in list).\n" +
                "Order of settings: Preset Name, Mean Free Path, Albedo Red, Albedo Green, Albedo Blue, Albedo Alpha, NoFog\n" +
                "Mean Free Path - Density of fog. The greater the number, the less dense. 50000 is max (less fog) and 0 is min (more fog).\n" +
                "Albedo Color - Color of fog. 255 is max and 0 is min.\n" +
                "Albedo Alpha - Transparency of colors. 1.0 is max for opaque and 0.0 is min for transparent.\n" +
                "No Fog - Density is negligible, so no fog appears when set to true.\n");

            string section2 = "Key Bindings";
            nextPresetHotkeyConfig = Config.Bind(section2, "Next Preset Hotkey", "n", "Hotkey to switch to the next fog preset.");
            hotKeysEnabled = Config.Bind(section2, "Enable Hotkeys", true, "Enable or disable hotkeys for switching fog presets.");

            string section3 = "Fog Settings";
            applyToFogExclusionZone = Config.Bind(section3, "Apply to Fog Exclusion Zone", false, "Apply fog settings to the Fog Exclusion Zone (eg. inside of ship).");

            // Initialize the key bindings with the hotkey value
            IngameKeybinds.Instance.InitializeKeybindings(nextPresetHotkeyConfig.Value);

            // Create config entries
            presetEntries = new ConfigEntry<string>[FogConfigPresets.Count];
            for (int i = 0; i < FogConfigPresets.Count; i++)
            {
                var preset = FogConfigPresets[i];
                //presetEntries[i] = Config.Bind("Fog Presets", preset.PresetName, preset.ToString(), $"Preset {preset.PresetName}");
                presetEntries[i] = Config.Bind("Fog Presets", "Preset " + i, preset.ToString(), $"Preset {preset.PresetName}");
            }

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
                }
            }

            // Register the keybind for next preset
            IngameKeybinds.Instance.NextPresetHotkey.performed += ctx => NextPreset();

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
                    mls.LogInfo($"Fog Object Details: {fogObject.ToString()}");
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
                        parameters.meanFreePath = currentPreset.MeanFreePath;
                        parameters.albedo = new Color(
                            currentPreset.AlbedoR,
                            currentPreset.AlbedoG,
                            currentPreset.AlbedoB,
                            currentPreset.AlbedoA
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
        }
    }
}

