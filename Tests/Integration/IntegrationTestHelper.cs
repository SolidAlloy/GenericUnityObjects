namespace GenericUnityObjects.EditorTests.Integration
{
    using System;
    using System.IO;
    using System.Reflection;
    using Editor.ScriptableObjects.SelectionWindow;
    using NUnit.Framework;
    using UnityEditor;
    using Util;

    internal static class IntegrationTestHelper
    {
        public const string TestingDir = "Assets/Testing";
        public const string DefaultGenericClassName = "GenericSOTest";
        public const string DefaultAssetPath = TestingDir + "/New " + DefaultGenericClassName + "1`1.asset";

        public static string GetAssetPath(int number)
        {
            return $"{TestingDir}/{DefaultGenericClassName}{number}.cs";
        }

        public static void AddScript(int scriptNum, bool withAttribute = true, params string[] genericAssetMenuParams)
        {
            string usingLine = "using GenericUnityObjects;\n";
            string attributeLine = $"[CreateGenericAssetMenu({string.Join(", ", genericAssetMenuParams)})]\n";
            string classLine = $"public class {DefaultGenericClassName}{scriptNum}<T> : GenericScriptableObject {{}}";

            string content = withAttribute ? usingLine + attributeLine + classLine : usingLine + classLine;

            File.WriteAllText(GetAssetPath(scriptNum), content);
            AssetDatabase.Refresh();
        }

        public static void AddArgumentScript(string argName)
        {
            File.WriteAllText($"{TestingDir}/{argName}.cs", $"[System.Serializable] public class {argName} {{ }}");
            AssetDatabase.Refresh();
        }

        public static void MakeAbstract(string className)
        {
            string filePath = $"{TestingDir}/{className}.cs";

            string content = File.ReadAllText(filePath);
            int classIndex = content.IndexOf("class", StringComparison.Ordinal);
            content = content.Substring(0, classIndex) + "abstract " +
                      content.Substring(classIndex, content.Length - classIndex);

            File.WriteAllText(filePath, content);
            AssetDatabase.Refresh();
        }

        public static void MakeNonGeneric(string className)
        {
            string filePath = $"{TestingDir}/{className}.cs";

            string content = File.ReadAllText(filePath);
            int genericArgIndex = content.IndexOf("<T>", StringComparison.Ordinal);
            int afterGenericArg = genericArgIndex + 3;

            content = content.Substring(0, genericArgIndex)
                      + content.Substring(afterGenericArg, content.Length - afterGenericArg);

            File.WriteAllText(filePath, content);
            AssetDatabase.Refresh();
        }

        public static void ChangeTypeAndFileName(string oldClassName, string newClassName)
        {
            string oldFilePath = $"{TestingDir}/{oldClassName}.cs";
            string newFilePath = $"{TestingDir}/{newClassName}.cs";

            string content = File.ReadAllText(oldFilePath);
            content = content.Replace(oldClassName, newClassName);
            File.Delete(oldFilePath);
            File.WriteAllText(newFilePath, content);
            File.Move($"{oldFilePath}.meta", $"{newFilePath}.meta");
            AssetDatabase.Refresh();
        }

        public static void ChangeGUID(int scriptNum)
        {
            string content = File.ReadAllText($"{TestingDir}/{DefaultGenericClassName}{scriptNum}.cs");
            File.WriteAllText($"{TestingDir}/{DefaultGenericClassName}{scriptNum}`1.cs", content);
            AssetDatabase.DeleteAsset($"{TestingDir}/{DefaultGenericClassName}{scriptNum}.cs");
            AssetDatabase.Refresh();
        }

        public static void ChangeTypeNameOnly(int scriptNum, string newName)
        {
            string assetPath = GetAssetPath(scriptNum);
            string content = File.ReadAllText(assetPath);
            content = content.Replace(DefaultGenericClassName, newName);
            File.WriteAllText(assetPath, content);
            AssetDatabase.Refresh();
        }

        public static void ChangeArgumentName(int scriptNum, string newArgumentName)
        {
            string content = File.ReadAllText(GetAssetPath(scriptNum));
            content = content.Replace("<T>", $"<{newArgumentName}>");
            File.WriteAllText(GetAssetPath(scriptNum), content);
            AssetDatabase.Refresh();
        }

        public static void RemoveScript(int scriptNum)
        {
            AssetDatabase.DeleteAsset(GetAssetPath(scriptNum));
            AssetDatabase.Refresh();
        }

        public static bool ValidateMenuItem(string menuItem)
        {
            MethodInfo validateMenuItem =
                typeof(EditorApplication).GetMethod("ValidateMenuItem", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(validateMenuItem);

            return (bool) validateMenuItem.Invoke(null, new object[] { menuItem });
        }

        public static void AssertThatConcreteClassIsUpdated(Type genericTypeDefinition, Type argType)
        {
            // scriptable object of new type can be instantiated
            var genericType = genericTypeDefinition.MakeGenericType(argType);
            var testInstance = GenericScriptableObject.CreateInstance(genericType);
            Assert.IsNotNull(testInstance);

            // LoadAssetAtPath is not null
            var asset = AssetDatabase.LoadMainAssetAtPath(DefaultAssetPath);
            Assert.IsNotNull(asset);
        }

        public static void AssertThatConcreteClassIsRemoved(Type argType)
        {
            // dll does not exist
            Assert.IsFalse(File.Exists($"{Config.AssembliesDirPath}/{DefaultGenericClassName}1_{argType.Name}.dll"));

            // LoadAssetAtPath is null
            var asset = AssetDatabase.LoadMainAssetAtPath(DefaultAssetPath);
            Assert.IsNull(asset);
        }

        public static void AssertThatConcreteClassChanged(string newTypeName, Type argType)
        {
            // old dll does not exist
            Assert.IsFalse(File.Exists($"{Config.AssembliesDirPath}/{DefaultGenericClassName}1_{argType.Name}.dll"));

            // LoadAssetAtPath is null
            var asset = AssetDatabase.LoadMainAssetAtPath(DefaultAssetPath);
            Assert.IsNull(asset);

            // new type is added to menu items
            Assert.IsTrue(ValidateMenuItem($"Assets/Create/{newTypeName}1<T>"));
        }

        public static void CreateFolder(string folder)
        {
            var pathParts = folder.Split(new[] { '/' }, 2);
            AssetDatabase.CreateFolder(pathParts[0], pathParts[1]);
        }

        public static void OpenTestingFolder()
        {
            var folderObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(TestingDir);
            Assert.IsNotNull(folderObject);

            var projectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            var projectBrowser = EditorWindow.GetWindow(projectBrowserType);

            // private void ShowFolderContents(int folderInstanceID, bool revealAndFrameInFolderTree)
            MethodInfo showFolderContents = projectBrowserType.GetMethod(
                "ShowFolderContents",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(int), typeof(bool) },
                null);

            Assert.NotNull(showFolderContents);

            showFolderContents.Invoke(projectBrowser, new object[] { folderObject.GetInstanceID(), true });

            string openPath = GetCurrentlyOpenPathInProject();

            Assert.AreEqual(TestingDir, openPath);
        }

        private static string GetCurrentlyOpenPathInProject()
        {
            MethodInfo getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod(
                "GetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic);

            return (string) getActiveFolderPath.Invoke(null, null);
        }


        public static void TriggerAssetCreation(Type typeToSet)
        {
            EditorApplication.ExecuteMenuItem($"Assets/Create/{DefaultGenericClassName}1<T>");

            Assert.IsTrue(EditorWindow.HasOpenInstances<OneTypeSelectionWindow>());

            var selectionWindow = EditorWindow.GetWindow<OneTypeSelectionWindow>();

            selectionWindow.OnTypeSelected(new[] { typeToSet });
        }

        public static void FinishInteractiveCreation()
        {
            Type projectBrowserType = Assembly
                .Load("UnityEditor.CoreModule")
                .GetType("UnityEditor.ProjectBrowser");

            FieldInfo lastInstanceField = projectBrowserType.GetField(
                "s_LastInteractedProjectBrowser",
                BindingFlags.Static | BindingFlags.Public);

            Assert.IsNotNull(lastInstanceField);

            object lastInstance = lastInstanceField.GetValue(null);

            Assert.IsNotNull(lastInstance, "Project window was not found in the current layout. Please add it to the editor layout.");

            MethodInfo endRenaming = projectBrowserType.GetMethod(
                "EndRenaming",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.IsNotNull(endRenaming);

            endRenaming.Invoke(lastInstance, null);
        }

        public static Type GetTestType(string typeName)
        {
            Type type = Assembly.Load("Assembly-CSharp").GetType(typeName);
            Assert.IsNotNull(type);
            return type;
        }
    }
}