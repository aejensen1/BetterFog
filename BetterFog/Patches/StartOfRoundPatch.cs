using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.Rendering.HighDefinition;

namespace BetterFog.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch
    {
        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        public static void StartGame_MyPatch()
        {
            BetterFog.mls.LogInfo("Game has started - FOG");
            if (GameNetworkManager.Instance.gameHasStarted)
            {
                BetterFog.ApplyFogSettings();
            }
        }

        [HarmonyPatch("ShipHasLeft")]
        [HarmonyPostfix]
        public static void ShipHasLeft_MyPatch()
        {
            //BetterFog.Bruh.GetComponent<RemoveFog>().TerminateFogObjects();
            //Debug.Log("Destroying all fog objects - ship has left moon");
        }
    }
}
