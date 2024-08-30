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

namespace BetterFog
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class BetterFog : BaseUnityPlugin
    {
        public const string modGUID = "ironthumb.BetterFog";
        public const string modName = "BetterFog";
        public const string modVersion = "3.1.4";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource mls;
        private static BetterFog instance;
        private static FogConfigPreset fogPreset;

        // Config entries for each fog property
        private static string defaultPresetName = "Default";
        public static ConfigEntry<float> meanFreePath;
        public static ConfigEntry<float> albedoR;
        public static ConfigEntry<float> albedoG;
        public static ConfigEntry<float> albedoB;
        public static ConfigEntry<float> albedoA;
        public static ConfigEntry<float> anisotropy;

        public static List<FogConfigPreset> FogConfigPresets;
        private ConfigEntry<string>[] presetEntries;
        public static int currentPresetIndex;
        public static FogConfigPreset currentPreset;

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
            // Arguments: Preset Name, MeanFreePath, AlbedoR, AlbedoG, AlbedoB, AlbedoA, Anisotropy
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
            Config.Bind(section1, "Default Preset Name", defaultPresetName, "Name of the default fog preset (No value sets default to first in list).\n" +
                "Order of settings: Preset Name, Mean Free Path, Albedo Red, Albedo Green, Albedo Blue, Albedo Alpha, Anisotropy\n" +
                "Mean Free Path - Density of fog. The greater the number, the less dense. 50000 is max (less fog) and 0 is min (more fog).\n" +
                "Albedo Color - Color of fog. 255 is max and 0 is min.\n" + 
                "Albedo Alpha - Transparency of colors. 1.0 is max for opaque and 0.0 is min for transparent.\n" +
                "No Fog - Density is negligible, so no fog appears when set to true.\n");


            // Create config entries
            presetEntries = new ConfigEntry<string>[FogConfigPresets.Count];
            for (int i = 0; i < FogConfigPresets.Count; i++)
            {
                var preset = FogConfigPresets[i];
                presetEntries[i] = Config.Bind("Fog Presets", preset.PresetName, preset.ToString(), $"Preset {preset.PresetName}");
            }

            if (defaultPresetName == null) // If no default preset is set, use the first preset in the list
            {
                currentPreset = FogConfigPresets[0];
            }
            else // Otherwise, find the preset with the default name
            {
                currentPreset = FogConfigPresets.Find(preset => preset.PresetName == defaultPresetName);
            }
            // Initialize and create FogSettingsManager
            //InitializeFogSettingsManager();

            // Register the keybind for next preset
            IngameKeybinds.Instance.NextPresetHotkey.performed += ctx => NextPreset();

            // Apply the Harmony patches
            try
            {
                harmony.PatchAll(typeof(StartOfRoundPatch).Assembly);
                mls.LogInfo("StartOfRound patches applied successfully.");
            }
            catch (Exception ex)
            {
                mls.LogError($"Failed to apply Harmony patches: {ex}");
                throw; // Rethrow the exception to indicate initialization failure
            }

            try
            {
                harmony.PatchAll(typeof(QuickMenuManagerPatch).Assembly);
                mls.LogInfo("QuickMenuManager patches applied successfully.");
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

        private void InitializeFogSettingsManager()
        {
            try
            {
                if (FogSettingsManager.Instance == null)
                {
                    mls.LogInfo("FogSettingsManager does not exist. Creating new instance.");
                    //var fogSettingsManagerObject = new GameObject("FogSettingsManager");
                    //mls.LogInfo("FogSettingsManager created.");
                    //DontDestroyOnLoad(fogSettingsManagerObject);
                    //mls.LogInfo("FogSettingsManager set to DontDestroyOnLoad.");
                    //fogSettingsManagerObject.AddComponent<FogSettingsManager>();
                    //mls.LogInfo("FogSettingsManager initialized.");
                }
                else
                {
                    mls.LogWarning("FogSettingsManager already exists.");
                }
            }
            catch (Exception ex)
            {
                mls.LogError($"Exception in InitializeFogSettingsManager: {ex.Message}\n{ex.StackTrace}");
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
                if (fogObject != null)
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

        void NextPreset()
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


    }
}
