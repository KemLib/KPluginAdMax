using UnityEditor;
using UnityEngine;

namespace KPlugin.AdMax.Editor
{
    public class CreateGameObject
    {
        #region Properties
        private const string GAME_OBJECT_NAME_MANAGER = "KPlugin_AdMax_Manager",
            GAME_OBJECT_NAME_APP_OPEN = "KPlugin_AdMax_AppOpen",
            GAME_OBJECT_NAME_BANNER = "KPlugin_AdMax_Banner",
            GAME_OBJECT_NAME_MREC = "KPlugin_AdMax_MRec",
            GAME_OBJECT_NAME_INTERSTITIAL = "KPlugin_AdMax_Interstitial",
            GAME_OBJECT_NAME_REWARDED = "KPlugin_AdMax_Rewarded";
        #endregion

        #region Methods
        [MenuItem("GameObject/KPlugin/AdMax/Manager", priority = 0)]
        private static void Create_InitManager()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_MANAGER);
            newGO.AddComponent<AdMaxManager>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/AdMax/AppOpen", priority = 1)]
        private static void Create_AdAppOpen()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_APP_OPEN);
            newGO.AddComponent<AdMaxAppOpen>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/AdMax/Banner", priority = 2)]
        private static void Create_AdBanner()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_BANNER);
            newGO.AddComponent<AdMaxBanner>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/AdMax/MRec", priority = 3)]
        private static void Create_AdMRec()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_MREC);
            newGO.AddComponent<AdMaxMRec>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/AdMax/Interstitial", priority = 4)]
        private static void Create_AdInterstitial()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_INTERSTITIAL);
            newGO.AddComponent<AdMaxInterstitial>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/AdMax/Rewarded", priority = 5)]
        private static void Create_AdRewarded()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_REWARDED);
            newGO.AddComponent<AdMaxRewarded>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        #endregion
    }
}
