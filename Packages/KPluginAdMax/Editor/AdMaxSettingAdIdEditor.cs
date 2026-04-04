using UnityEditor;
using UnityEngine;

namespace KPlugin.AdMax.Editor
{
    public class AdMaxSettingAdIdEditor
    {
        #region Properties
        private const string TEST_AD_ANDROID_APP_OPEN_ID = "",
            TEST_AD_ANDROID_BANNER_ID = "",
            TEST_AD_ANDROID_MREC_ID = "",
            TEST_AD_ANDROID_INTERSTITIAL_ID = "",
            TEST_AD_ANDROID_REWARDED_ID = "";
        private const string TEST_AD_IOS_APP_OPEN_ID = "",
            TEST_AD_IOS_BANNER_ID = "",
            TEST_AD_IOS_MRECT_ID = "",
            TEST_AD_IOS_INTERSTITIAL_ID = "",
            TEST_AD_IOS_REWARDED_ID = "";

        private SerializedProperty propertyAds;
        private AdMaxAdType adType;
        private bool isShow;
        #endregion

        #region Construction
        public AdMaxSettingAdIdEditor(SerializedProperty propertyAds, AdMaxAdType adType)
        {
            this.propertyAds = propertyAds;
            this.adType = adType;
            //
            isShow = false;
        }
        #endregion

        #region Method
        public void OnInspectorGUI()
        {
            string title = adType.ToString() + " Setting";
            GUILayout.BeginVertical(title, "window");
            OnInspectorGUI_Menu();
            if (isShow)
                OnInspectorGUI_Ids();
            GUILayout.EndVertical();
        }

        private void OnInspectorGUI_Menu()
        {
            GUILayout.BeginHorizontal();
            int count = EditorGUILayout.IntField(new GUIContent("Count"), propertyAds.arraySize);
            if (count != propertyAds.arraySize)
                propertyAds.arraySize = count;
            if (propertyAds.arraySize > 0)
            {
                if (isShow)
                {
                    if (GUILayout.Button("Hide"))
                        isShow = false;
                }
                else
                {
                    if (GUILayout.Button("Show"))
                        isShow = true;
                }
            }
            GUILayout.EndHorizontal();
        }

        private void OnInspectorGUI_Ids()
        {
            for (int i = 0; i < propertyAds.arraySize; i++)
            {
                SerializedProperty propertyAd = propertyAds.GetArrayElementAtIndex(i);
                string title = "Item " + i;
                GUILayout.Space(5);
                GUILayout.BeginVertical(title, "window");
                OnInspectorGUI_Id(propertyAd);
                GUILayout.EndVertical();
            }
        }

        private void OnInspectorGUI_Id(SerializedProperty propertyAd)
        {
            SerializedProperty propertyAndroidId = propertyAd.FindPropertyRelative("androidId"),
                propertyIosId = propertyAd.FindPropertyRelative("iosId");
            //
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(propertyAndroidId, new GUIContent("Android Id"));
            if (GUILayout.Button("Default ID"))
                propertyAndroidId.stringValue = GetAndroidId_Default();
            GUILayout.EndHorizontal();
            //
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(propertyIosId, new GUIContent("Ios Id"));
            if (GUILayout.Button("Default ID"))
                propertyIosId.stringValue = GetIosId_Default();
            GUILayout.EndHorizontal();
        }
        #endregion

        private string GetAndroidId_Default()
        {
            switch (adType)
            {
                case AdMaxAdType.AppOpen:
                    return TEST_AD_ANDROID_APP_OPEN_ID;
                case AdMaxAdType.Banner:
                    return TEST_AD_ANDROID_BANNER_ID;
                case AdMaxAdType.MRec:
                    return TEST_AD_ANDROID_MREC_ID;
                case AdMaxAdType.Interstitial:
                    return TEST_AD_ANDROID_INTERSTITIAL_ID;
                case AdMaxAdType.Rewarded:
                    return TEST_AD_ANDROID_REWARDED_ID;
                default:
                    return string.Empty;
            }
        }
        private string GetIosId_Default()
        {
            switch (adType)
            {
                case AdMaxAdType.AppOpen:
                    return TEST_AD_IOS_APP_OPEN_ID;
                case AdMaxAdType.Banner:
                    return TEST_AD_IOS_BANNER_ID;
                case AdMaxAdType.MRec:
                    return TEST_AD_IOS_MRECT_ID;
                case AdMaxAdType.Interstitial:
                    return TEST_AD_IOS_INTERSTITIAL_ID;
                case AdMaxAdType.Rewarded:
                    return TEST_AD_IOS_REWARDED_ID;
                default:
                    return string.Empty;
            }
        }
    }
}
