namespace GenericScriptableObjects
{
    using System;
    using System.Linq;
    using TypeReferences;
    using UnityEngine;
    using UnityEngine.Assertions;

    public abstract class BehaviourSelector : MonoBehaviour
    {
        [SerializeField] internal TypeReferenceWithBaseTypes[] TypeRefs;
        public abstract Type GenericBehaviourType { get; }

        private void Reset()
        {
            Assert.IsTrue(GenericBehaviourType.IsGenericTypeDefinition);

            int argsNum = GenericBehaviourType.GetGenericArguments().Length;
            TypeRefs = new TypeReferenceWithBaseTypes[argsNum];

            var constraints = GenericBehaviourType.GetGenericArguments()
                .Select(type => type.GetGenericParameterConstraints())
                .ToArray();

            for (int i = 0; i < argsNum; i++)
            {
                TypeRefs[i] = new TypeReferenceWithBaseTypes
                {
                    BaseTypeNames = constraints[i]
                        .Select(TypeReference.GetTypeNameAndAssembly)
                        .ToArray()
                };
            }
        }
    }
}