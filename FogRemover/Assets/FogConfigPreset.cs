﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogRemover.Assets
{
    public class FogConfigPreset
    {
        public string PresetName { get; set; }
        public float MeanFreePath { get; set; }
        public float AlbedoR { get; set; }
        public float AlbedoG { get; set; }
        public float AlbedoB { get; set; }
        public float AlbedoA { get; set; }
        public float Anisotropy { get; set; }
        public float GlobalLightProbeDimmer { get; set; }

        public FogConfigPreset() { }

        public FogConfigPreset(
            string presetName, float meanFreePath, float albedoR, float albedoG,
            float albedoB, float albedoA, float anisotropy,
            float globalLightProbeDimmer)
        {
            PresetName = presetName;
            MeanFreePath = meanFreePath;
            AlbedoR = albedoR;
            AlbedoG = albedoG;
            AlbedoB = albedoB;
            AlbedoA = albedoA;
            Anisotropy = anisotropy;
            GlobalLightProbeDimmer = globalLightProbeDimmer;
        }

        public override string ToString()
        {
            return $"{PresetName}|{MeanFreePath}|{AlbedoR}|{AlbedoG}|{AlbedoB}|{AlbedoA}|{Anisotropy}|{GlobalLightProbeDimmer}";
        }

        public static FogConfigPreset FromString(string data)
        {
            var parts = data.Split('|');
            if (parts.Length != 8)
                throw new ArgumentException("Invalid preset string format");

            return new FogConfigPreset(
                parts[0],
                float.Parse(parts[1]),
                float.Parse(parts[2]),
                float.Parse(parts[3]),
                float.Parse(parts[4]),
                float.Parse(parts[5]),
                float.Parse(parts[6]),
                float.Parse(parts[7])
            );
        }
    }

}
