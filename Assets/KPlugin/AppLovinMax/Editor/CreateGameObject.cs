using UnityEditor;
using UnityEngine;

namespace KPlugin.AppLovinMax.Editor
{
    public class CreateGameObject
    {
        #region Properties
        private const string GAME_OBJECT_NAME_MANAGER = "KPlugin_AppLovinMax_Manager",
            GAME_OBJECT_NAME_APP_OPEN = "KPlugin_AppLovinMax_AppOpen",
            GAME_OBJECT_NAME_BANNER = "KPlugin_AppLovinMax_Banner",
            GAME_OBJECT_NAME_MREC = "KPlugin_AppLovinMax_MRec",
            GAME_OBJECT_NAME_INTERSTITIAL = "KPlugin_AppLovinMax_Interstitial",
            GAME_OBJECT_NAME_REWARDED = "KPlugin_AppLovinMax_Rewarded";
        #endregion

        #region Methods
        [MenuItem("GameObject/KPlugin/AppLovinMax/Create Manager", priority = 0)]
        private static void Create_InitManager()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_MANAGER);
            newGO.AddComponent<AppLovinMaxManager>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/AppLovinMax/Create Ad AppOpen", priority = 1)]
        private static void Create_AdAppOpen()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_APP_OPEN);
            newGO.AddComponent<AppLovinMaxAppOpen>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/AppLovinMax/Create Ad Banner", priority = 2)]
        private static void Create_AdBanner()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_BANNER);
            newGO.AddComponent<AppLovinMaxBanner>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/AppLovinMax/Create Ad MRec", priority = 3)]
        private static void Create_AdMRec()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_MREC);
            newGO.AddComponent<AppLovinMaxMRec>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/AppLovinMax/Create Ad Interstitial", priority = 4)]
        private static void Create_AdInterstitial()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_INTERSTITIAL);
            newGO.AddComponent<AppLovinMaxInterstitial>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/AppLovinMax/Create Ad Rewarded", priority = 5)]
        private static void Create_AdRewarded()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_REWARDED);
            newGO.AddComponent<AppLovinMaxRewarded>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        #endregion
    }
}
