using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// Made this patch to disable fog density changes based on position. For example, fog would get undesirably thicker on Rend when leaving the ship.

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(AudioReverbTrigger))]
    public class AudioReverbTriggerPatch
    {
        // Patch the changeVolume method
        [HarmonyPatch("changeVolume")]
        [HarmonyPrefix]
        public static bool changeVolumePrefix(ref IEnumerator __result, AudioSource aud, float changeVolumeTo, AudioReverbTrigger __instance)
        {
            BetterFog.mls.LogMessage("AudioReverbTrigger changeVolume");   
            // Access fields from the AudioReverbTrigger instance
            var localFog = __instance.localFog;
            var fogEnabledAmount = __instance.fogEnabledAmount;
            var toggleLocalFog = __instance.toggleLocalFog;
            var playerScript = __instance.playerScript;

            // Replace the original coroutine with a custom one
            __result = CustomChangeVolumeCoroutine(aud, changeVolumeTo, localFog, fogEnabledAmount, toggleLocalFog, playerScript);

            // Return false to skip the original method
            return false;
        }

        // Custom coroutine to replace the original changeVolume
        private static IEnumerator CustomChangeVolumeCoroutine(AudioSource aud, float changeVolumeTo, LocalVolumetricFog localFog, float fogEnabledAmount, bool toggleLocalFog, PlayerControllerB playerScript)
        {
            if (localFog != null)
            {
                float fogTarget = fogEnabledAmount;
                if (!toggleLocalFog)
                {
                    //fogTarget = 200f; // Custom fog target... COMMENTED OUT ON PURPOSE TO REMOVE FOG DENSITY CHANGES
                }

                for (int j = 0; j < 40; j++)
                {
                    aud.volume = Mathf.Lerp(aud.volume, changeVolumeTo, (float)j / 40f);
                    //localFog.parameters.meanFreePath = Mathf.Lerp(localFog.parameters.meanFreePath, fogTarget, (float)j / 40f);... COMMENTED OUT ON PURPOSE TO REMOVE FOG DENSITY CHANGES
                    yield return new WaitForSeconds(0.004f);
                }
            }
            else
            {
                for (int j = 0; j < 40; j++)
                {
                    aud.volume = Mathf.Lerp(aud.volume, changeVolumeTo, (float)j / 40f);
                    yield return new WaitForSeconds(0.004f);
                }
            }

            // Remove the coroutine references after completion
            playerScript.audioCoroutines.Remove(aud);
            playerScript.audioCoroutines2.Remove(aud);
        }
    }
}
