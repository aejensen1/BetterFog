using System;
using System.Globalization;
using static UnityEngine.Rendering.DebugUI;

namespace BetterFog.Assets
{
    public class FogConfigPreset
    {
        public string PresetName { get; set; }
        public float MeanFreePath { get; set; }
        public float AlbedoR { get; set; }
        public float AlbedoG { get; set; }
        public float AlbedoB { get; set; }

        public FogConfigPreset() { }

        public FogConfigPreset(
            string presetName, float meanFreePath, float albedoR, float albedoG,
            float albedoB)
        {
            PresetName = presetName;
            MeanFreePath = meanFreePath;
            AlbedoR = albedoR;
            AlbedoG = albedoG;
            AlbedoB = albedoB;
        }

        // Copy Constructor
        public FogConfigPreset(FogConfigPreset other)
        {
            PresetName = other.PresetName;
            MeanFreePath = other.MeanFreePath;
            AlbedoR = other.AlbedoR;
            AlbedoG = other.AlbedoG;
            AlbedoB = other.AlbedoB;
        }

        public override string ToString()
        {
            // return $"PresetName={PresetName},Density={MeanFreePath},Red Hue={AlbedoR},Green Hue={AlbedoG},Blue Hue={AlbedoB}";
            // Use Invariant Culture in case player is using a different region configuration in their control panel settings. This will normalize float values.
            return $"PresetName={PresetName},Density={MeanFreePath.ToString(CultureInfo.InvariantCulture)},Red Hue={AlbedoR.ToString(CultureInfo.InvariantCulture)},Green Hue={AlbedoG.ToString(CultureInfo.InvariantCulture)},Blue Hue={AlbedoB.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
