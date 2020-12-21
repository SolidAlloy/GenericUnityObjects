namespace GenericUnityObjects.Util
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using SolidUtilities.Extensions;
    using UnityEngine.Assertions;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    internal static class TypeHelper
    {
        [CanBeNull]
        public static Type GetEmptyTypeDerivedFrom(Type parentType)
        {
#if UNITY_EDITOR
            var foundTypes = TypeCache.GetTypesDerivedFrom(parentType);

            if (foundTypes.Count == 0)
                return null;

            // Why would there be another empty type derived from the GenericScriptableObject other than the auto-generated one?
            Assert.IsTrue(foundTypes.Count == 1);

            Type matchingType = foundTypes.FirstOrDefault(type => type.IsEmpty());
            return matchingType;
#else
            return null;
#endif
        }
    }
}