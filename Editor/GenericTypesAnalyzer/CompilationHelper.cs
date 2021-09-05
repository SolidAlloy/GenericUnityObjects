namespace GenericUnityObjects.Editor
{
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEngine;

    internal static class CompilationHelper
    {
        private const string CompiledOnceKey = "CompiledOnce";

        static CompilationHelper()
        {
            EditorApplication.quitting += () =>
            {
                PlayerPrefs.DeleteKey(CompiledOnceKey);
                PlayerPrefs.Save();
            };
        }

        public static void RecompileOnce()
        {
            if (PlayerPrefs.GetInt(CompiledOnceKey) == 1)
                return;

            PlayerPrefs.SetInt(CompiledOnceKey, 1);
            PlayerPrefs.Save();
            CompilationPipeline.RequestScriptCompilation();
        }
    }
}