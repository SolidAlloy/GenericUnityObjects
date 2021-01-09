namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using SolidUtilities.Editor.Helpers;
    using Util;
    using Object = UnityEngine.Object;

    internal class ConcreteClassChecker<TObject>
        where TObject : Object
    {
        private readonly GenericTypesChecker<TObject> _genericTypesChecker;

        public ConcreteClassChecker(GenericTypesChecker<TObject> genericTypesChecker)
            => _genericTypesChecker = genericTypesChecker;

        public void UpdateConcreteClassesAssemblies(Type behaviourType, ConcreteClass[] concreteClasses)
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

            ConcreteClassCreator<TObject>.UpdateConcreteClassAssembly(genericType, argumentTypes, concreteClass);
            LogHelper.RemoveLogEntriesByMode(LogModes.EditorErrors);
            LogHelper.RemoveLogEntriesByMode(LogModes.UserAndEditorWarnings);
        }

        private bool GetArgumentTypes(ConcreteClass concreteClass, out Type[] argumentTypes)
        {
            var arguments = concreteClass.Arguments;
            int argumentsLength = arguments.Length;

            argumentTypes = new Type[argumentsLength];

            for (int i = 0; i < argumentsLength; i++)
            {
                ArgumentInfo argument = arguments[i];

                if (argument.RetrieveType<TObject>(out Type type, out bool retrievedFromGUID))
                {
                    if (retrievedFromGUID)
                    {
                        ArgumentsChecker<TObject>.UpdateArgumentTypeName(argument, type, _genericTypesChecker);
                    }

                    argumentTypes[i] = type;
                }
                else
                {
                    GenerationDatabase<TObject>.RemoveArgument(argument, AssemblyAssetOperations.RemoveAssemblyByGUID);

                    // Since one of the arguments was not found, the assembly associated with the concrete class
                    // already has been removed, and there is no need to try updating it.
                    return false;
                }
            }

            return true;
        }
    }
}