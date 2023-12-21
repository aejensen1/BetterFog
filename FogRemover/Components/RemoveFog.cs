using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace FogRemover.Components
{
    internal class RemoveFog : MonoBehaviour
    {
        public Volume vol;
        public Fog fog;
        public void Awake()
        {
            FogRemover.mls.LogInfo("Removing fog...");
        }
        public void Update()
        {
            
            if (vol == null)
            {
                foreach (Volume v in FindObjectsOfType<Volume>().ToList())
                {
                    if (v.gameObject.name == "VolumeMain" && !FogRemover.disableAll.Value)
                    {
                        vol = v;
                    }
                    if (FogRemover.disableAll.Value && v != null)
                    {
                        v.sharedProfile.TryGet(out fog);
                        if (fog != null)
                        {
                            fog.active = false;
                            fog.enabled.value = false;
                            fog.enabled.overrideState = false;
                        }
                        vol = v;
                    }
                }
                if (fog == null && vol != null && !FogRemover.disableAll.Value)
                {
                    vol.sharedProfile.TryGet(out fog);
                }
            }
            if (fog != null && !FogRemover.disableAll.Value)
            {
                if (fog.enabled.value)
                {
                    fog.enabled.value = false;
                    fog.enabled.overrideState = false;
                    fog.active = false;
                    FogRemover.mls.LogInfo("Fog removed");
                }
            }
        }
    }
}
