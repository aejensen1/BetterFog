using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;
using FogRemover.Assets;

namespace FogRemover.Components
{
    public class AddFog : MonoBehaviour
    {
        private Volume fogVolume;
        private VolumeProfile newFogProfile;
        private Fog fogComponent;
        private FogConfigPreset fogPreset;

        public void CreateFog()
        {
            FogRemover.mls.LogInfo("Creating new fog settings");
            // Create a new Volume Profile for new fog settings
            newFogProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            FogRemover.mls.LogInfo($"New Volume Profile created: {newFogProfile.name}");

            // Add Fog component to the new profile
            fogComponent = newFogProfile.Add<Fog>();
            fogComponent.active = true;

            // Create a new Volume GameObject and set the new profile
            fogVolume = gameObject.AddComponent<Volume>();
            fogVolume.isGlobal = true;
            fogVolume.sharedProfile = newFogProfile;

            // Apply settings from config
            fogPreset = FogRemover.currentPreset;
            ApplyFogSettings();
        }

        // Apply supplied fog preset settings from config
        void ApplyFogSettings()
        {
            fogComponent.meanFreePath.overrideState = true;
            fogComponent.meanFreePath.value = fogPreset.MeanFreePath;

            fogComponent.albedo.overrideState = true;
            fogComponent.albedo.value = new Color(
                fogPreset.AlbedoR,
                fogPreset.AlbedoG,
                fogPreset.AlbedoB,
                fogPreset.AlbedoA
            );

            fogComponent.anisotropy.overrideState = true;
            fogComponent.anisotropy.value = fogPreset.Anisotropy;

            fogComponent.globalLightProbeDimmer.overrideState = true;
            fogComponent.globalLightProbeDimmer.value = fogPreset.GlobalLightProbeDimmer;

            FogRemover.mls.LogInfo($"New fog settings applied from preset " + fogPreset.PresetName + ": \n" +
                $"Mean Free Path: {fogComponent.meanFreePath.value}\n" +
                $"Base Height: {fogComponent.baseHeight.value}\n" +
                $"Maximum Height: {fogComponent.maximumHeight.value}\n" +
                $"Albedo: {fogComponent.albedo.value}\n" +
                $"Anisotropy: {fogComponent.anisotropy.value}\n" +
                $"Global Light Probe Dimmer: {fogComponent.globalLightProbeDimmer.value}");
        }
    }
}
