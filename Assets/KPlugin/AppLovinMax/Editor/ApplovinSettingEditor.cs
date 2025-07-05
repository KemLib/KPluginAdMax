using KTool.FileIo;
using UnityEditor;
using UnityEngine;

namespace KPlugin.AppLovinMax.Editor
{
    public class ApplovinSettingEditor
    {
        #region Properties
        private const string ASSET_APPLOVIN_SETTING_FOLDER_NAME = "Assets/MaxSdk/Resources",
            ASSET_APPLOVIN_SETTING_FILE_NAME = "AppLovinSettings";
        private const string ASSET_APPLOVIN_SETTING_PATH = ASSET_APPLOVIN_SETTING_FOLDER_NAME + "/" + ASSET_APPLOVIN_SETTING_FILE_NAME + ".asset";
        private const string DEFAULT_ANDROID_APP_ID = "ca-app-pub-3940256099942544~3347511713",
            DEFAULT_IOS_APP_ID = "ca-app-pub-3940256099942544~3347511713";

        private SerializedObject serializedApplovin;
        private SerializedProperty propertyQualityServiceEnabled,
            propertySdkKey,
            propertyCustomGradleVersionUrl,
            propertyCustomGradleToolsVersion,
            propertyAdMobAndroidAppId,
            propertyAdMobIOSAppId;
        private bool isShowProperty;
        #endregion

        #region Construction
        public ApplovinSettingEditor()
        {
            SettingObject_Load();
        }
        #endregion

        #region Unity Event
        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("AppLovin Max Setting", "window");
            if (serializedApplovin == null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create Setting"))
                {
                    SettingObject_Create();
                    SettingObject_Load();
                }
                GUILayout.EndHorizontal();
            }
            else
                OnGUI_SerializedApplovin();
            GUILayout.EndVertical();
        }
        private void OnGUI_SerializedApplovin()
        {
            if (isShowProperty)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Hide Setting"))
                {
                    isShowProperty = false;
                }
                GUILayout.EndHorizontal();
                //
                if (!isShowProperty)
                    return;
                OnGUI_PropertyApplovin();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Show Setting"))
                {
                    isShowProperty = true;
                }
                GUILayout.EndHorizontal();
            }
        }
        private void OnGUI_PropertyApplovin()
        {
            serializedApplovin.Update();
            //
            EditorGUILayout.PropertyField(propertyQualityServiceEnabled, new GUIContent("Quality ServiceEnabled"));
            EditorGUILayout.PropertyField(propertySdkKey, new GUIContent("Sdk Key"));
            EditorGUILayout.PropertyField(propertyCustomGradleVersionUrl, new GUIContent("Custom Gradle VersionUrl"));
            EditorGUILayout.PropertyField(propertyCustomGradleToolsVersion, new GUIContent("Custom Gradle Tools Version"));
            //
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(propertyAdMobAndroidAppId, new GUIContent("Android App Id"));
            if (GUILayout.Button("Default ID"))
                propertyAdMobAndroidAppId.stringValue = DEFAULT_ANDROID_APP_ID;
            GUILayout.EndHorizontal();
            //
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(propertyAdMobIOSAppId, new GUIContent("IOS App Id"));
            if (GUILayout.Button("Default ID"))
                propertyAdMobIOSAppId.stringValue = DEFAULT_IOS_APP_ID;
            GUILayout.EndHorizontal();
            //
            serializedApplovin.ApplyModifiedProperties();
        }
        #endregion

        #region Method
        private void SettingObject_Load()
        {
            ScriptableObject scriptable = AssetDatabase.LoadAssetAtPath<ScriptableObject>(ASSET_APPLOVIN_SETTING_PATH);
            if (scriptable == null)
            {
                serializedApplovin = null;
                propertyQualityServiceEnabled = null;
                propertySdkKey = null;
                propertyCustomGradleVersionUrl = null;
                propertyCustomGradleToolsVersion = null;
                propertyAdMobAndroidAppId = null;
                propertyAdMobIOSAppId = null;
                return;
            }
            serializedApplovin = new SerializedObject(scriptable);
            propertyQualityServiceEnabled = serializedApplovin.FindProperty("qualityServiceEnabled");
            propertySdkKey = serializedApplovin.FindProperty("sdkKey");
            propertyCustomGradleVersionUrl = serializedApplovin.FindProperty("customGradleVersionUrl");
            propertyCustomGradleToolsVersion = serializedApplovin.FindProperty("customGradleToolsVersion");
            propertyAdMobAndroidAppId = serializedApplovin.FindProperty("adMobAndroidAppId");
            propertyAdMobIOSAppId = serializedApplovin.FindProperty("adMobIosAppId");
        }
        private void SettingObject_Create()
        {
            if (!AssetFinder.Exists(ASSET_APPLOVIN_SETTING_FOLDER_NAME))
            {
                AssetFinder.CreateFolder(ASSET_APPLOVIN_SETTING_FOLDER_NAME);
                AssetDatabase.Refresh();
            }
            ScriptableObject scriptable = ScriptableObject.CreateInstance("AppLovinSettings");
            AssetDatabase.CreateAsset(scriptable, ASSET_APPLOVIN_SETTING_PATH);
        }
        #endregion
    }
}
