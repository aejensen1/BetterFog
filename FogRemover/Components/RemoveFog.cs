using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace FogRemover.Components
{
    public class RemoveFog : MonoBehaviour
    {
        private Volume vol;
        //private Fog fog;
        //private static GameObject fogObject;
        //private static AddFog addFogComponent;

        /*void Update()
        {
            if (vol == null)
            {
                foreach (Volume v in FindObjectsOfType<Volume>())
                {
                    if (v.gameObject.name == "VolumeMain")
                    {
                        vol = v;
                        if (vol.sharedProfile.TryGet(out fog))
                        {
                            fog.active = false;
                            fog.enabled.value = false;
                            fog.enabled.overrideState = false;
                            FogRemover.mls.LogInfo("Old fog settings disabled.");
                            break;
                        }
                    }
                }
            }
        }*/

        public void TerminateOldFog() => StartCoroutine(CheckAndRemoveFog());

        private IEnumerator CheckAndRemoveFog()
        {
            while (true)
            {
                Volume vol = null;
                foreach (Volume v in FindObjectsOfType<Volume>())
                {
                    if (v.gameObject.name == "VolumeMain")
                    {
                        vol = v;
                        break;
                    }
                }

                if (vol != null)
                {
                    FogRemover.mls.LogInfo("VolumeMain found.");
                    if (vol.sharedProfile.TryGet(out Fog fog))
                    {
                        FogRemover.mls.LogInfo("Fog profile found. Disabling old fog settings.");
                        fog.active = false;
                        fog.enabled.value = false;
                        fog.enabled.overrideState = false;
                        FogRemover.mls.LogInfo("Old fog settings disabled.");

                        // Trigger the CreateFog method in AddFog component
                        GameObject fogObject = FogRemover.NewFog;
                        if (fogObject != null)
                        {
                            var addFogComponent = fogObject.GetComponent<AddFog>();
                            if (addFogComponent != null)
                            {
                                addFogComponent.CreateFog();
                                FogRemover.mls.LogInfo("Triggered CreateFog method in AddFog component.");
                            }
                            else
                            {
                                FogRemover.mls.LogError("AddFog component is null. Could not trigger CreateFog.");
                            }
                        }
                        else
                        {
                            FogRemover.mls.LogError("Fog GameObject is null. Could not trigger CreateFog.");
                        }
                        yield break; // Exit the coroutine
                    }
                    else
                    {
                        FogRemover.mls.LogError("Fog profile not found in VolumeMain.");
                    }
                }
                else
                {
                    FogRemover.mls.LogInfo("VolumeMain not found.");
                }

                yield return new WaitForSeconds(1f); // Wait for 1 second before checking again
                FogRemover.mls.LogInfo("Waiting for VolumeMain to be created...");
            }
        }


        public void TerminateFogObjects() => StartCoroutine(CheckAndRemoveFogObjects());

        private IEnumerator CheckAndRemoveFogObjects()
        {
            foreach (Volume v in FindObjectsOfType<Volume>())
            {
                if (v.gameObject.name == "FogRemoverHolder" || v.gameObject.name == "NewFogHolder")
                {
                    vol = v;
                    Destroy(vol.gameObject);
                    FogRemover.mls.LogInfo("Old fog objects destroyed.");
                }
            }
            yield break; // Exit the coroutine
        }
    }
}
