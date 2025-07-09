using KTool.Advertisement;
using UnityEditor;
using UnityEngine;

namespace KPlugin.AppLovinMax.Editor
{

    [CustomEditor(typeof(AppLovinMaxMRec))]
    public class AppLovinMaxMRecEditor : UnityEditor.Editor
    {
        #region Properties
        private SerializedProperty propertyAdName,
            propertyIsAutoReload,
            propertyAdPosition,
            propertyAdSize,
            propertyPosition,
            propertySize,
            propertySetInstance,
            propertyInitIndispensable,
            propertyIndexAd;
        #endregion

        #region Methods Unity        
        private void OnEnable()
        {
            propertyAdName = serializedObject.FindProperty("adName");
            propertyIsAutoReload = serializedObject.FindProperty("isAutoReload");
            propertyAdPosition = serializedObject.FindProperty("adPosition");
            propertyAdSize = serializedObject.FindProperty("adSize");
            propertyPosition = serializedObject.FindProperty("position");
            propertySize = serializedObject.FindProperty("size");
            propertySetInstance = serializedObject.FindProperty("setInstance");
            propertyInitIndispensable = serializedObject.FindProperty("initIndispensable");
            propertyIndexAd = serializedObject.FindProperty("indexAd");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //
            EditorGUILayout.PropertyField(propertySetInstance, new GUIContent("Set Instance"));
            EditorGUILayout.PropertyField(propertyInitIndispensable, new GUIContent("Init Indispensable"));
            EditorGUILayout.PropertyField(propertyAdName, new GUIContent("Ad Name"));
            if (string.IsNullOrEmpty(propertyAdName.stringValue))
            {
                propertyAdName.stringValue = serializedObject.targetObject.name;
            }
            EditorGUI.BeginDisabledGroup(true);
            if (!propertyIsAutoReload.boolValue)
                propertyIsAutoReload.boolValue = true;
            EditorGUILayout.PropertyField(propertyIsAutoReload, new GUIContent("Auto Reload"));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(propertyIndexAd, new GUIContent("Index Ad"));
            //
            EditorGUILayout.PropertyField(propertyAdPosition, new GUIContent("Ad Position"));
            if (propertyAdPosition.enumValueIndex == (int)AdPosition.Custom)
            {
                EditorGUILayout.PropertyField(propertyPosition, new GUIContent("Position"));
                Vector2 size = propertyPosition.vector2Value;
                propertyPosition.vector2Value = new Vector2(Mathf.Max(0, size.x), Mathf.Max(0, size.y));
            }
            //
            EditorGUI.BeginDisabledGroup(true);
            if (propertyAdSize.enumValueIndex != (int)AdSize.Standard)
                propertyAdSize.enumValueIndex = (int)AdSize.Standard;
            EditorGUILayout.PropertyField(propertyAdSize, new GUIContent("Ad Size"));
            if (propertyAdSize.enumValueIndex == (int)AdSize.Custom)
            {
                EditorGUILayout.PropertyField(propertySize, new GUIContent("Size"));
                Vector2 size = propertySize.vector2Value;
                propertySize.vector2Value = new Vector2(Mathf.Max(0, size.x), Mathf.Max(0, size.y));
            }
            EditorGUI.EndDisabledGroup();
            //
            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Methods

        #endregion
    }
}
