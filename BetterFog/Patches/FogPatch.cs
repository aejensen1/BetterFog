using UnityEngine.Rendering.HighDefinition;
using HarmonyLib;

[HarmonyPatch(typeof(Fog))]
public class FogPatch
{
    [HarmonyPrefix]
    static bool Prefix(HDCamera hdCamera, Fog __instance)
    {
        return false; // Skip the original method
    }
}



