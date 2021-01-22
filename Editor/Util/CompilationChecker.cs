namespace Testing.Editor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEditor.Compilation;
    using Debug = UnityEngine.Debug;

    [InitializeOnLoad]
    internal static class CompilationChecker
    {
        private static readonly Dictionary<string, Stopwatch> Dictionary;
        private static readonly Stopwatch Stopwatch;

        static CompilationChecker()
        {
#if GENERIC_UNITY_OBJECTS_DEBUG
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            CompilationPipeline.assemblyCompilationStarted += OnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            Dictionary = new Dictionary<string, Stopwatch>();
            Stopwatch = new Stopwatch();
#endif
        }

        private static void OnCompilationStarted(object context)
        {
            Dictionary.Clear();
            Stopwatch.Start();
        }

        private static void OnCompilationFinished(object context)
        {
            var elapsed = Stopwatch.Elapsed;

            Stopwatch.Stop();
            Stopwatch.Reset();

            foreach (var pair in Dictionary)
            {
                Debug.Log($"Assembly {pair.Key.Replace("Library/ScriptAssemblies/", string.Empty)} " +
                          $"built in {pair.Value.Elapsed.TotalSeconds:F} seconds.");
            }

            Debug.Log($"Total compilation time: {elapsed.TotalSeconds:F} seconds.");
        }

        private static void OnAssemblyCompilationStarted(string value)
        {
            Dictionary.Add(value, Stopwatch.StartNew());
        }

        private static void OnAssemblyCompilationFinished(string value, CompilerMessage[] messages)
        {
            Dictionary[value].Stop();
        }
    }
}