namespace GenericUnityObjects.Editor
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// A class the Samples package can use to call some properties and methods of the internal classes.
    /// </summary>
    [UsedImplicitly]
    public static class SampleLayer
    {
        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public static class PersistentStorage
        {
            public static bool UsageExampleTypesAreAdded
            {
                get => Editor.PersistentStorage.UsageExampleTypesAreAdded;
                set => Editor.PersistentStorage.UsageExampleTypesAreAdded = value;
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public static class GenericObjectDatabase
        {
            public static bool ContainsKey(Type genericType, Type[] key) =>
                Util.GenericObjectDatabase.ContainsKey(genericType, key);

            public static void Add(Type genericTypeWithoutArgs, Type[] key, Type value) =>
                Util.GenericObjectDatabase.Add(genericTypeWithoutArgs, key, value);
        }
    }
}