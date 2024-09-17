using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using HarmonyLib;
using System;
using UnityEngine.Rendering;

/*[HarmonyPatch(typeof(Fog))]
public class FogPatch
{
    [HarmonyPrefix]
    static bool Prefix(ref ShaderVariablesGlobal cb, HDCamera hdCamera, Fog __instance)
    {
        var enableVolumetricFogField = typeof(Fog).GetField("enableVolumetricFog", BindingFlags.NonPublic | BindingFlags.Instance);
        if (enableVolumetricFogField == null)
        {
            Debug.LogError("enableVolumetricFog field not found");
            return true;
        }

        var enableVolumetricFogValue = enableVolumetricFogField.GetValue(__instance);
        var valueProperty = enableVolumetricFogValue.GetType().GetProperty("value");
        bool enableVolumetricFog = (bool)valueProperty.GetValue(enableVolumetricFogValue);

        var frameSettingsField = typeof(HDCamera).GetField("frameSettings", BindingFlags.NonPublic | BindingFlags.Instance);
        if (frameSettingsField == null)
        {
            Debug.LogError("frameSettings field not found");
            return true;
        }

        var frameSettings = frameSettingsField.GetValue(hdCamera);
        var isEnabledMethod = frameSettings.GetType().GetMethod("IsEnabled", new[] { typeof(FrameSettingsField) });
        bool volumetricsEnabled = (bool)isEnabledMethod.Invoke(frameSettings, new object[] { FrameSettingsField.Volumetrics });

        bool flag = enableVolumetricFog && volumetricsEnabled;

        // Reflection to set fields
        var fields = new[]
        {
            new { Name = "_FogEnabled", Value = 1 },
            new { Name = "_PBRFogEnabled", Value = IsPBRFogEnabled(hdCamera) ? 1 : 0 },
            new { Name = "_EnableVolumetricFog", Value = flag ? 1 : 0 },
            new { Name = "_MaxFogDistance", Value = __instance.maxFogDistance.value },
            new { Name = "_FogColorMode", Value = (float)__instance.colorMode.value },
            new { Name = "_FogColor", Value = new Color(__instance.color.value.r, __instance.color.value.g, __instance.color.value.b, 0f) },
            new { Name = "_MipFogParameters", Value = new Vector4(__instance.mipFogNear.value, __instance.mipFogFar.value, __instance.mipFogMaxMip.value, 0f) },
            new { Name = "_HeightFogBaseScattering", Value = flag ? (Vector4)new LocalVolumetricFogArtistParameters(__instance.albedo.value, __instance.meanFreePath.value, __instance.anisotropy.value).ConvertToEngineData().scattering : Vector4.one * new LocalVolumetricFogArtistParameters(__instance.albedo.value, __instance.meanFreePath.value, __instance.anisotropy.value).ConvertToEngineData().extinction },
            new { Name = "_HeightFogBaseExtinction", Value = new LocalVolumetricFogArtistParameters(__instance.albedo.value, __instance.meanFreePath.value, __instance.anisotropy.value).ConvertToEngineData().extinction },
            new { Name = "_HeightFogExponents", Value = new Vector2(1f / ScaleHeightFromLayerDepth(Mathf.Max(0.01f, __instance.maximumHeight.value - __instance.baseHeight.value)), ScaleHeightFromLayerDepth(Mathf.Max(0.01f, __instance.maximumHeight.value - __instance.baseHeight.value))) },
            new { Name = "_HeightFogBaseHeight", Value = __instance.baseHeight.value - (ShaderConfig.s_CameraRelativeRendering != 0 ? hdCamera.camera.transform.position.y : 0) },
            new { Name = "_GlobalFogAnisotropy", Value = __instance.anisotropy.value },
            new { Name = "_VolumetricFilteringEnabled", Value = ((__instance.denoisingMode.value & FogDenoisingMode.Gaussian) != 0) ? 1 : 0 },
            new { Name = "_FogDirectionalOnly", Value = __instance.directionalLightsOnly.value ? 1 : 0 }
        };

        foreach (var field in fields)
        {
            var fieldInfo = typeof(ShaderVariablesGlobal).GetField(field.Name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(cb, field.Value);
            }
            else
            {
                Debug.LogError($"Field {field.Name} not found.");
            }
        }

        return false;
    }
}*/


[HarmonyPatch(typeof(Fog))]
public class FogPatch
{
    [HarmonyPrefix]
    static bool Prefix(HDCamera hdCamera, Fog __instance)
    {
        // Reflectively access the ShaderVariablesGlobal
        //MethodInfo getShaderVariablesGlobalMethod = typeof(Fog).GetMethod("GetShaderVariablesGlobal", BindingFlags.NonPublic | BindingFlags.Instance);
        //if (getShaderVariablesGlobalMethod == null)
        //{
        //    Debug.LogError("GetShaderVariablesGlobal method not found");
        //    return fa; // Let the original method run if we fail
        //}

        //ShaderVariablesGlobal cb = (ShaderVariablesGlobal)getShaderVariablesGlobalMethod.Invoke(__instance, null);

        // Modify ShaderVariablesGlobal fields using reflection
        //var fogEnabledField = typeof(ShaderVariablesGlobal).GetField("_FogEnabled", BindingFlags.NonPublic | BindingFlags.Instance);
        //if (fogEnabledField != null)
        //{
        //    fogEnabledField.SetValue(cb, 1);
        //}

        // Continue modifying other fields similarly...

        // Optionally, you might need to reflectively call a method to apply the modified ShaderVariablesGlobal back
        /*MethodInfo applyShaderVariablesGlobalMethod = typeof(Fog).GetMethod("ApplyShaderVariablesGlobal", BindingFlags.NonPublic | BindingFlags.Instance);
        if (applyShaderVariablesGlobalMethod != null)
        {
            //applyShaderVariablesGlobalMethod.Invoke(__instance, new object[] { cb });
        }*/

        return false; // Skip the original method
    }
}



