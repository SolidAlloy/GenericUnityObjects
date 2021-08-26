namespace GenericUnityObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ApplyToChildrenAttribute : Attribute
    {
        public readonly IReadOnlyCollection<Type> Attributes;

        public ApplyToChildrenAttribute(params Type[] attributeTypes)
        {
            Attributes = attributeTypes.Where(attributeType => attributeType.GetCustomAttribute<AttributeUsageAttribute>()?.Inherited is false).ToList();
        }
    }
}