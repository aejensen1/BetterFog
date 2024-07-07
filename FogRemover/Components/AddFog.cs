using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;

namespace FogRemover.Components
{
    public class AddFog : MonoBehaviour
    {
        private Volume fogVolume;
        private VolumeProfile newFogProfile;
        private Fog fogComponent;
        private float startMeanFreePath;
        private float oscillationRange = 1f; // Adjust as needed
        private float oscillationSpeed = 1f; // Adjust as needed

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
            ApplyFogSettings();

            startMeanFreePath = FogRemover.meanFreePath.Value;

            //FogRemover.mls.LogInfo("Triggering fog oscillation");
            //StartCoroutine(OscillateMeanFreePath());
        }

        void ApplyFogSettings()
        {

            fogComponent.meanFreePath.overrideState = true;
            fogComponent.meanFreePath.value = FogRemover.meanFreePath.Value;

            fogComponent.baseHeight.overrideState = true;
            //fogComponent.baseHeight.value = FogRemover.baseHeight;
            //Vector3 cameraPosition = cameraClass.position;
            fogComponent.baseHeight.value = -10000f;

            fogComponent.maximumHeight.overrideState = true;
            fogComponent.maximumHeight.value = FogRemover.maximumHeight;

            fogComponent.albedo.overrideState = true;
            fogComponent.albedo.value = new Color(
                FogRemover.albedoR.Value,
                FogRemover.albedoG.Value,
                FogRemover.albedoB.Value,
                FogRemover.albedoA.Value
            );

            fogComponent.anisotropy.overrideState = true;
            fogComponent.anisotropy.value = FogRemover.anisotropy.Value;

            fogComponent.globalLightProbeDimmer.overrideState = true;
            fogComponent.globalLightProbeDimmer.value = FogRemover.globalLightProbeDimmer.Value;

            FogRemover.mls.LogInfo($"New fog settings applied: \n" +
                $"Mean Free Path: {fogComponent.meanFreePath.value}\n" +
                $"Base Height: {fogComponent.baseHeight.value}\n" +
                $"Maximum Height: {fogComponent.maximumHeight.value}\n" +
                $"Albedo: {fogComponent.albedo.value}\n" +
                $"Anisotropy: {fogComponent.anisotropy.value}\n" +
                $"Global Light Probe Dimmer: {fogComponent.globalLightProbeDimmer.value}");
        }

        IEnumerator OscillateMeanFreePath()
        {
            while (true)
            {
                float elapsedTime = 0f;

                while (elapsedTime < Mathf.PI * 2)
                {
                    elapsedTime += Time.deltaTime * oscillationSpeed;
                    fogComponent.meanFreePath.value = startMeanFreePath + Mathf.Sin(elapsedTime) * oscillationRange;
                    FogRemover.mls.LogInfo("Fog Level Adjusted to: " + fogComponent.meanFreePath.value);
                    yield return null;
                }
            }
        }
    }
}
