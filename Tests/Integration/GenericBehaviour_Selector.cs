namespace GenericUnityObjects.IntegrationTests
{
    using System.Collections;
    using System.IO;
    using System.Linq;
    using JetBrains.Annotations;
    using NUnit.Framework;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Util;

    public class GenericBehaviour_Selector : ScriptableObject
    {
        private const string NewBehaviourName = "NewBehaviour";

        private static readonly string DefaultSelectorPath = $"{Config.BehaviourSelectorsPath}/{TestHelper.DefaultGenericClassName}_1.dll";

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
            var selectorType = AssetDatabase.LoadAssetAtPath<MonoScript>(DefaultSelectorPath).GetClass();
            _testGameObject.AddComponent(selectorType);
        }

        [UnityTearDown, UsedImplicitly]
        public IEnumerator AfterEachTest()
        {
            AssetDatabase.DeleteAsset(TestHelper.TestingDir);
            AssetDatabase.Refresh();
            yield return new RecompileScripts(false);
            yield return null;
            yield return new RecompileScripts(false);
            TestHelper.AssertNumberOfLogs(expectedWarningLogs: 0);
        }

        [Test]
        public void Selector_is_added_when_generic_behaviour_script_is_added()
        {
            Assert.That(File.Exists(DefaultSelectorPath));

            var menuPaths = Unsupported.GetSubmenus("Component");
            Assert.That(menuPaths.Contains($"Component/Scripts/{TestHelper.DefaultGenericClassName}<T>"));
        }

        [UnityTest]
        public IEnumerator Selector_is_removed_when_generic_behaviour_script_is_removed()
        {
            TestHelper.RemoveScript(TestHelper.DefaultGenericClassName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            AssertThatSelectorIsRemoved();
        }

        [UnityTest]
        public IEnumerator Selector_is_updated_when_generic_behaviour_name_changes_but_guid_does_not()
        {
            TestHelper.ChangeTypeAndFileName(TestHelper.DefaultGenericClassName, NewBehaviourName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            AssertThatSelectorIsUpdated(NewBehaviourName, "T");
        }

        [UnityTest]
        public IEnumerator Selector_is_updated_when_generic_behaviour_guid_changes_but_name_does_not()
        {
            TestHelper.ChangeGUID(TestHelper.DefaultGenericClassName);

            yield return new WaitForDomainReload();
            yield return null;

            AssertThatSelectorIsUpdated(TestHelper.DefaultGenericClassName, "T");
        }

        [UnityTest]
        public IEnumerator Selector_is_updated_when_type_name_changes_but_script_name_and_guid_does_not()
        {
            TestHelper.ChangeTypeNameOnly(TestHelper.DefaultGenericClassName, NewBehaviourName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            AssertThatSelectorIsUpdated(NewBehaviourName, "T");
        }

        [UnityTest]
        public IEnumerator Selector_is_updated_when_generic_argument_changes()
        {
            const string newArgName = "TNew";

            TestHelper.ChangeArgumentName(TestHelper.DefaultGenericClassName, newArgName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            AssertThatSelectorIsUpdated(TestHelper.DefaultGenericClassName, newArgName);
        }

        [UnityTest]
        public IEnumerator Selector_changes_when_type_name_and_guid_are_different()
        {
            TestHelper.ChangeTypeNameOnly(TestHelper.DefaultGenericClassName, NewBehaviourName);
            TestHelper.ChangeGUID(TestHelper.DefaultGenericClassName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            AssertThatSelectorChanges(TestHelper.DefaultGenericClassName, NewBehaviourName);
        }

        private void AssertThatSelectorIsRemoved()
        {
            Assert.IsFalse(File.Exists(DefaultSelectorPath));

            var menuPaths = Unsupported.GetSubmenus("Component");
            Assert.IsFalse(menuPaths.Contains($"Component/Scripts/{TestHelper.DefaultGenericClassName}<T>"));

            var components = _testGameObject.GetComponents<Component>();
            Assert.That(components.Length, Is.EqualTo(2));
            Assert.That(components[1], Is.Null);
        }

        private void AssertThatSelectorIsUpdated(string className, string argName)
        {
            Assert.That(File.Exists($"{Config.BehaviourSelectorsPath}/{className}_1.dll"));

            var menuPaths = Unsupported.GetSubmenus("Component");
            Assert.That(menuPaths.Contains($"Component/Scripts/{className}<{argName}>"));

            var components = _testGameObject.GetComponents<Component>();
            Assert.That(components.Length, Is.EqualTo(2));
            Assert.That(components[1], Is.Not.Null);
        }

        private void AssertThatSelectorChanges(string oldClassname, string newClassName)
        {
            Assert.IsFalse(File.Exists($"{Config.BehaviourSelectorsPath}/{oldClassname}_1.dll"));
            Assert.That(File.Exists($"{Config.BehaviourSelectorsPath}/{newClassName}_1.dll"));

            var menuPaths = Unsupported.GetSubmenus("Component");
            Assert.IsFalse(menuPaths.Contains($"Component/Scripts/{oldClassname}<T>"));
            Assert.That(menuPaths.Contains($"Component/Scripts/{newClassName}<T>"));

            var components = _testGameObject.GetComponents<Component>();
            Assert.That(components.Length, Is.EqualTo(2));
            Assert.That(components[1], Is.Null);
        }
    }
}