namespace GenericUnityObjects.IntegrationTests
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using Editor.MonoBehaviours;
    using JetBrains.Annotations;
    using NUnit.Framework;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Util;

    public class GenericBehaviour_ConcreteClass : ScriptableObject
    {
        private const string TwoWarnings = "TwoWarnings";
        private const string NewBehaviourName = "NewBehaviour";
        private const string ShortArgName = "int";
        private const string FullArgName = "Int32";
        private static readonly Type ArgType = typeof(int);

        [SerializeField] private GameObject _testGameObject;

        [UnitySetUp, UsedImplicitly]
        public IEnumerator BeforeEachTest()
        {
            LogHelper.Clear();

            TestHelper.CreateFolder(TestHelper.TestingDir);

            TestHelper.AddBehaviourScript();

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            _testGameObject = new GameObject();

            var selectorType = AssetDatabase
                .LoadAssetAtPath<MonoScript>($"{Config.BehaviourSelectorsPath}/{TestHelper.DefaultGenericClassName}_1.dll")
                .GetClass();

            GenericBehaviourCreator.AddComponent(
                selectorType,
                _testGameObject,
                TestHelper.GetTestType($"{TestHelper.DefaultGenericClassName}`1"),
                new[] { typeof(int) });

            yield return new WaitForDomainReload();
            yield return null;
        }

        [UnityTearDown, UsedImplicitly]
        public IEnumerator AfterEachTest()
        {
            AssetDatabase.DeleteAsset(TestHelper.TestingDir);
            AssetDatabase.Refresh();
            yield return new RecompileScripts(false);
            yield return null;
            yield return new RecompileScripts(false);

            if (TestContext.CurrentContext.Test.Properties["Category"].Contains(TwoWarnings))
            {
                TestHelper.AssertNumberOfLogs(expectedWarningLogs: 2);
            }
            else
            {
                TestHelper.AssertNumberOfLogs();
            }
        }

        [Test]
        public void Concrete_class_is_added_when_concrete_component_is_added_for_the_first_time()
        {
            Assert.That(File.Exists($"{Config.MonoBehavioursPath}/{TestHelper.DefaultGenericClassName}_Int32.dll"));

            var menuPaths = Unsupported.GetSubmenus("Component");
            Assert.That(menuPaths.Contains($"Component/Scripts/{TestHelper.DefaultGenericClassName}<int>"));

            var components = _testGameObject.GetComponents<Component>();
            Assert.That(components.Length, Is.EqualTo(2));
            Assert.That(components[1], Is.Not.Null);
        }

        [Test, Category(TwoWarnings)]
        public void Assembly_does_not_reload_when_concrete_component_is_added_for_the_second_time()
        {
            var selectorType = AssetDatabase
                .LoadAssetAtPath<MonoScript>($"{Config.MonoBehavioursPath}/{TestHelper.DefaultGenericClassName}_1.dll")
                .GetClass();

            GenericBehaviourCreator.AddComponent(
                selectorType,
                _testGameObject,
                TestHelper.GetTestType($"{TestHelper.DefaultGenericClassName}`1"),
                new[] { typeof(int) });

            var components = _testGameObject.GetComponents<Component>();

            var genericType = TestHelper.GetTestType($"{TestHelper.DefaultGenericClassName}`1")
                .MakeGenericType(typeof(int));

            Assert.That(components.Length, Is.EqualTo(3));
            Assert.That(components[2].GetType().BaseType == genericType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_type_name_changes_and_guid_does_not()
        {
            TestHelper.ChangeTypeNameOnly(TestHelper.DefaultGenericClassName, NewBehaviourName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.Behaviour.AssertThatConcreteClassIsUpdated(_testGameObject, NewBehaviourName, ShortArgName, ArgType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_guid_changes_but_type_name_does_not()
        {
            TestHelper.ChangeGUID(TestHelper.DefaultGenericClassName);

            yield return new WaitForDomainReload();
            yield return null;

            TestHelper.Behaviour.AssertThatConcreteClassIsUpdated(_testGameObject, TestHelper.DefaultGenericClassName, ShortArgName, ArgType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_type_name_changes_but_script_name_and_guid_does_not()
        {
            TestHelper.ChangeTypeNameOnly(TestHelper.DefaultGenericClassName, NewBehaviourName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.Behaviour.AssertThatConcreteClassIsUpdated(_testGameObject, NewBehaviourName, ShortArgName, ArgType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_generic_argument_name_changes()
        {
            const string newArgName = "TNew";

            TestHelper.ChangeArgumentName(TestHelper.DefaultGenericClassName, newArgName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.Behaviour.AssertThatConcreteClassIsUpdated(_testGameObject, TestHelper.DefaultGenericClassName, ShortArgName, ArgType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_removed_when_type_is_removed()
        {
            TestHelper.RemoveScript(TestHelper.DefaultGenericClassName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.Behaviour.AssertThatConcreteClassIsRemoved(_testGameObject, TestHelper.DefaultGenericClassName, FullArgName, ShortArgName);
        }

        [UnityTest]
        public IEnumerator Concrete_class_changes_when_type_name_and_guid_are_different()
        {
            TestHelper.ChangeTypeNameOnly(TestHelper.DefaultGenericClassName, NewBehaviourName);
            TestHelper.ChangeGUID(TestHelper.DefaultGenericClassName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.Behaviour.AssertThatConcreteClassChanged(_testGameObject, TestHelper.DefaultGenericClassName, NewBehaviourName, FullArgName, ShortArgName);
        }
    }
}