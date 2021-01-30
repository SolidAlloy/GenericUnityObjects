namespace GenericUnityObjects.ScriptableObject_Example.Editor
{
    using System;
    using System.Collections.Generic;
    using IntegrationTests;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;


    public static class Installer
    {
        private static readonly string _scriptableObjectSampleFolder = FindExampleFolder();


        [DidReloadScripts((int)DidReloadScriptsOrder.AfterAssemblyGeneration)]
        private static void InstallScriptableObjects()
        {
            const string key = "InstallScriptableObjects";

            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                return;
            }

            PlayerPrefs.SetInt(key, 1);

            var typesToAdd = new[]
            {
                new KeyValuePair<Type, Type[]>(typeof(WarriorStats<>), new[] { typeof(Archer) }),
                new KeyValuePair<Type, Type[]>(typeof(WarriorStats<>), new[] { typeof(Berserker) }),
                new KeyValuePair<Type, Type[]>(typeof(WarriorStats<>), new[] { typeof(Knight) }),
                new KeyValuePair<Type, Type[]>(typeof(WarriorsRelationship<,>), new[] { typeof(Archer), typeof(Berserker) }),
                new KeyValuePair<Type, Type[]>(typeof(WarriorsRelationship<,>), new[] { typeof(Archer), typeof(Knight) }),
                new KeyValuePair<Type, Type[]>(typeof(WarriorsRelationship<,>), new[] { typeof(Berserker), typeof(Knight) }),
            };

            ExampleInstaller.AddConcreteClasses<GenericScriptableObject>(typesToAdd, CreateScriptableObjects);
        }

        private static string FindExampleFolder()
        {
            var guids = AssetDatabase.FindAssets("ScriptableObject Example", new[] { "Assets/Samples" });
            Assert.IsTrue(guids.Length == 1);
            return AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        private static void CreateScriptableObjects()
        {
            var warriors = new Warriors();
            CreateStats(warriors);
            CreateRelationships(warriors);
            warriors.SetDirty();
            DeleteItself();
        }

        private static void DeleteItself()
        {
            string[] guids = AssetDatabase.FindAssets(nameof(Installer), new[] { _scriptableObjectSampleFolder });
            Assert.IsTrue(guids.Length == 1);
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            AssetDatabase.DeleteAsset(assetPath);
        }

        private static void CreateStats(Warriors warriors)
        {
            var heavyArcher = GenericScriptableObject.CreateInstance<WarriorStats<Archer>>();
            heavyArcher.Damage = 65;
            heavyArcher.Health = 600;

            var lightArcher = GenericScriptableObject.CreateInstance<WarriorStats<Archer>>();
            lightArcher.Damage = 85;
            lightArcher.Health = 450;

            var defaultBerserker = GenericScriptableObject.CreateInstance<WarriorStats<Berserker>>();
            defaultBerserker.Damage = 80;
            defaultBerserker.Health = 1000;

            var defaultKnight = GenericScriptableObject.CreateInstance<WarriorStats<Knight>>();
            defaultKnight.Damage = 100;
            defaultKnight.Health = 900;

            const string statsFolder = "Warrior Stats";
            AssetDatabase.CreateAsset(heavyArcher, $"{_scriptableObjectSampleFolder}/{statsFolder}/Heavy Archer.asset");
            AssetDatabase.CreateAsset(lightArcher, $"{_scriptableObjectSampleFolder}/{statsFolder}/Light Archer.asset");
            AssetDatabase.CreateAsset(defaultBerserker, $"{_scriptableObjectSampleFolder}/{statsFolder}/Default Berserker Stats.asset");
            AssetDatabase.CreateAsset(defaultKnight, $"{_scriptableObjectSampleFolder}/{statsFolder}/Default Knight Stats.asset");

            warriors.Arnet.Stats = lightArcher;
            warriors.Edun.Stats = heavyArcher;
            warriors.Fulchard.Stats = defaultKnight;
            warriors.Geltull.Stats = defaultBerserker;
        }

        private static void CreateRelationships(Warriors warriors)
        {
            const string relationshipsFolder = "Relationships";

            var arnetGeltull = GenericScriptableObject.CreateInstance<WarriorsRelationship<Archer, Berserker>>();
            arnetGeltull.Relationship = RelationshipType.Neutral;
            arnetGeltull.FirstWarrior = warriors.Arnet;
            arnetGeltull.SecondWarrior = warriors.Geltull;
            AssetDatabase.CreateAsset(arnetGeltull, $"{_scriptableObjectSampleFolder}/{relationshipsFolder}/Arnet - Geltull.asset");

            var edunFulchard = GenericScriptableObject.CreateInstance<WarriorsRelationship<Archer, Knight>>();
            edunFulchard.Relationship = RelationshipType.Friendly;
            edunFulchard.FirstWarrior = warriors.Edun;
            edunFulchard.SecondWarrior = warriors.Fulchard;
            AssetDatabase.CreateAsset(edunFulchard, $"{_scriptableObjectSampleFolder}/{relationshipsFolder}/Edun - Fulchard.asset");

            var geltullFulchard = GenericScriptableObject.CreateInstance<WarriorsRelationship<Berserker, Knight>>();
            geltullFulchard.Relationship = RelationshipType.Aggressive;
            geltullFulchard.FirstWarrior = warriors.Geltull;
            geltullFulchard.SecondWarrior = warriors.Fulchard;
            AssetDatabase.CreateAsset(geltullFulchard, $"{_scriptableObjectSampleFolder}/{relationshipsFolder}/Geltull - Fulchard.asset");
        }

        private class Warriors
        {
            private const string WarriorsFolder = "Warriors";

            public readonly Archer Arnet = GetWarrior<Archer>("Arnet");
            public readonly Archer Edun = GetWarrior<Archer>("Edun");
            public readonly Knight Fulchard = GetWarrior<Knight>("Fulchard");
            public readonly Berserker Geltull = GetWarrior<Berserker>("Geltull");

            public void SetDirty()
            {
                EditorUtility.SetDirty(Arnet);
                EditorUtility.SetDirty(Edun);
                EditorUtility.SetDirty(Fulchard);
                EditorUtility.SetDirty(Geltull);
            }

            private static T GetWarrior<T>(string name)
                where T : WarriorClass
            {
                return AssetDatabase.LoadAssetAtPath<T>($"{_scriptableObjectSampleFolder}/{WarriorsFolder}/{name}.asset");
            }
        }
    }

}