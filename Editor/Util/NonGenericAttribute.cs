namespace GenericUnityObjects.Editor.Util
{
    using System;
    using TypeReferences;

    internal class NonGenericAttribute : InheritsAttribute
    {
        private static readonly string[] _additionalAssemblies = { "Assembly-CSharp" };

        public NonGenericAttribute(Type[] baseTypes)
            : base(baseTypes)
        {
            ExcludeNone = true;
            IncludeAdditionalAssemblies = _additionalAssemblies;
            ShortName = true;

            // When type has inheritance constraints, there's usually only a few types available, so expanding all
            // folders will look nice.
            if (baseTypes.Length != 0)
                ExpandAllFolders = true;
        }

        internal override bool MatchesRequirements(Type type)
        {
            return base.MatchesRequirements(type) && ! type.IsGenericTypeDefinition;
        }
    }
}