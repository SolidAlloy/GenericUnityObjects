namespace GenericScriptableObjects.Editor.AssetCreation
{
    using System;
    using SolidUtilities.Helpers;

    internal static class GenericBehaviourUtil
    {
        public static string GetGeneratedFileName(Type genericTypeWithoutArgs)
        {
            string fileName = genericTypeWithoutArgs.FullName.MakeClassFriendly();
            return $"{fileName}.cs";
        }
    }
}