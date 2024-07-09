using System;
using HarmonyLib;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using FogRemover.Components;
using BepInEx.Configuration;
using FogRemover.Patches;
using FogRemover.Assets;
using System.Collections.Generic;
using FogRemover.Input;

namespace FogRemover
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class FogRemover : BaseUnityPlugin
    {
        public const string modGUID = "grug.lethalcompany.fogremover";
        public const string modName = "Remove fog";
        public const string modVersion = "0.1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource mls;
        private static FogRemover instance;

        private static GameObject newFog;
        private static GameObject bruh;

        // Config entries for each fog property
        private static string defaultPresetName = "Default";
        public static ConfigEntry<float> meanFreePath;
        //public static ConfigEntry<float> baseHeight;
        //public static ConfigEntry<float> maximumHeight;
        public static ConfigEntry<float> albedoR;
        public static ConfigEntry<float> albedoG;
        public static ConfigEntry<float> albedoB;
        public static ConfigEntry<float> albedoA;
        public static ConfigEntry<float> anisotropy;
        public static ConfigEntry<float> globalLightProbeDimmer;
        public static float baseHeight;
        public static float maximumHeight = 20000f;

        public List<FogConfigPreset> FogConfigPresets;
        private ConfigEntry<string>[] presetEntries;
        public int currentPresetIndex;
        public static FogConfigPreset currentPreset;

        // Singleton pattern
        public static FogRemover Instance
        {
            get
            {
                if (instance == null)
                {
                    var gameObject = new GameObject("FogRemover");
                    DontDestroyOnLoad(gameObject);
                    instance = gameObject.AddComponent<FogRemover>();
                }
                return instance;
            }
        }

        public static GameObject NewFog
        {
            get
            {
                if (newFog == null)
                {
                    newFog = new GameObject("NewFogHolder");
                    DontDestroyOnLoad(newFog);
                    mls.LogInfo("New Fog GameObject created.");
                    newFog.AddComponent<AddFog>();
                }
                return newFog;
            }
        }

        public static GameObject Bruh
        {
            get
            {
                if (bruh == null)
                {
                    bruh = new GameObject("FogRemoverHolder");
                    DontDestroyOnLoad(bruh);
                    mls.LogInfo("FogRemover GameObject created.");
                    bruh.hideFlags = HideFlags.HideAndDontSave;
                    bruh.AddComponent<RemoveFog>();
                }
                return bruh;
            }
        }

        public static void DestroyFog()
        {
            if (newFog != null)
            {
                Destroy(newFog);
                mls.LogInfo("New Fog GameObject destroyed.");
            }
        }

        /*public void OnDestroy()
        {
            harmony.UnpatchSelf();
        }*/

        public void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this.gameObject);

            //

            // Initialize config entries only if they are null
            // Config entries for each preset
            /*preset1 = 

            presetName = Config.Bind("Fog Settings", "Preset Name", "Default", "Name of the fog preset.");
            meanFreePath = Config.Bind("Fog Settings", "Mean Free Path", 100f, "Controls the distance at which the fog starts to fade in (in meters).");
            //baseHeight = Config.Bind("Fog Settings", "Base Height", 0f, "Sets the height above ground where the fog starts.");
            //maximumHeight = Config.Bind("Fog Settings", "Maximum Height", 10f, "Sets the maximum height of the fog above the base height.");
            albedoR = Config.Bind("Fog Settings", "Albedo Red", 1f, "Controls the red color component of the fog.");
            albedoG = Config.Bind("Fog Settings", "Albedo Green", 1f, "Controls the green color component of the fog.");
            albedoB = Config.Bind("Fog Settings", "Albedo Blue", 1f, "Controls the blue color component of the fog.");
            albedoA = Config.Bind("Fog Settings", "Albedo Alpha", 1f, "Controls the transparency of the fog.");
            anisotropy = Config.Bind("Fog Settings", "Anisotropy", 0f, "Controls the directional anisotropy of the fog.");
            globalLightProbeDimmer = Config.Bind("Fog Settings", "Global Light Probe Dimmer", 1f, "Controls the brightness of light probes affected by fog.");
            */


            // Initialize your FogConfigPresets list
            // Arguments: Preset Name, MeanFreePath, AlbedoR, AlbedoG, AlbedoB, AlbedoA, Anisotropy, GlobalLightProbeDimmer
            FogConfigPresets = new List<FogConfigPreset>
            {
                new FogConfigPreset("Default", 11000f, 0f, 0f, 0f, 0.2f, 0f, 1000f),
                new FogConfigPreset("Heavy Fog", 10f, 0f, 0f, 0f, 0f, 0f, 100000f),
                new FogConfigPreset("Light Fog", 9850f, 5f, 5f, 5f, 0.1f, 0f, 1000f),
                new FogConfigPreset("Red Fog", 9800f, 67f, 24f, 24f, 0.5f, 0f, 1200f),
                new FogConfigPreset("Orange Fog", 9800f, 228f, 128f, 27f, 0.1f, 0f, 1300f),
                new FogConfigPreset("Yellow Fog", 9800f, 183f, 187f, 0f, 0.1f, 0f, 1000f),
                new FogConfigPreset("Green Fog", 9800f, 17f, 83f, 38f, 0.1f, 0f, 1000f),
                new FogConfigPreset("Blue Fog", 9800f, 37f, 155f, 218f, 0.1f, 0f, 1000f),
                new FogConfigPreset("Purple Fog", 9800f, 121f, 40f, 170f, 0.8f, 0f, 1000f),
                new FogConfigPreset("Pink Fog", 9800f, 224f, 12f, 219f, 0.8f, 0f, 1f),
                new FogConfigPreset("No Fog", 1000000f, 1f, 1f, 1f, 0f, 1f, 1f)
            };

            // Bind each preset to the config
            string section1 = "Default Fog Preset";
            Config.Bind(section1, "Default Preset Name", defaultPresetName, "Name of the default fog preset (No value sets preset to first in list.\n " +
                "Order of settings: Preset Name, Mean Free Path, Albedo Red, Albedo Green, Albedo Blue, Albedo Alpha, Anisotropy, Global Light Probe Dimmer");

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

            // Register the keybind for next preset
            IngameKeybinds.Instance.NextPresetHotkey.performed += ctx => NextPreset();

            mls = base.Logger;

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
        }

        void Update()
        {
            // Check for key press (e.g., 'N' key to go to the next preset)
            //if (BepInEx.IInputSystem.GetKeyDown(KeyCode.N))
            //{
            //    NextPreset();
            //}
        }

        void NextPreset()
        {
            mls.LogInfo("Next preset hotkey pressed.");
            int currentPresetIndex = FogConfigPresets.IndexOf(currentPreset);
            if (currentPresetIndex == FogConfigPresets.Count - 1)
            {
                currentPresetIndex = -1; // Reset to the first preset if the last preset is reached
            }
            currentPreset = FogConfigPresets[currentPresetIndex + 1];
            mls.LogInfo($"Current preset: {currentPreset.PresetName}");
            //bruh.GetComponent<RemoveFog>().TerminateFogObjects(); // Remove the old fog objects
            bruh.GetComponent<RemoveFog>().TerminateOldFog(); // Remove the old fog settings and create new fog object
        }

    // Method to adjust fog settings based on the y-coordinate
    public static void AdjustFogBasedOnYCoordinate(float yCoordinate)
        {
            // Adjust the fog properties using the y-coordinate
            baseHeight = yCoordinate;  // Assuming baseHeight is the property to be adjusted
            Debug.Log($"Fog base height adjusted to: {yCoordinate}");
        }
    }
}
