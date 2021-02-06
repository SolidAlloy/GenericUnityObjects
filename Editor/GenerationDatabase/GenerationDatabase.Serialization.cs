namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System.Collections.Generic;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using Object = UnityEngine.Object;

    internal abstract partial class GenerationDatabase<TUnityObject>
        where TUnityObject : Object
    {
        protected bool _shouldSetDirty;

        public void OnAfterDeserialize()
        {
            InitializeArgumentGenericTypesDict();
            InitializeGenericTypeArgumentsDict();
        }

        public void Initialize()
        {
            _argumentGenericTypesDict = new FastIterationDictionary<ArgumentInfo, List<GenericTypeInfo>>();
            _argumentsPool = new Pool<ArgumentInfo>();
            _genericTypeArgumentsDict = new FastIterationDictionary<GenericTypeInfo, List<ConcreteClass>>();
            _genericTypesPool = new Pool<GenericTypeInfo>();
        }

        protected abstract void InitializeArgumentGenericTypesDict();

        protected abstract void InitializeGenericTypeArgumentsDict();

        public void OnBeforeSerialize()
        {
            SerializeArgumentGenericTypesDict();
            SerializeGenericTypeArgumentsDict();
        }

        protected abstract void SerializeArgumentGenericTypesDict();

        protected abstract void SerializeGenericTypeArgumentsDict();

        private void OnEnable()
        {
            if ( ! _shouldSetDirty)
                return;

            _shouldSetDirty = false;
            EditorUtility.SetDirty(this);
        }
    }
}