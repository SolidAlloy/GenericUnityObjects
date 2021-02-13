namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System.Collections.Generic;
    using UnityEngine;
    using Util;

    internal partial class BehavioursGenerationDatabase
    {
        [SerializeField] private Collection<BehaviourInfo>[] _genericTypeValues;
        [SerializeField] private BehaviourInfo[] _genericTypeKeys;
        protected override int GenericTypeKeysLength => _genericTypeKeys.Length;
        protected override int GenericTypeValuesLength => _genericTypeValues.Length;
        protected override GenericTypeInfo[] GetTypeValueAtIndex(int index) => _genericTypeValues[index];
        protected override GenericTypeInfo GetTypeKeyAtIndex(int index) => _genericTypeKeys[index];

        protected override void SetTypeValueAtIndex(int index, List<GenericTypeInfo> value) =>
            _genericTypeValues[index] = value.ConvertAll(typeInfo => (BehaviourInfo) typeInfo);

        protected override void SetTypeKeyAtIndex(int index, GenericTypeInfo value) =>
            _genericTypeKeys[index] = (BehaviourInfo) value;

        protected override void ResetTypeValuesToLength(int length) =>
            _genericTypeValues = new Collection<BehaviourInfo>[length];

        protected override void ResetTypeKeysToLength(int length) => _genericTypeKeys = new BehaviourInfo[length];
    }
}