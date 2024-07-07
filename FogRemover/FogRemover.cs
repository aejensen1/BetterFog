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

        public static Dictionary<string, FogConfigPreset> FogConfigPresets;

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

            // Initialize config entries only if they are null
            meanFreePath = Config.Bind("Fog Settings", "Mean Free Path", 100f, "Controls the distance at which the fog starts to fade in (in meters).");
            //baseHeight = Config.Bind("Fog Settings", "Base Height", 0f, "Sets the height above ground where the fog starts.");
            //maximumHeight = Config.Bind("Fog Settings", "Maximum Height", 10f, "Sets the maximum height of the fog above the base height.");
            albedoR = Config.Bind("Fog Settings", "Albedo Red", 1f, "Controls the red color component of the fog.");
            albedoG = Config.Bind("Fog Settings", "Albedo Green", 1f, "Controls the green color component of the fog.");
            albedoB = Config.Bind("Fog Settings", "Albedo Blue", 1f, "Controls the blue color component of the fog.");
            albedoA = Config.Bind("Fog Settings", "Albedo Alpha", 1f, "Controls the transparency of the fog.");
            anisotropy = Config.Bind("Fog Settings", "Anisotropy", 0f, "Controls the directional anisotropy of the fog.");
            globalLightProbeDimmer = Config.Bind("Fog Settings", "Global Light Probe Dimmer", 1f, "Controls the brightness of light probes affected by fog.");

            // Initialize your FogConfigPresets dictionary
            // Arguments: MeanFreePath, BaseHeight, MaximumHeight, AlbedoR, AlbedoG, AlbedoB, AlbedoA, Anisotropy, GlobalLightProbeDimmer
            FogConfigPresets = new Dictionary<string, FogConfigPreset>
            {
                { "Default", new FogConfigPreset(100f, 1f, 1f, 1f, 1f, 0f, 1f) },
                { "HeavyFog", new FogConfigPreset(50f, 0.8f, 0.8f, 0.8f, 1f, 0.5f, 0.5f) },
                { "LightFog", new FogConfigPreset(150f, 1f, 1f, 1f, 0.5f, 0.2f, 1.2f) },
                { "RedFog", new FogConfigPreset(9800f, 67f, 24f, 24f, 0.8f, 0f, 1000f) },
                { "NoFog", new FogConfigPreset(10000f, 1f, 1f, 1f, 1f, 0f, 1f) }
            };

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

        // Method to adjust fog settings based on the y-coordinate
        public static void AdjustFogBasedOnYCoordinate(float yCoordinate)
        {
            // Adjust the fog properties using the y-coordinate
            baseHeight = yCoordinate;  // Assuming baseHeight is the property to be adjusted
            Debug.Log($"Fog base height adjusted to: {yCoordinate}");
        }
    }
}
