#if MISSING_SCRIPT_TYPE
namespace GenericUnityObjects.Editor
{
    using UnityEditor;

    [InitializeOnLoad]
    internal static class MissingScriptTypeDisabler
    {
        static MissingScriptTypeDisabler()
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (!symbols.Contains("DISABLE_GENERIC_OBJECT_EDITOR") && !symbols.Contains("DISABLE_MISSING_SCRIPT_EDITOR"))
            {
                symbols += ";DISABLE_MISSING_SCRIPT_EDITOR";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
        }
    }
}
#endif