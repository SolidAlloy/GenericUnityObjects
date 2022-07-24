namespace GenericUnityObjects.Editor
{
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

#if MISSING_SCRIPT_TYPE
    using MissingScriptType.Editor;
#endif

#if EASY_BUTTONS
    using EasyButtons.Editor;
#endif

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
#endif

#if ! DISABLE_GENERIC_OBJECT_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
#endif
    public class UnityObjectEditor :
#if ODIN_INSPECTOR
        OdinEditor
#else
        Editor
#endif
    {
#if ODIN_INSPECTOR && ! DISABLE_GENERIC_OBJECT_EDITOR
        [DidReloadScripts(1)]
        private static void OnScriptsReload()
        {
            // a workaround for bug https://bitbucket.org/sirenix/odin-inspector/issues/833/customeditor-with-editorforchildclasses
            var odinEditorTypeField = typeof(InspectorTypeDrawingConfig).GetField("odinEditorType", BindingFlags.Static | BindingFlags.NonPublic);
            odinEditorTypeField.SetValue(null, typeof(UnityObjectEditor));
        }
#endif

        private GenericUnityObjectHelper _helper;

#if MISSING_SCRIPT_TYPE
        private MissingScriptTypeUtility _missingScriptUtility;
#endif

#if EASY_BUTTONS
        private ButtonsDrawer _buttonsDrawer;
#endif

        protected
#if ODIN_INSPECTOR
            override
#endif
            void OnEnable()
        {
#if ODIN_INSPECTOR
            base.OnEnable();
#endif

            _helper = new GenericUnityObjectHelper(target);

#if MISSING_SCRIPT_TYPE
            try
            {
                _missingScriptUtility = new MissingScriptTypeUtility(serializedObject);
            }
            catch { } // SerializedObjectNotCreatableException is internal, so we can't catch it directly.
#endif

#if EASY_BUTTONS
            _buttonsDrawer = new ButtonsDrawer(target);
#endif
        }

        protected override void OnHeaderGUI()
        {
            GenericHeaderUtility.OnHeaderGUI(this);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            if (target == null)
            {
                DrawMissingScript();
                return;
            }

#if ODIN_INSPECTOR
            if (target == null || GlobalConfig<GeneralDrawerConfig>.Instance.ShowMonoScriptInEditor && !target.GetType().IsDefined(typeof(HideMonoScriptAttribute), true))
#endif
                _helper.DrawMonoScript(serializedObject.FindProperty("m_Script"));

#if ODIN_INSPECTOR
            bool previousValue = ForceHideMonoScriptInEditor;
            ForceHideMonoScriptInEditor = true;
            base.OnInspectorGUI();
            ForceHideMonoScriptInEditor = previousValue;
#else
            SerializedProperty iterator = serializedObject.GetIterator();

            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (iterator.propertyPath != "m_Script")
                    EditorGUILayout.PropertyField(iterator, true, null);
            }
#endif

#if EASY_BUTTONS
            _buttonsDrawer.DrawButtons(targets);
#endif

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMissingScript()
        {
#if MISSING_SCRIPT_TYPE
            _missingScriptUtility?.Draw();
#else
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
#endif
        }
    }
}