namespace GenericUnityObjects.IntegrationTests
{
    using System.Collections;
    using System.IO;
    using Editor.Util;
    using JetBrains.Annotations;
    using NUnit.Framework;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Util;

    public class MenuItems : ScriptableObject
    {
        private const string OneErrorLog = "OneErrorLog";
        private static readonly string _menuItemsDLLPath = PersistentStorage.MenuItemsAssemblyPath;

        [UnitySetUp, UsedImplicitly]
        public IEnumerator BeforeEachTest()
        {
            LogHelper.Clear();

            Directory.CreateDirectory(TestHelper.TestingDir);
            TestHelper.AddScriptableObjectScript(1);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();
        }

        [UnityTearDown, UsedImplicitly]
        public IEnumerator AfterEachTest()
        {
            AssetDatabase.DeleteAsset(TestHelper.TestingDir);

            DirectoryInfo dirInfo = new DirectoryInfo(Config.GetAssemblyPathForType(null));

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }

            AssetDatabase.Refresh();
            yield return new RecompileScripts(false);
            yield return null;
            yield return new RecompileScripts(false);

            if (TestContext.CurrentContext.Test.Properties["Category"].Contains(OneErrorLog))
            {
                TestHelper.AssertNumberOfLogs(expectedErrorLogs: 1, expectedWarningLogs: 0);
            }
            else
            {
                TestHelper.AssertNumberOfLogs(expectedWarningLogs: 0);
            }
        }

        [Test]
        public void Adding_generic_scriptable_object_script_generates_MenuItems_dll_if_not_present()
        {
            string guid = AssetDatabase.AssetPathToGUID(_menuItemsDLLPath);
            Assert.IsFalse(string.IsNullOrEmpty(guid));
        }

        [UnityTest]
        public IEnumerator Adding_second_generic_scriptable_object_script_updates_MenuItems_dll_without_GUID_change()
        {
            PlayerPrefs.SetString("previousGUID", AssetDatabase.AssetPathToGUID(_menuItemsDLLPath));

            TestHelper.AddScriptableObjectScript(2);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            string newGUID = AssetDatabase.AssetPathToGUID(_menuItemsDLLPath);
            Assert.AreEqual(PlayerPrefs.GetString("previousGUID"), newGUID);
        }

        [Test]
        public void Adding_generic_scriptable_object_adds_an_entry_to_Assets_Create_menu()
        {
            Assert.IsTrue(TestHelper.ValidateMenuItem($"Assets/Create/{TestHelper.DefaultGenericClassName}1<T>"));
        }

        [UnityTest]
        public IEnumerator Removing_a_single_generic_scriptable_object_removes_MenuItems_dll()
        {
            TestHelper.RemoveScript($"{TestHelper.DefaultGenericClassName}1");
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.IsFalse(File.Exists(_menuItemsDLLPath));
        }

        [UnityTest]
        public IEnumerator Removing_second_generic_scriptable_object_does_not_remove_MenuItems_dll()
        {
            TestHelper.AddScriptableObjectScript(2);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.RemoveScript($"{TestHelper.DefaultGenericClassName}2");

            yield return new WaitForDomainReload();
            yield return null;

            // At this moment, domain reload is expected due to method removal from MenuItems.dll.
            // I don't know, why it doesn't happen. It may be because the GUID of the .dll doesn't change.
            yield return new RecompileScripts(false);

            Assert.IsTrue(File.Exists(_menuItemsDLLPath));
        }

        [UnityTest, Category(OneErrorLog)]
        public IEnumerator Removing_generic_scriptable_object_removes_entry_from_Assets_Create_menu()
        {
            TestHelper.RemoveScript($"{TestHelper.DefaultGenericClassName}1");
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            AssertMenuItemDoesNotExist();
        }

        [UnityTest]
        public IEnumerator Removing_CreateGenericAssetMenu_attribute_from_class_removes_MenuItems_dll()
        {
            TestHelper.AddScriptableObjectScript(1, false);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.False(File.Exists(_menuItemsDLLPath));
        }

        [UnityTest]
        public IEnumerator Adding_CreateGenericAssetMenu_attribute_to_existing_class_creates_MenuItems_dll()
        {
            TestHelper.RemoveScript($"{TestHelper.DefaultGenericClassName}1");
            TestHelper.AddScriptableObjectScript(2, false);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            TestHelper.AddScriptableObjectScript(2, true);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.True(File.Exists(_menuItemsDLLPath));
        }

        [UnityTest]
        public IEnumerator Updating_menu_name_in_attribute_updates_menu_entry()
        {
            const string newMenuName = "New Name";

            TestHelper.AddScriptableObjectScript(1, true, $"MenuName = \"{newMenuName}\"");
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.IsTrue(TestHelper.ValidateMenuItem($"Assets/Create/{newMenuName}"));
        }

        [UnityTest]
        public IEnumerator Updating_type_name_updates_menu_entry()
        {
            const string newTypeName = "NewName";

            TestHelper.ChangeTypeAndFileName($"{TestHelper.DefaultGenericClassName}1", newTypeName);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.IsTrue(TestHelper.ValidateMenuItem($"Assets/Create/{newTypeName}<T>"));
        }

        [UnityTest]
        public IEnumerator Updating_generic_type_name_updates_menu_entry()
        {
            const string newArgumentName = "TNew";

            TestHelper.ChangeArgumentName($"{TestHelper.DefaultGenericClassName}1", newArgumentName);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.IsTrue(TestHelper.ValidateMenuItem($"Assets/Create/{TestHelper.DefaultGenericClassName}1<{newArgumentName}>"));
        }

        [UnityTest]
        public IEnumerator Menu_entry_is_not_generated_for_abstract_class_with_CreateGenericAssetMenu_attribute()
        {
            TestHelper.MakeAbstract($"{TestHelper.DefaultGenericClassName}1");

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            AssertMenuItemDoesNotExist();
        }

        [UnityTest]
        public IEnumerator Menu_entry_is_not_generated_for_non_generic_class_with_CreateGenericAssetMenu_attribute()
        {
            TestHelper.MakeNonGeneric($"{TestHelper.DefaultGenericClassName}1");

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            AssertMenuItemDoesNotExist();
        }

        private static void AssertMenuItemDoesNotExist()
        {
            string menuItem = $"Assets/Create/{TestHelper.DefaultGenericClassName}1<T>";
            Assert.IsFalse(TestHelper.ValidateMenuItem(menuItem));
            LogAssert.Expect(LogType.Error, $"ValidateMenuItem failed because there is no menu named {menuItem}");
        }
    }
}