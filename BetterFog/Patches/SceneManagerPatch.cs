using HarmonyLib;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Reflection; // Add this to access method info

namespace BetterFog.Patches
{
    [HarmonyPatch]
    public class SceneManagerPatch
    {
        // Method to explicitly specify the overloaded LoadScene method
        static MethodBase TargetMethod()
        {
            // We are targeting LoadScene(string, LoadSceneParameters)
            return typeof(SceneManager).GetMethod("LoadScene", new[] { typeof(string), typeof(LoadSceneParameters) });
        }

        // Postfix patch to run after LoadScene is called
        [HarmonyPostfix]
        public static void LoadScenePatch(string sceneName, LoadSceneParameters parameters)
        {
            BetterFog.CollectVanillaValues();
        }
    }
}
