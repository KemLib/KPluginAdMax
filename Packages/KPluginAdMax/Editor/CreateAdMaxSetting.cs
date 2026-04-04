using KTool.FileIo;
using UnityEditor;
using UnityEngine;

namespace KPlugin.AdMax.Editor
{
    public static class CreateAppLovinMaxSetting
    {
        #region Properties

        #endregion

        #region Method
        [MenuItem("KPlugin/AdMax/Create Setting")]
        private static void MenuItem_CreateAppLovinMaxSetting()
        {
            AdMaxSetting scriptable = AssetDatabase.LoadAssetAtPath<AdMaxSetting>(AdMaxSettingEditor.ASSET_Ad_Max_SETTING_PATH);
            if (scriptable == null)
                scriptable = CreateSetting();
            //
            Selection.objects = new Object[] { scriptable };
        }

        public static AdMaxSetting CreateSetting()
        {
            if (!AssetFinder.Exists(AdMaxSettingEditor.ASSET_AD_MAX_SETTING_FOLDER_NAME))
            {
                AssetFinder.CreateFolder(AdMaxSettingEditor.ASSET_AD_MAX_SETTING_FOLDER_NAME);
                AssetDatabase.Refresh();
            }
            AdMaxSetting scriptable = ScriptableObject.CreateInstance<AdMaxSetting>();
            AssetDatabase.CreateAsset(scriptable, AdMaxSettingEditor.ASSET_Ad_Max_SETTING_PATH);
            return scriptable;
        }
        #endregion
    }
}
