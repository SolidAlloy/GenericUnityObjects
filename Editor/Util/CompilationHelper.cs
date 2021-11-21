namespace GenericUnityObjects.Editor.Util
{
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEngine;

    [InitializeOnLoad]
    internal static class CompilationHelper
    {
        private const string CompiledOnceKey = "CompiledOnce";

        private static double _recompilationTime;

        public static bool RecompilationRequested { get; private set; }

        static CompilationHelper()
        {
            EditorApplication.quitting += () =>
            {
                PlayerPrefs.DeleteKey(CompiledOnceKey);
                PlayerPrefs.Save();
            };

            _recompilationTime = EditorApplication.timeSinceStartup;
        }

        public static void RecompileOnce()
        {
            RecompilationRequested = true;

            var timeSinceRecompilation = EditorApplication.timeSinceStartup - _recompilationTime;

            // Recompilation doesn't work when it is requested right after the domain reload, for some reason
            if (timeSinceRecompilation < 1)
            {
                EditorCoroutineHelper.Delay(RecompileOnceImpl, 1 - (float) timeSinceRecompilation);
            }
            else
            {
                RecompileOnceImpl();
            }
        }

        private static void RecompileOnceImpl()
        {
            if (PlayerPrefs.GetInt(CompiledOnceKey) == 1)
                return;

            PlayerPrefs.SetInt(CompiledOnceKey, 1);
            PlayerPrefs.Save();
            CompilationPipeline.RequestScriptCompilation();
        }

        public static void CompilationNotNeeded()
        {
            PlayerPrefs.SetInt(CompiledOnceKey, 0);
            PlayerPrefs.Save();
        }
    }
}