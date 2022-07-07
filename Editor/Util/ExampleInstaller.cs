namespace GenericUnityObjects.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using Editor.Util;
    using JetBrains.Annotations;
    using SolidUtilities.Editor;
    using UnityEditor;
    using Object = UnityEngine.Object;

    /// <summary>
    /// A class that helps to install Samples. When a sample is imported to project, its concrete class are not generated
    /// yet and need to be generated manually. They call this class to generate a bunch of concrete classes at once.
    /// </summary>
    public static class ExampleInstaller
    {
        [UsedImplicitly]
        public static void AddConcreteClasses<TObject>(KeyValuePair<Type, Type[]>[] typesToAdd, Action afterAddingTypes)
            where TObject : Object
        {
            using (AssetDatabaseHelper.DisabledScope())
            {
                foreach (var pair in typesToAdd)
                {
                    ConcreteClassCreator<TObject>.CreateConcreteClass(pair.Key, pair.Value);
                }
            }

            PersistentStorage.ExecuteOnScriptsReload(afterAddingTypes);
            AssetDatabase.Refresh();
        }
    }
}