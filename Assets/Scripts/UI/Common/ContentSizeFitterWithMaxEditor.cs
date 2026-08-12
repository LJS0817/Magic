using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(ContentSizeFitterWithMax), true)]
    [CanEditMultipleObjects]
    public class ContentSizeFitterWithMaxEditor : ContentSizeFitterEditor
    {
        SerializedProperty m_MaxWidth;

        SerializedProperty m_MaxHeight;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_MaxWidth = serializedObject.FindProperty("m_MaxWidth");
            m_MaxHeight = serializedObject.FindProperty("m_MaxHeight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_MaxWidth, true);
            EditorGUILayout.PropertyField(m_MaxHeight, true);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}