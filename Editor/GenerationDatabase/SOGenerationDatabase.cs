namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System.Collections.Generic;
    using UnityEngine;
    using Util;

    /// <summary>
    /// All the work is done in the parent class. This is implemented just to create a ScriptableObject asset.
    /// </summary>
    internal class SOGenerationDatabase : GenerationDatabase<ScriptableObject>
    {
        [SerializeField] private Collection<GenericTypeInfo>[] _genericTypeValues;
        [SerializeField] private GenericTypeInfo[] _genericTypeKeys;

        protected override int GenericTypeKeysLength => _genericTypeKeys.Length;

        protected override int GenericTypeValuesLength => _genericTypeValues.Length;

        protected override GenericTypeInfo[] GetTypeValueAtIndex(int index) => _genericTypeValues[index];

        protected override GenericTypeInfo GetTypeKeyAtIndex(int index) => _genericTypeKeys[index];

        protected override void SetTypeValueAtIndex(int index, List<GenericTypeInfo> value) => _genericTypeValues[index] = value;

        protected override void SetTypeKeyAtIndex(int index, GenericTypeInfo value) => _genericTypeKeys[index] = value;

        protected override void ResetTypeValuesToLength(int length) => _genericTypeValues = new Collection<GenericTypeInfo>[length];

        protected override void ResetTypeKeysToLength(int length) => _genericTypeKeys = new GenericTypeInfo[length];
    }
}