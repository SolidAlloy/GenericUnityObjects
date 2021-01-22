namespace GenericUnityObjects.EditorTests.Integration
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Remoting.Contexts;
    using JetBrains.Annotations;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Util;

    public class MenuItems : ScriptableObject
    {
        private static readonly string _menuItemsDLLPath = $"{Config.AssembliesDirPath}/{Config.MenuItemsAssemblyName}.dll";

        [UnitySetUp, UsedImplicitly]
        public IEnumerator BeforeEachTest()
        {
            Directory.CreateDirectory(IntegrationTestHelper.TestingDir);
            IntegrationTestHelper.AddScript(1);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();
        }

        [UnityTearDown, UsedImplicitly]
        public IEnumerator AfterEachTest()
        {
            AssetDatabase.DeleteAsset(IntegrationTestHelper.TestingDir);

            DirectoryInfo dirInfo = new DirectoryInfo(Config.AssembliesDirPath);

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }

            AssetDatabase.Refresh();
            yield return new RecompileScripts(false);
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

            IntegrationTestHelper.AddScript(2);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            string newGUID = AssetDatabase.AssetPathToGUID(_menuItemsDLLPath);
            Assert.AreEqual(PlayerPrefs.GetString("previousGUID"), newGUID);
        }

        private bool ValidateMenuItem(string menuItem)
        {
            MethodInfo validateMenuItem =
                typeof(EditorApplication).GetMethod("ValidateMenuItem", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(validateMenuItem);

            return (bool) validateMenuItem.Invoke(null, new object[] { menuItem });
        }

        [Test]
        public void Adding_generic_scriptable_object_adds_an_entry_to_Assets_Create_menu()
        {
            Assert.IsTrue(ValidateMenuItem($"Assets/Create/{IntegrationTestHelper.DefaultClassName}1<T>"));
        }

        [UnityTest]
        public IEnumerator Removing_a_single_generic_scriptable_object_removes_MenuItems_dll()
        {
            IntegrationTestHelper.RemoveScript(1);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.IsFalse(File.Exists(_menuItemsDLLPath));
        }

        [UnityTest]
        public IEnumerator Removing_second_generic_scriptable_object_does_not_remove_MenuItems_dll()
        {
            IntegrationTestHelper.AddScript(2);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            IntegrationTestHelper.RemoveScript(2);

            yield return new WaitForDomainReload();
            yield return null;

            // At this moment, domain reload is expected due to method removal from MenuItems.dll.
            // I don't know, why it doesn't happen. It may be because the GUID of the .dll doesn't change.
            yield return new RecompileScripts(false);

            Assert.IsTrue(File.Exists(_menuItemsDLLPath));
        }

        [UnityTest]
        public IEnumerator Removing_generic_scriptable_object_removes_entry_from_Assets_Create_menu()
        {
            IntegrationTestHelper.RemoveScript(1);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            string menuItem = $"Assets/Create/{IntegrationTestHelper.DefaultClassName}1<T>";

            Assert.IsFalse(ValidateMenuItem(menuItem));
            LogAssert.Expect(LogType.Error, $"ValidateMenuItem failed because there is no menu named {menuItem}");
        }

        [UnityTest]
        public IEnumerator Removing_CreateGenericAssetMenu_attribute_from_class_removes_MenuItems_dll()
        {
            IntegrationTestHelper.AddScript(1, false);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.False(File.Exists(_menuItemsDLLPath));
        }

        [UnityTest]
        public IEnumerator Adding_CreateGenericAssetMenu_attribute_to_existing_class_creates_MenuItems_dll()
        {
            IntegrationTestHelper.RemoveScript(1);
            IntegrationTestHelper.AddScript(2, false);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            IntegrationTestHelper.AddScript(2, true);

            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.True(File.Exists(_menuItemsDLLPath));
        }

        [UnityTest]
        public IEnumerator Updating_menu_name_in_attribute_updates_menu_entry()
        {
            const string newMenuName = "New Name";

            IntegrationTestHelper.AddScript(1, true, $"MenuName = \"{newMenuName}\"");
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.IsTrue(ValidateMenuItem($"Assets/Create/{newMenuName}"));
        }

        [UnityTest]
        public IEnumerator Updating_type_name_updates_menu_entry()
        {
            const string newTypeName = "NewName";

            IntegrationTestHelper.ChangeTypeAndFileName(1, newTypeName);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.IsTrue(ValidateMenuItem($"Assets/Create/{newTypeName}1<T>"));
        }

        [UnityTest]
        public IEnumerator Updating_generic_type_name_updates_menu_entry()
        {
            const string newArgumentName = "TNew";

            IntegrationTestHelper.ChangeArgumentName(1, newArgumentName);
            yield return new WaitForDomainReload();
            yield return null;
            yield return new WaitForDomainReload();

            Assert.IsTrue(ValidateMenuItem($"Assets/Create/{IntegrationTestHelper.DefaultClassName}1<{newArgumentName}>"));
        }
    }
}