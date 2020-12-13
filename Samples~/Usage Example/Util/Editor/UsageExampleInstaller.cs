namespace GenericScriptableObjects.Usage_Example.Util.Editor
{
    using System;
    using ConcreteImplementations;
    using GenericScriptableObjects.Editor;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEditor.Callbacks;

    public static class UsageExampleInstaller
    {
        private static readonly Type[] TypesToAdd =
        {
            typeof(WarriorStats_1_Archer), typeof(WarriorStats_1_Berserker), typeof(WarriorStats_1_Knight),
            typeof(WarriorsRelationship_2_Archer_Berserker), typeof(WarriorsRelationship_2_Archer_Knight),
            typeof(WarriorsRelationship_2_Berserker_Knight)
        };

        [DidReloadScripts]
        public static void AddTypesToDictIfNecessary()
        {
            if (SampleLayer.PersistentStorage.UsageExampleTypesAreAdded)
                return;

            foreach (Type type in TypesToAdd)
            {
                Type parentType = type.BaseType;
                Assert.IsNotNull(parentType);
                Type parentTypeDefinition = parentType.GetGenericTypeDefinition();
                var genericArgs = parentType.GetGenericArguments();

                if (! SampleLayer.GenericObjectDatabase.ContainsKey(parentTypeDefinition, genericArgs))
                    SampleLayer.GenericObjectDatabase.Add(parentTypeDefinition, genericArgs, type);
            }

            SampleLayer.PersistentStorage.UsageExampleTypesAreAdded = true;

            // Reload assemblies to generate MenuItems for the newly added types.
            AssetDatabase.Refresh();
        }
    }
}