namespace GenericUnityObjects.MonoBehaviour_Example.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IntegrationTests;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;

    public static class Installer
    {
        private static readonly string _behaviourSampleFolder = FindExampleFolder();

        [DidReloadScripts((int)DidReloadScriptsOrder.AfterAssemblyGeneration)]
        public static void InstallBehaviours()
        {
            const string key = "InstallBehaviours";

            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                return;
            }

            PlayerPrefs.SetInt(key, 1);

            var typesToAdd = new[]
            {
                new KeyValuePair<Type, Type[]>(typeof(Unit<>), new[] { typeof(Archer) }),
                new KeyValuePair<Type, Type[]>(typeof(Unit<>), new[] { typeof(Knight) }),
            };

            ExampleInstaller.AddConcreteClasses<MonoBehaviour>(
                typesToAdd,
                AddComponentsDelayed);
        }

        private static string FindExampleFolder()
        {
            var guids = AssetDatabase.FindAssets("MonoBehaviour Example", new[] { "Assets/Samples" });
            Assert.IsTrue(guids.Length == 1);
            return AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        private static void AddComponentsDelayed() => EditorApplication.delayCall += AddComponents;

        private static void AddComponents()
        {
            var scene = EditorSceneManager.OpenScene($"{_behaviourSampleFolder}/MonoBehaviour Demo.unity", OpenSceneMode.Additive);
            var allGameObjects = scene.GetRootGameObjects();

            var archersSquad = allGameObjects.First(go => go.name == "Archers Squad");
            var archerUnitComponent = archersSquad.AddGenericComponent<Unit<Archer>>();
            Assert.IsNotNull(archerUnitComponent);
            archerUnitComponent.Warriors = archersSquad.GetComponentsInChildren<Archer>();

            var knightsSquad = allGameObjects.First(go => go.name == "Knights Squad");
            var knightUnitComponent = knightsSquad.AddGenericComponent<Unit<Knight>>();
            Assert.IsNotNull(knightUnitComponent);
            knightUnitComponent.Warriors = knightsSquad.GetComponentsInChildren<Knight>();

            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, true);

            EditorApplication.delayCall -= AddComponents;
            DeleteItself();
        }

        private static void DeleteItself()
        {
            string[] guids = AssetDatabase.FindAssets(nameof(Installer), new[] { _behaviourSampleFolder });
            Assert.IsTrue(guids.Length == 1);
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            AssetDatabase.DeleteAsset(assetPath);
        }
    }
}
