using System;

namespace BetterFog.Assets
{
    public class FogConfigPreset
    {
        public string PresetName { get; set; }
        public float MeanFreePath { get; set; }
        public float AlbedoR { get; set; }
        public float AlbedoG { get; set; }
        public float AlbedoB { get; set; }
        //public bool NoFog { get; set; }

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
            //NoFog = noFog;
        }

        public override string ToString()
        {
            return $"PresetName={PresetName},Density={MeanFreePath},Red Hue={AlbedoR},Green Hue={AlbedoG},Blue Hue={AlbedoB}";
        }

        public static FogConfigPreset FromString(string data)
        {
            var parts = data.Split(',');
            if (parts.Length != 6)
                throw new ArgumentException("Invalid preset string format");

            return new FogConfigPreset(
                parts[0].Split('=')[1],
                float.Parse(parts[1].Split('=')[1]),
                float.Parse(parts[2].Split('=')[1]),
                float.Parse(parts[3].Split('=')[1]),
                float.Parse(parts[4].Split('=')[1])
            );
        }
    }

}
