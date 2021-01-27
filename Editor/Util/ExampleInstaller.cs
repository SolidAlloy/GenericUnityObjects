namespace GenericUnityObjects.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using Editor.Util;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEngine.Events;

    public static class ExampleInstaller
    {
        [UsedImplicitly]
        public static void InstallScriptableObjects(KeyValuePair<Type, Type[]>[] typesToAdd, UnityAction afterAddingTypes)
        {
            foreach (var pair in typesToAdd)
            {
                ConcreteClassCreator<GenericScriptableObject>.CreateConcreteClass(pair.Key, pair.Value);
            }

            PersistentStorage.ExecuteOnScriptsReload(afterAddingTypes);
            AssetDatabase.Refresh();
        }
    }
}