namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using SolidUtilities.Editor.Helpers;
    using Util;

    internal static partial class GenericTypesAnalyzer<TDatabase>
    {
        private static class ConcreteClassChecker
        {
            public static void UpdateConcreteClassesAssemblies(Type behaviourType, ConcreteClass[] concreteClasses)
            {
                foreach (ConcreteClass concreteClass in concreteClasses)
                {
                    UpdateConcreteClassAssembly(behaviourType, concreteClass);
                }
            }

            private static void UpdateConcreteClassAssembly(Type genericType, ConcreteClass concreteClass)
            {
                if ( ! GetArgumentTypes(concreteClass, out Type[] argumentTypes))
                    return;

                UpdateConcreteClassAssembly(genericType, argumentTypes, concreteClass);
                LogHelper.RemoveLogEntriesByMode(LogModes.EditorErrors);
                LogHelper.RemoveLogEntriesByMode(LogModes.UserAndEditorWarnings);
            }

            private static void UpdateConcreteClassAssembly(Type genericType, Type[] argumentTypes, ConcreteClass concreteClass)
            {
                string newAssemblyName;

                try
                {
                    newAssemblyName = ConcreteClassCreator.GetConcreteClassAssemblyName(genericType, argumentTypes);
                }
                catch (TypeLoadException)
                {
                    return;
                }

                AssemblyAssetOperations.ReplaceAssemblyByGUID(concreteClass.AssemblyGUID, newAssemblyName, () =>
                {
                    if (typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(genericType))
                    {
                        BehaviourCreator.CreateConcreteClassAssembly(genericType, argumentTypes, newAssemblyName);
                    }
                    else if (typeof(GenericScriptableObject).IsAssignableFrom(genericType))
                    {
                        ScriptableObjectCreator.CreateConcreteClassAssembly(genericType, argumentTypes, newAssemblyName);
                    }
                });
            }

            private static bool GetArgumentTypes(ConcreteClass concreteClass, out Type[] argumentTypes)
            {
                var arguments = concreteClass.Arguments;
                int argumentsLength = arguments.Length;

                argumentTypes = new Type[argumentsLength];

                for (int i = 0; i < argumentsLength; i++)
                {
                    ArgumentInfo argument = arguments[i];

                    if (argument.RetrieveType<TDatabase>(out Type type, out bool retrievedFromGUID))
                    {
                        if (retrievedFromGUID)
                        {
                            ArgumentsChecker.UpdateArgumentTypeName(argument, type);
                        }

                        argumentTypes[i] = type;
                    }
                    else
                    {
                        GenerationDatabase<TDatabase>.RemoveArgument(argument, AssemblyAssetOperations.RemoveAssemblyByGUID);

                        // Since one of the arguments was not found, the assembly associated with the concrete class
                        // already has been removed, and there is no need to try updating it.
                        return false;
                    }
                }

                return true;
            }
        }
    }
}