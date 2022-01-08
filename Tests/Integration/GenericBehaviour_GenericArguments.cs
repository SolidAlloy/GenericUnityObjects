namespace GenericUnityObjects.IntegrationTests
{
    using System;
    using System.Collections;
    using Editor.MonoBehaviours;
    using JetBrains.Annotations;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Util;

    public class GenericBehaviour_GenericArguments : ScriptableObject
    {
        private const string DefaultArgName = "CustomArg";
        private const string NewArgName = "NewArg";

        [SerializeField] private GameObject _testGameObject;

        [UnitySetUp, UsedImplicitly]
        public IEnumerator BeforeEachTest()
        {
            LogHelper.Clear();

            TestHelper.CreateFolder(TestHelper.TestingDir);

            TestHelper.AddArgumentScript(DefaultArgName);

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
                new[] { TestHelper.GetTestType(DefaultArgName) });

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
            TestHelper.AssertNumberOfLogs();
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_updated_when_argument_type_name_changes_and_guid_does_not()
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

            TestHelper.Behaviour.AssertThatConcreteClassIsRemoved(_testGameObject, TestHelper.DefaultGenericClassName, DefaultArgName, DefaultArgName);
            TestHelper.Behaviour.AssertThatConcreteClassIsRemoved(_testGameObject, TestHelper.DefaultGenericClassName, NewArgName, NewArgName);
        }

        [UnityTest]
        public IEnumerator Concrete_class_is_removed_when_generic_argument_type_is_removed()
        {
            TestHelper.RemoveScript(DefaultArgName);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.Behaviour.AssertThatConcreteClassIsRemoved(_testGameObject, TestHelper.DefaultGenericClassName, DefaultArgName, DefaultArgName);
        }

        private void AssertThatConcreteClassIsUpdated(string argName)
        {
            Type argType = TestHelper.GetTestType(argName);
            TestHelper.Behaviour.AssertThatConcreteClassIsUpdated(_testGameObject, TestHelper.DefaultGenericClassName, argName, argType);
        }
    }
}