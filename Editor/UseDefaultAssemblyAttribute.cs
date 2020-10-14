namespace GenericScriptableObjects.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using TypeReferences;

    internal class UseDefaultAssemblyAttribute : TypeOptionsAttribute
    {
        public UseDefaultAssemblyAttribute()
        {
            IncludeAdditionalAssemblies = GetAssembliesCsharpHasAccessTo();
        }

        // The string array will then be converted to Assemblies again inside the property drawer and it's not great for
        // performance, but overriding that behavior will require more code and more changes to the TypeReferences package.
        private static string[] GetAssembliesCsharpHasAccessTo()
        {
            var csharpAssembly = Assembly.Load("Assembly-CSharp");
            var assemblyNames = new List<string> { "Assembly-CSharp" };

            var referencedAssemblyNames = csharpAssembly.GetReferencedAssemblies().Select(assemblyName => assemblyName.Name);
            assemblyNames.AddRange(referencedAssemblyNames);
            return assemblyNames.ToArray();
        }
    }
}