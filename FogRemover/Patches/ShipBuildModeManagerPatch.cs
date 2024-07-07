using HarmonyLib;
using UnityEngine;
using FogRemover.Components;
using Unity.Netcode;

namespace FogRemover.Patches
{
    [HarmonyPatch(typeof(ShipBuildModeManager))]
    public class ShipBuildModeManagerPatch
    {
        // Patch the PlaceShipObjectServerRpc method
        [HarmonyPatch("PlaceShipObject")]
        [HarmonyPostfix]
        public static void PlaceShipObjectPatch(Vector3 placementPosition, Vector3 placementRotation, PlaceableShipObject placeableObject, bool placementSFX = true)
        {
            // Extract the y-coordinate from the newPosition parameter
            float shipLandingYCoordinate = placementPosition.y;
            FogRemover.mls.LogInfo("Ship has been placed at y-coordinate: " + shipLandingYCoordinate);

            // Adjust fog settings based on the y-coordinate
            FogRemover.AdjustFogBasedOnYCoordinate(shipLandingYCoordinate);
        }
    }
}
