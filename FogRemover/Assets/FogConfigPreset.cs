using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogRemover.Assets
{
    public class FogConfigPreset
    {
        public float MeanFreePath { get; set; }
        public float AlbedoR { get; set; }
        public float AlbedoG { get; set; }
        public float AlbedoB { get; set; }
        public float AlbedoA { get; set; }
        public float Anisotropy { get; set; }
        public float GlobalLightProbeDimmer { get; set; }

        public FogConfigPreset(
            float meanFreePath, float albedoR, float albedoG,
            float albedoB, float albedoA, float anisotropy,
            float globalLightProbeDimmer)
        {
            MeanFreePath = meanFreePath;
            AlbedoR = albedoR;
            AlbedoG = albedoG;
            AlbedoB = albedoB;
            AlbedoA = albedoA;
            Anisotropy = anisotropy;
            GlobalLightProbeDimmer = globalLightProbeDimmer;
        }
    }

}
