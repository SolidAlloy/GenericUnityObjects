#if GENERIC_UNITY_OBJECTS_DEBUG
namespace GenericUnityObjects.Editor.Util
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using Debug = UnityEngine.Debug;


    using UnityEditor.Compilation;


    /// <summary>
    /// Logs how many assemblies were compiled. It may be useful to check that only the assemblies you expect to be
    /// affected are compiled and not more.
    /// </summary>
    [InitializeOnLoad]
    internal static class CompilationChecker
    {
        private static readonly List<string> _assemblies;

        static CompilationChecker()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
#pragma warning disable 618
            CompilationPipeline.assemblyCompilationStarted += OnAssemblyCompilationStarted;
#pragma warning restore 618
            _assemblies = new List<string>();
        }

        private static void OnCompilationStarted(object context)
        {
            _assemblies.Clear();
        }

        private static void OnCompilationFinished(object context)
        {
            string assemblyNames = string.Join(", ", _assemblies
                .Select(name => name.Replace("Library/ScriptAssemblies/", string.Empty)
                    .Replace(".dll", string.Empty)));

            Debug.Log($"{_assemblies.Count} assemblies were compiled: {assemblyNames}");
        }

        private static void OnAssemblyCompilationStarted(string assemblyName)
        {
            _assemblies.Add(assemblyName);
        }
    }
}
#endif