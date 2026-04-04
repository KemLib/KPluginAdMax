using UnityEditor;
using UnityEngine;

namespace KPlugin.AdMax.Editor
{

    [CustomEditor(typeof(AdMaxInterstitial))]
    public class AdMaxInterstitialEditor : UnityEditor.Editor
    {
        #region Properties
        private SerializedProperty propertyAdName,
            propertyIsAutoReload,
            propertyInitIndispensable,
            propertySetInstance,
            propertyIndexAd;
        #endregion

        #region Methods Unity        
        private void OnEnable()
        {
            propertyAdName = serializedObject.FindProperty("adName");
            propertyIsAutoReload = serializedObject.FindProperty("isAutoReload");
            propertyInitIndispensable = serializedObject.FindProperty("initIndispensable");
            propertySetInstance = serializedObject.FindProperty("setInstance");
            propertyIndexAd = serializedObject.FindProperty("indexAd");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //
            EditorGUILayout.PropertyField(propertyInitIndispensable, new GUIContent("Init Indispensable"));
            EditorGUILayout.PropertyField(propertySetInstance, new GUIContent("Set Instance"));
            EditorGUILayout.PropertyField(propertyAdName, new GUIContent("Ad Name"));
            if (string.IsNullOrEmpty(propertyAdName.stringValue))
            {
                propertyAdName.stringValue = serializedObject.targetObject.name;
            }
            EditorGUILayout.PropertyField(propertyIsAutoReload, new GUIContent("Auto Reload"));
            EditorGUILayout.PropertyField(propertyIndexAd, new GUIContent("Index Ad"));
            //
            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Methods

        #endregion
    }
}
