using System;
using HarmonyLib;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using UnityEngine.Rendering;
using FogRemover.Components;
using BepInEx.Configuration;

namespace FogRemover
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class FogRemover : BaseUnityPlugin
    {
        public const string modGUID = "grug.lethalcompany.fogremover";
        public const string modName = "Remove fog";
        public const string modVersion = "0.1.0.0";

        //logger.loginfo(""); to log
        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource mls;
        private static FogRemover instance;
        public static GameObject bruh;
        public static ConfigEntry<bool> disableAll;
        public void Awake()
        {
            disableAll = Config.Bind("General","All fog disable?", false, "This may cause major amounts of lag. use at your own risk");
            mls = base.Logger;
            if (instance == null)
            {
                instance = this;
            }
            if (bruh == null)
            {
                bruh = new GameObject("FogRemoverHolder");
                DontDestroyOnLoad(bruh);
                bruh.hideFlags = HideFlags.HideAndDontSave;
                bruh.AddComponent<RemoveFog>();
            }

        }
    }
}
