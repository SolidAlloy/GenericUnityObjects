namespace GenericUnityObjects.IntegrationTests
{
    using System;
    using System.Collections;
    using JetBrains.Annotations;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine.TestTools;

    public class GenericScriptableObject_GenericArguments
    {
        private const string DefaultArgName = "CustomArg";
        private const string NewArgName = "NewArg";

        [UnitySetUp, UsedImplicitly]
        public IEnumerator BeforeEachTest()
        {
            LogHelper.Clear();

            TestHelper.CreateFolder(TestHelper.TestingDir);

            TestHelper.AddArgumentScript(DefaultArgName);

            TestHelper.AddScriptableObjectScript(1);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.OpenTestingFolder();

            Type argType = TestHelper.GetTestType(DefaultArgName);
            TestHelper.TriggerAssetCreation(argType);

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
            TestHelper.AssertNumberOfLogs();
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_argument_type_name_changes_but_guid_does_not()
        {
            TestHelper.ChangeTypeAndFileName(DefaultArgName, NewArgName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            AssertThatConcreteClassIsUpdated(NewArgName);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_argument_guid_changes_but_type_name_does_not()
        {
            TestHelper.ChangeGUID(DefaultArgName);

            yield return new WaitForDomainReload();
            yield return null;

            AssertThatConcreteClassIsUpdated(DefaultArgName);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_type_name_of_generic_argument_changes_but_script_name_and_guid_does_not()
        {
            TestHelper.ChangeTypeNameOnly(DefaultArgName, NewArgName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            AssertThatConcreteClassIsUpdated(NewArgName);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_removed_when_generic_argument_type_and_guid_are_different()
        {
            TestHelper.ChangeTypeNameOnly(DefaultArgName, NewArgName);
            TestHelper.ChangeGUID(DefaultArgName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.ScriptableObject.AssertThatConcreteClassIsRemoved(DefaultArgName, NewArgName);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_removed_when_generic_argument_type_is_removed()
        {
            TestHelper.RemoveScript(DefaultArgName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.ScriptableObject.AssertThatConcreteClassIsRemoved(DefaultArgName);
        }

        private static void AssertThatConcreteClassIsUpdated(string argName)
        {
            Type genericType = TestHelper.GetTestType($"{TestHelper.DefaultGenericClassName}1`1");
            Type argType = TestHelper.GetTestType(argName);
            TestHelper.ScriptableObject.AssertThatConcreteClassIsUpdated(genericType, argType);
        }
    }
}