using HarmonyLib;
using Unity.Netcode;

namespace BetterFog.Patches
{
    
    [HarmonyPatch(typeof(NetworkSceneManager))]
    public class NetworkSceneManagerPatch
    {
        [HarmonyPatch("OnSceneLoaded")]
        [HarmonyPostfix]
        public static void OnSceneLoadedPatch()
        {
            //BetterFog.mls.LogInfo("OnSceneLoaded invoked.");
            

            
            BetterFog.mls.LogInfo("OnSceneLoaded invoked. Applying fog settings to moon.");
            BetterFog.ApplyFogSettingsOnGameStart();

            /*
            if (GameNetworkManager.Instance.gameHasStarted)
            {
                 Applying fog settings to moon.");
            }
            else
            {
                BetterFog.mls.LogInfo("OnSceneLoaded invoked. Game has not started yet. Skipping fog settings.");
                BetterFog.mls.LogInfo(GameNetworkManager.Instance.gameHasStarted);
            }*/
        }

        /*
        [HarmonyPatch("SceneManager_ActiveSceneChanged")]
        [HarmonyPostfix]
        public static void SceneManager_ActiveSceneChangedPatch()
        {
            //if (GameNetworkManager.Instance.gameHasStarted)
            //{
            BetterFog.mls.LogInfo("SceneManager_ActiveSceneChanged invoked. Applying fog settings to moon.");


                // Start applying fog settings gradually
                //if (!BetterFog.applyingFogSettings)
                //{
                //    BetterFog.ApplyFogSettingsGradually(2f, 0.99f); // Add 2 seconds of fog update delay for when ship lands. May need to change if ship landing time changes.
                //}
            //}
        }

        [HarmonyPatch("OnSceneEventProgressCompleted")]
        [HarmonyPostfix]
        public static void OnSceneEventProgressCompletedPatch()
        {
            //if (GameNetworkManager.Instance.gameHasStarted)
            //{
            BetterFog.mls.LogInfo("OnSceneEventProgressCompleted invoked. Applying fog settings to moon.");
            //}
        }

        [HarmonyPatch("OnClientLoadedScene")]
        [HarmonyPostfix]
        public static void OnClientLoadedScenePatch()
        {
            //if (GameNetworkManager.Instance.gameHasStarted)
            //{
            BetterFog.mls.LogInfo("OnClientLoadedScene invoked. Applying fog settings to moon.");
            //}
        }

        [HarmonyPatch("OnClientSceneLoadingEvent")]
        [HarmonyPostfix]
        public static void OnClientSceneLoadingEventPatch()
        {
            //if (GameNetworkManager.Instance.gameHasStarted)
            //{
            BetterFog.mls.LogInfo("OnClientSceneLoadingEvent invoked. Applying fog settings to moon.");
            //}
        }

        [HarmonyPatch("ClientLoadedSynchronization")]
        [HarmonyPostfix]
        public static void ClientLoadedSynchronizationPatch()
        {
            //if (GameNetworkManager.Instance.gameHasStarted)
            //{
            BetterFog.mls.LogInfo("ClientLoadedSynchronization invoked. Applying fog settings to moon.");
            //}
        }
        */
    }
}
