namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using SolidUtilities.Editor.Helpers;
    using Util;

    internal static partial class GenericTypesAnalyzer<TDatabase>
    {
        private class ScriptableObjectConcreteClassChecker : ConcreteClassChecker
        {
            private static readonly ScriptableObjectConcreteClassChecker _checker =
                new ScriptableObjectConcreteClassChecker();

            public static void UpdateConcreteClassesAssemblies(Type behaviourType, ConcreteClass[] concreteClasses) =>
                _checker.UpdateConcreteClassesAssembliesImpl(behaviourType, concreteClasses);

            protected override void UpdateConcreteClassAssembly(Type genericType, Type[] argumentTypes, ConcreteClass concreteClass)
            {
                ConcreteSOCreator.UpdateConcreteClassAssembly(genericType, argumentTypes, concreteClass);
            }
        }

        private class BehaviourConcreteClassChecker : ConcreteClassChecker
        {
            private static readonly BehaviourConcreteClassChecker _checker =
                new BehaviourConcreteClassChecker();

            public static void UpdateConcreteClassesAssemblies(Type behaviourType, ConcreteClass[] concreteClasses) =>
                _checker.UpdateConcreteClassesAssembliesImpl(behaviourType, concreteClasses);

            protected override void UpdateConcreteClassAssembly(Type genericType, Type[] argumentTypes, ConcreteClass concreteClass)
            {
                ConcreteBehaviourCreator.UpdateConcreteClassAssembly(genericType, argumentTypes, concreteClass);
            }
        }

        private abstract class ConcreteClassChecker
        {
            protected void UpdateConcreteClassesAssembliesImpl(Type behaviourType, ConcreteClass[] concreteClasses)
            {
                foreach (ConcreteClass concreteClass in concreteClasses)
                {
                    UpdateConcreteClassAssembly(behaviourType, concreteClass);
                }
            }

            private void UpdateConcreteClassAssembly(Type genericType, ConcreteClass concreteClass)
            {
                if ( ! GetArgumentTypes(concreteClass, out Type[] argumentTypes))
                    return;

                UpdateConcreteClassAssembly(genericType, argumentTypes, concreteClass);
                LogHelper.RemoveLogEntriesByMode(LogModes.EditorErrors);
                LogHelper.RemoveLogEntriesByMode(LogModes.UserAndEditorWarnings);
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

            protected abstract void UpdateConcreteClassAssembly(Type genericType, Type[] argumentTypes,
                ConcreteClass concreteClass);
        }
    }
}