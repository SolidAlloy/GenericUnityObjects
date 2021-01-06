// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    using JetBrains.Annotations;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        [PublicAPI]
        public string AssemblyName { get; }
    }
}