using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using FogRemover.Components;
using BepInEx.Configuration;

namespace FogRemover.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch
    {
        //public static void Postfix()
        //{
        //    StartGame_MyPatch();
            //ShipHasLeft_MyPatch();
        //}

        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        public static void StartGame_MyPatch()
        {
            FogRemover.mls.LogInfo("Game has started - FOG");
            if (GameNetworkManager.Instance.gameHasStarted)
            {
                FogRemover.mls.LogInfo("Game has started2 - FOG");
                // Ensure newFog is initialized before proceeding
                if (FogRemover.NewFog == null)
                {
                    Debug.LogError("newFog is not initialized(main).");
                    return;
                }

                // Proceed with the logic that requires newFog
                Debug.Log("newFog is initialized and ready to use(main).");

                // Ensure newFog is initialized before proceeding
                if (FogRemover.Bruh == null)
                {
                    Debug.LogError("bruh is not initialized(main).");
                    return;
                }

                // Proceed with the logic that requires newFog
                Debug.Log("bruh is initialized and ready to use(main).");

                // Trigger the RemoveFog component functions
                FogRemover.Bruh.GetComponent<RemoveFog>().TerminateOldFog();
            }
        }

        [HarmonyPatch("ShipHasLeft")]
        [HarmonyPostfix]
        public static void ShipHasLeft_MyPatch()
        {
            FogRemover.Bruh.GetComponent<RemoveFog>().TerminateFogObjects();
            Debug.Log("Destroying all fog objects - ship has left moon");
        }
    }
}
