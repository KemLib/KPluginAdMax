using UnityEditor;
using UnityEngine;

namespace KPlugin.AppLovinMax.Editor
{
    [CustomEditor(typeof(AppLovinMaxSetting))]
    public class AppLovinMaxSettingEditor : UnityEditor.Editor
    {
        #region Properties
        public const string ASSET_APPLOVINMAX_SETTING_FOLDER_NAME = "Assets/KPlugin/Resources/" + AppLovinMaxSetting.RESOURCES_PATH_FOLDER,
            ASSET_APPLOVINMAX_SETTING_FILE_NAME = AppLovinMaxSetting.RESOURCES_PATH_FILE;
        public const string ASSET_AppLovinMax_SETTING_PATH = ASSET_APPLOVINMAX_SETTING_FOLDER_NAME + "/" + ASSET_APPLOVINMAX_SETTING_FILE_NAME + ".asset";

        private ApplovinSettingEditor applovinSettingEditor;
        private SerializedProperty propertyUserId;
        private AdIdSettingEditor appOpenSetting,
            bannerSetting,
            mrecSetting,
            interstitialSetting,
            rewardedSetting;
        #endregion

        #region Unity Event
        private void OnEnable()
        {
            Init();
        }

        public override void OnInspectorGUI()
        {
            OnGui_AppLovinSetting();
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(propertyUserId, new GUIContent("User Id"));
            GUILayout.Space(5);
            OnGui_AppLovinMaxSetting();
        }
        private void OnGui_AppLovinSetting()
        {
            applovinSettingEditor.OnInspectorGUI();
        }
        private void OnGui_AppLovinMaxSetting()
        {
            serializedObject.Update();
            //
            appOpenSetting.OnInspectorGUI();
            GUILayout.Space(5);
            bannerSetting.OnInspectorGUI();
            GUILayout.Space(5);
            mrecSetting.OnInspectorGUI();
            GUILayout.Space(5);
            interstitialSetting.OnInspectorGUI();
            GUILayout.Space(5);
            rewardedSetting.OnInspectorGUI();
            //
            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Method
        private void Init()
        {
            applovinSettingEditor = new ApplovinSettingEditor();
            propertyUserId = serializedObject.FindProperty("userId");
            SerializedProperty propertyAppOpenIds = serializedObject.FindProperty("appOpenIds"),
                propertyBannerIds = serializedObject.FindProperty("bannerIds"),
                propertyMrecIds = serializedObject.FindProperty("mrecIds"),
                propertyInterstitialIds = serializedObject.FindProperty("interstitialIds"),
                propertyRewardedIds = serializedObject.FindProperty("rewardedIds"),
                propertyRewardedInterstitialIds = serializedObject.FindProperty("rewardedInterstitialIds"),
                propertyNativeIds = serializedObject.FindProperty("nativeIds");
            appOpenSetting = new AdIdSettingEditor(propertyAppOpenIds, AppLovinMaxAdType.AppOpen);
            bannerSetting = new AdIdSettingEditor(propertyBannerIds, AppLovinMaxAdType.Banner);
            mrecSetting = new AdIdSettingEditor(propertyMrecIds, AppLovinMaxAdType.Mrec);
            interstitialSetting = new AdIdSettingEditor(propertyInterstitialIds, AppLovinMaxAdType.Interstitial);
            rewardedSetting = new AdIdSettingEditor(propertyRewardedIds, AppLovinMaxAdType.Rewarded);
        }
        #endregion
    }
}
