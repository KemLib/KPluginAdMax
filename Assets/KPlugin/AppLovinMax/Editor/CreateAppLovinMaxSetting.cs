using KTool.FileIo;
using UnityEditor;
using UnityEngine;

namespace KPlugin.AppLovinMax.Editor
{
    public static class CreateAppLovinMaxSetting
    {
        #region Properties

        #endregion

        #region Method
        [MenuItem("KPlugin/AppLovinMax/Create Setting")]
        private static void MenuItem_CreateAppLovinMaxSetting()
        {
            AppLovinMaxSetting scriptable = AssetDatabase.LoadAssetAtPath<AppLovinMaxSetting>(AppLovinMaxSettingEditor.ASSET_AppLovinMax_SETTING_PATH);
            if (scriptable == null)
                scriptable = CreateSetting();
            //
            Selection.objects = new Object[] { scriptable };
        }

        public static AppLovinMaxSetting CreateSetting()
        {
            if (!AssetFinder.Exists(AppLovinMaxSettingEditor.ASSET_APPLOVINMAX_SETTING_FOLDER_NAME))
            {
                AssetFinder.CreateFolder(AppLovinMaxSettingEditor.ASSET_APPLOVINMAX_SETTING_FOLDER_NAME);
                AssetDatabase.Refresh();
            }
            AppLovinMaxSetting scriptable = ScriptableObject.CreateInstance<AppLovinMaxSetting>();
            AssetDatabase.CreateAsset(scriptable, AppLovinMaxSettingEditor.ASSET_AppLovinMax_SETTING_PATH);
            return scriptable;
        }
        #endregion
    }
}
