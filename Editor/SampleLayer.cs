namespace GenericScriptableObjects.Editor
{
    using System;

    /// <summary>
    /// A class the Samples package can use to call some properties and methods of the internal classes.
    /// </summary>
    public static class SampleLayer
    {
        public static class PersistentStorage
        {
            public static bool UsageExampleTypesAreAdded
            {
                get => Util.PersistentStorage.UsageExampleTypesAreAdded;
                set => Util.PersistentStorage.UsageExampleTypesAreAdded = value;
            }
        }

        public static class GenericObjectDatabase
        {
            public static bool ContainsKey(Type genericType, Type[] key) =>
                GenericScriptableObjects.GenericObjectDatabase.ContainsKey(genericType, key);

            public static void Add(Type genericTypeWithoutArgs, Type[] key, Type value) =>
                GenericScriptableObjects.GenericObjectDatabase.Add(genericTypeWithoutArgs, key, value);
        }
    }
}