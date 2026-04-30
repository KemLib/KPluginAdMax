using UnityEditor;
using UnityEngine;

namespace KPlugin.AdMax.Editor
{
    [CustomEditor(typeof(AdMaxSetting))]
    public class AdMaxSettingEditor : UnityEditor.Editor
    {
        #region Properties
        public const string ASSET_AD_MAX_SETTING_FOLDER_NAME = "Assets/KPlugin/Resources/" + AdMaxSetting.RESOURCES_PATH_FOLDER,
            ASSET_AD_MAX_SETTING_FILE_NAME = AdMaxSetting.RESOURCES_PATH_FILE;
        public const string ASSET_Ad_Max_SETTING_PATH = ASSET_AD_MAX_SETTING_FOLDER_NAME + "/" + ASSET_AD_MAX_SETTING_FILE_NAME + ".asset";

        private ApplovinSettingEditor applovinSettingEditor;
        private AdMaxSettingAdIdEditor appOpenSetting,
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
            SerializedProperty propertyAppOpenIds = serializedObject.FindProperty("appOpenIds"),
                propertyBannerIds = serializedObject.FindProperty("bannerIds"),
                propertyMrecIds = serializedObject.FindProperty("mrecIds"),
                propertyInterstitialIds = serializedObject.FindProperty("interstitialIds"),
                propertyRewardedIds = serializedObject.FindProperty("rewardedIds"),
                propertyRewardedInterstitialIds = serializedObject.FindProperty("rewardedInterstitialIds"),
                propertyNativeIds = serializedObject.FindProperty("nativeIds");
            appOpenSetting = new AdMaxSettingAdIdEditor(propertyAppOpenIds, AdMaxAdType.AppOpen);
            bannerSetting = new AdMaxSettingAdIdEditor(propertyBannerIds, AdMaxAdType.Banner);
            mrecSetting = new AdMaxSettingAdIdEditor(propertyMrecIds, AdMaxAdType.MRec);
            interstitialSetting = new AdMaxSettingAdIdEditor(propertyInterstitialIds, AdMaxAdType.Interstitial);
            rewardedSetting = new AdMaxSettingAdIdEditor(propertyRewardedIds, AdMaxAdType.Rewarded);
        }
        #endregion
    }
}
