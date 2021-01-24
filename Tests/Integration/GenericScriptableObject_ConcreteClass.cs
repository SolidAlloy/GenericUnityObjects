namespace GenericUnityObjects.EditorTests.Integration
{
    using System;
    using System.Collections;
    using System.IO;
    using JetBrains.Annotations;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine.TestTools;
    using Util;

    public class GenericScriptableObject_ConcreteClass
    {
        private static readonly Type DefaultArgumentType = typeof(int);
        private const string SkipDefaultAssetCreation = "SkipDefaultAssetCreation";
        private const string NewTypeName = "NewName";


        // UnitySetUp is required because SetUp runs each time after domain reload which is unwanted for UnityTest methods.
        [UnitySetUp, UsedImplicitly]
        public IEnumerator BeforeEachTest()
        {
            IntegrationTestHelper.CreateFolder(IntegrationTestHelper.TestingDir);

            if (ShouldSkipDefaultAssetCreation())
                yield break;

            IntegrationTestHelper.AddScript(1);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            IntegrationTestHelper.OpenTestingFolder();
            IntegrationTestHelper.TriggerAssetCreation(DefaultArgumentType);

            yield return new WaitForDomainReload();
            yield return null;

            IntegrationTestHelper.FinishInteractiveCreation();
        }

        [UnityTearDown, UsedImplicitly]
        public IEnumerator AfterEachTest()
        {
            AssetDatabase.DeleteAsset(IntegrationTestHelper.TestingDir);
            AssetDatabase.Refresh();
            yield return new RecompileScripts(false);
        }

        [Test]
        public void Concrete_class_dll_is_added_when_concrete_class_is_instantiated_for_the_first_time()
        {
            Assert.That(File.Exists($"{Config.AssembliesDirPath}/{IntegrationTestHelper.DefaultGenericClassName}1_{DefaultArgumentType.Name}.dll"));
        }

        [Test]
        public void Assembly_does_not_reload_when_concrete_class_is_instantiated_for_the_second_time()
        {
            IntegrationTestHelper.TriggerAssetCreation(DefaultArgumentType);

            Assert.IsFalse(EditorApplication.isCompiling);

            IntegrationTestHelper.FinishInteractiveCreation();
        }

        [Test]
        public void Asset_is_created_with_the_default_name_when_CreateGenericAssetMenu_is_empty()
        {
            Assert.That(File.Exists(IntegrationTestHelper.DefaultAssetPath));
        }

        [UnityTest, Category(SkipDefaultAssetCreation)]
        public IEnumerator Asset_is_created_with_the_custom_name_when_FileName_is_specified_in_the_attribute()
        {
            const string expectedCustomName = "Custom Name";

            IntegrationTestHelper.AddScript(1, true, $"FileName = \"{expectedCustomName}\"");

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            IntegrationTestHelper.OpenTestingFolder();
            IntegrationTestHelper.TriggerAssetCreation(DefaultArgumentType);

            yield return new WaitForDomainReload();
            yield return null;

            IntegrationTestHelper.FinishInteractiveCreation();

            Assert.That(File.Exists($"{IntegrationTestHelper.TestingDir}/{expectedCustomName}.asset"));
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_type_name_changes_and_guid_is_same()
        {
            IntegrationTestHelper.ChangeTypeAndFileName($"{IntegrationTestHelper.DefaultGenericClassName}1", NewTypeName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Type genericType = IntegrationTestHelper.GetTestType($"{NewTypeName}`1");
            IntegrationTestHelper.AssertThatConcreteClassIsUpdated(genericType, DefaultArgumentType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_guid_changes_but_type_name_is_same()
        {
            IntegrationTestHelper.ChangeGUID(1);

            yield return new WaitForDomainReload();
            yield return null;

            Type genericType = IntegrationTestHelper.GetTestType($"{IntegrationTestHelper.DefaultGenericClassName}1`1");
            IntegrationTestHelper.AssertThatConcreteClassIsUpdated(genericType, DefaultArgumentType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_type_name_changes_but_script_name_and_guid_does_not()
        {
            IntegrationTestHelper.ChangeTypeNameOnly(1, NewTypeName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Type genericType = IntegrationTestHelper.GetTestType($"{IntegrationTestHelper.DefaultGenericClassName}1`1");
            IntegrationTestHelper.AssertThatConcreteClassIsUpdated(genericType, DefaultArgumentType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_generic_argument_name_changes()
        {
            const string newArgName = "TTest";

            IntegrationTestHelper.ChangeArgumentName(1, newArgName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Type genericType = IntegrationTestHelper.GetTestType($"{IntegrationTestHelper.DefaultGenericClassName}1`1");
            IntegrationTestHelper.AssertThatConcreteClassIsUpdated(genericType, DefaultArgumentType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_removed_when_type_is_removed()
        {
            IntegrationTestHelper.RemoveScript(1);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            IntegrationTestHelper.AssertThatConcreteClassIsRemoved(DefaultArgumentType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_changes_when_type_name_and_guid_are_different()
        {
            IntegrationTestHelper.ChangeTypeNameOnly(1, NewTypeName);
            IntegrationTestHelper.ChangeGUID(1);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            IntegrationTestHelper.AssertThatConcreteClassChanged(NewTypeName, DefaultArgumentType);
        }

        private static bool ShouldSkipDefaultAssetCreation()
        {
            return TestContext.CurrentContext.Test.Properties["Category"].Contains(SkipDefaultAssetCreation);
        }
    }
}