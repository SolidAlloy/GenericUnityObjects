namespace GenericUnityObjects.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using Editor.Util;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEngine.Events;
    using Object = UnityEngine.Object;

    public static class ExampleInstaller
    {
        [UsedImplicitly]
        public static void AddConcreteClasses<TObject>(KeyValuePair<Type, Type[]>[] typesToAdd, UnityAction afterAddingTypes)
            where TObject : Object
        {
            using (new DisabledAssetDatabase(null))
            {
                foreach (var pair in typesToAdd)
                {
                    ConcreteClassCreator<TObject>.CreateConcreteClass(pair.Key, pair.Value);
                }
            }

            AssetDatabase.Refresh();

            PersistentStorage.ExecuteOnScriptsReload(afterAddingTypes);
        }
    }
}