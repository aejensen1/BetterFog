using HarmonyLib;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace BetterFog.Patches
{
    // Harmony patch to intercept the ConvertToEngineData method
    [HarmonyPatch(typeof(LocalVolumetricFogArtistParameters), "ConvertToEngineData")]
    public static class LocalVolumetricFogArtistParametersPatch
    {
        public static void ConvertToEngineDataPatch(ref LocalVolumetricFogArtistParameters __instance)
        {
            BetterFog.mls.LogInfo("LocalVolumetricFogArtistParameters ConvertToEngineDataPatch");
            // Find the fog object associated with these parameters (you need to implement this based on context)
            GameObject fogObject = FindFogObject(__instance);

            BetterFog.mls.LogInfo("Test");
            if (fogObject != null)
            {
                
                BetterFog.mls.LogInfo($"Fog object {fogObject}");
                // Check if this fog object has already been added to the dictionary
                if (!BetterFog.fogParameterChanges.ContainsKey(fogObject))
                {
                    BetterFog.mls.LogInfo("Adding fog object to dictionary");
                    BetterFog.fogParameterChanges.Add(fogObject, __instance);
                    Debug.Log($"Fog parameters for {fogObject.name} have been added. MeanFreePath: {__instance.meanFreePath}");
                }
                else
                {
                    BetterFog.mls.LogInfo("Fog object already in dictionary.");
                }
            }
            else
            {
                BetterFog.mls.LogInfo("No matching fog object found.");
            }
        }

        // Implement your logic to find the GameObject associated with these fog parameters
        private static GameObject FindFogObject(LocalVolumetricFogArtistParameters parameters)
        {
            // Find all LocalVolumetricFog components in the scene
            var fogObjects = Resources.FindObjectsOfTypeAll<LocalVolumetricFog>().ToList();
            BetterFog.mls.LogInfo($"Searching for object.");
            // Here you would implement logic to match the parameters to the corresponding fog object.
            // This could involve checking properties, names, or any identifying characteristics.
            foreach (var fogObject in fogObjects)
            {
                if (fogObject.parameters.Equals(parameters)) // Assuming artistParameters links back
                {
                    BetterFog.mls.LogInfo($"Found object {fogObject.gameObject}");
                    return fogObject.gameObject;
                }
            }

            // If no match found
            return null;
        }
    }
}
