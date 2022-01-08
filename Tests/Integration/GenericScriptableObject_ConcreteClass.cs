namespace GenericUnityObjects.IntegrationTests
{
    using System;
    using System.Collections;
    using System.IO;
    using JetBrains.Annotations;
    using NUnit.Framework;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine.TestTools;
    using Util;

    public class GenericScriptableObject_ConcreteClass
    {
        private const string ZeroWarnings = "ZeroWarnings";
        private const string SkipDefaultAssetCreation = "SkipDefaultAssetCreation";
        private const string NewTypeName = "NewName";
        private static readonly Type _defaultArgumentType = typeof(int);

        // UnitySetUp is required because SetUp runs each time after domain reload which is unwanted for UnityTest methods.
        [UnitySetUp, UsedImplicitly]
        public IEnumerator BeforeEachTest()
        {
            LogHelper.Clear();

            TestHelper.CreateFolder(TestHelper.TestingDir);

            if (ShouldSkipDefaultAssetCreation())
                yield break;

            TestHelper.AddScriptableObjectScript(1);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.OpenTestingFolder();
            TestHelper.TriggerAssetCreation(_defaultArgumentType);

            yield return new WaitForDomainReload();
            yield return null;

            TestHelper.FinishInteractiveCreation();
        }

        [UnityTearDown, UsedImplicitly]
        public IEnumerator AfterEachTest()
        {
            AssetDatabase.DeleteAsset(TestHelper.TestingDir);
            AssetDatabase.Refresh();
            yield return new RecompileScripts(false);
            yield return null;
            yield return new RecompileScripts(false);

            if (TestContext.CurrentContext.Test.Properties["Category"].Contains(ZeroWarnings))
            {
                TestHelper.AssertNumberOfLogs(expectedWarningLogs: 0);
            }
            else
            {
                TestHelper.AssertNumberOfLogs();
            }
        }

        [Test, Category(ZeroWarnings)]
        public void Concrete_class_dll_is_added_when_concrete_class_is_instantiated_for_the_first_time()
        {
            Assert.That(File.Exists($"{Config.ScriptableObjectsPath}/{TestHelper.DefaultGenericClassName}1_{_defaultArgumentType.Name}.dll"));
        }

        [Test, Category(ZeroWarnings)]
        public void Assembly_does_not_reload_when_concrete_class_is_instantiated_for_the_second_time()
        {
            TestHelper.TriggerAssetCreation(_defaultArgumentType);

            Assert.IsFalse(EditorApplication.isCompiling);

            TestHelper.FinishInteractiveCreation();
        }

        [Test, Category(ZeroWarnings)]
        public void Asset_is_created_with_the_default_name_when_CreateGenericAssetMenu_is_empty()
        {
            Assert.That(File.Exists(TestHelper.DefaultAssetPath));
        }

        [UnityTest, Category(SkipDefaultAssetCreation), Category(ZeroWarnings)]
        public IEnumerator Asset_is_created_with_the_custom_name_when_FileName_is_specified_in_the_attribute()
        {
            const string expectedCustomName = "Custom Name";

            TestHelper.AddScriptableObjectScript(1, true, $"FileName = \"{expectedCustomName}\"");

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.OpenTestingFolder();
            TestHelper.TriggerAssetCreation(_defaultArgumentType);

            yield return new WaitForDomainReload();
            yield return null;

            TestHelper.FinishInteractiveCreation();

            Assert.That(File.Exists($"{TestHelper.TestingDir}/{expectedCustomName}.asset"));
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_type_name_changes_and_guid_is_same()
        {
            TestHelper.ChangeTypeAndFileName($"{TestHelper.DefaultGenericClassName}1", NewTypeName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Type genericType = TestHelper.GetTestType($"{NewTypeName}`1");
            TestHelper.ScriptableObject.AssertThatConcreteClassIsUpdated(genericType, _defaultArgumentType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_guid_changes_but_type_name_is_same()
        {
            TestHelper.ChangeGUID($"{TestHelper.DefaultGenericClassName}1");

            yield return new WaitForDomainReload();
            yield return null;

            Type genericType = TestHelper.GetTestType($"{TestHelper.DefaultGenericClassName}1`1");
            TestHelper.ScriptableObject.AssertThatConcreteClassIsUpdated(genericType, _defaultArgumentType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_type_name_changes_but_script_name_and_guid_does_not()
        {
            TestHelper.ChangeTypeNameOnly($"{TestHelper.DefaultGenericClassName}1", NewTypeName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Type genericType = TestHelper.GetTestType($"{NewTypeName}`1");
            TestHelper.ScriptableObject.AssertThatConcreteClassIsUpdated(genericType, _defaultArgumentType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_generic_argument_name_changes()
        {
            const string newArgName = "TTest";

            TestHelper.ChangeArgumentName($"{TestHelper.DefaultGenericClassName}1", newArgName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Type genericType = TestHelper.GetTestType($"{TestHelper.DefaultGenericClassName}1`1");
            TestHelper.ScriptableObject.AssertThatConcreteClassIsUpdated(genericType, _defaultArgumentType);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_removed_when_type_is_removed()
        {
            TestHelper.RemoveScript($"{TestHelper.DefaultGenericClassName}1");

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.ScriptableObject.AssertThatConcreteClassIsRemoved(_defaultArgumentType.Name);
        }

        [UnityTest]
        public IEnumerator Concrete_class_changes_when_type_name_and_guid_are_different()
        {
            TestHelper.ChangeTypeNameOnly($"{TestHelper.DefaultGenericClassName}1", NewTypeName);
            TestHelper.ChangeGUID($"{TestHelper.DefaultGenericClassName}1");

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.ScriptableObject.AssertThatConcreteClassChanged(NewTypeName, _defaultArgumentType);
        }

        private static bool ShouldSkipDefaultAssetCreation()
        {
            return TestContext.CurrentContext.Test.Properties["Category"].Contains(SkipDefaultAssetCreation);
        }
    }
}