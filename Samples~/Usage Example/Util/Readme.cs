namespace GenericScriptableObjects.Usage_Example.Util
{
#if UNITY_EDITOR
    using SolidUtilities.Editor.Helpers;
    using UnityEditor;
#endif
    using UnityEngine;

    internal class Readme : ScriptableObject
    {
        [SerializeField]
        internal string Description = null;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Readme))]
    internal class ReadmeEditor : Editor
    {
        private SerializedProperty _description;
        private GUIStyle _style;

        private GUIStyle Style
        {
            get
            {
                if (_style == null)
                    _style = new GUIStyle(EditorStyles.textField) { wordWrap = true };

                return _style;
            }
        }

        private string Description
        {
            get => _description.stringValue;
            set => _description.stringValue = value;
        }

        public override void OnInspectorGUI()
        {
            string textAreaValue = null;

            if (EditorDrawHelper.CheckIfChanged(() => textAreaValue = EditorGUILayout.TextArea(Description, Style)))
            {
                Description = textAreaValue;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnEnable()
        {
            _description = serializedObject.FindProperty(nameof(Readme.Description));
        }
    }
#endif
}