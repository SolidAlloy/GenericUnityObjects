namespace GenericUnityObjects.Editor.Util
{
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
                get => Util.PersistentStorage.UsageExampleTypesAreAdded;
                set => Util.PersistentStorage.UsageExampleTypesAreAdded = value;
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public static class GenericObjectDatabase
        {
            // TODO: check how samples will be created
            /*
            public static bool ContainsKey(Type genericType, Type[] key) =>
                GenericUnityObjects.Util.GenericObjectDatabase.ContainsKey(genericType, key);

            public static void Add(Type genericTypeWithoutArgs, Type[] key, Type value) =>
                GenericUnityObjects.Util.GenericObjectDatabase.Add(genericTypeWithoutArgs, key, value);
                */
        }
    }
}