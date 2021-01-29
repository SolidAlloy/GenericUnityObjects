namespace GenericUnityObjects.Util
{
    using System;
    using System.Linq;
    using TypeReferences;
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    /// Parent class to all Generic MonoBehaviour selectors. It is a component that is added to game objects and allows
    /// to choose generic arguments to add the actual generic component.
    /// </summary>
    public abstract class BehaviourSelector : MonoBehaviour
    {
        [SerializeField] internal bool JustBeenAdded;
        [SerializeField] internal TypeReferenceWithBaseTypes[] TypeRefs;

        // This field is overriden by individual selectors and represents a generic type definition of a generic MonoBehaviour.
        public abstract Type GenericBehaviourType { get; }

        private void Reset()
        {
            JustBeenAdded = true;

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