namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using SolidUtilities.Editor;
    using Util;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Methods from this class are called by other checkers to update concrete class assemblies.
    /// </summary>
    /// <typeparam name="TObject"> A type derived from <see cref="UnityEngine.Object"/>. </typeparam>
    internal class ConcreteClassChecker<TObject>
        where TObject : Object
    {
        private readonly GenericTypesChecker<TObject> _genericTypesChecker;

        public ConcreteClassChecker(GenericTypesChecker<TObject> genericTypesChecker)
            => _genericTypesChecker = genericTypesChecker;

        public void UpdateConcreteClassesAssemblies(Type behaviourType, ConcreteClass[] concreteClasses, string genericTypeGUID)
        {
            foreach (ConcreteClass concreteClass in concreteClasses)
            {
                UpdateConcreteClassAssembly(behaviourType, concreteClass, genericTypeGUID);
            }
        }

        private void UpdateConcreteClassAssembly(Type genericType, ConcreteClass concreteClass, string genericTypeGUID)
        {
            if ( ! GetArgumentTypes(concreteClass, out Type[] argumentTypes))
                return;

            ConcreteClassCreator<TObject>.UpdateConcreteClassAssembly(genericType, argumentTypes, concreteClass, genericTypeGUID);

            // 'ConcreteClass_ade148c5c4a7ea64bb9a635005ef6220' is missing the class attribute 'ExtensionOfNativeClass'!
            LogHelper.RemoveLogEntriesByMode(LogModes.EditorErrors);

            // GameObject (named 'New Game Object') references runtime script in scene file. Fixing!
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