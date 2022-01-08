namespace GenericUnityObjects.IntegrationTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Editor.ScriptableObjects;
    using Editor.ScriptableObjects.SelectionWindow;
    using NUnit.Framework;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Util;

    public static class TestHelper
    {
        public const string TestingDir = "Assets/Testing";
        public const string DefaultGenericClassName = "GenericTest";
        public const string DefaultAssetPath = TestingDir + "/New " + DefaultGenericClassName + "1`1.asset";

        public static string GetAssetPath(int number)
        {
            return $"{TestingDir}/{DefaultGenericClassName}{number}.cs";
        }

        public static void AddScriptableObjectScript(int scriptNum, bool withAttribute = true, params string[] genericAssetMenuParams)
        {
            string usingLine = "using GenericUnityObjects;\n";
            string attributeLine = $"[CreateGenericAssetMenu({string.Join(", ", genericAssetMenuParams)})]\n";
            string classLine = $"public class {DefaultGenericClassName}{scriptNum}<T> : GenericScriptableObject {{}}";

            string content = withAttribute ? usingLine + attributeLine + classLine : usingLine + classLine;

            File.WriteAllText(GetAssetPath(scriptNum), content);
            AssetDatabase.Refresh();
        }

        public static void AddBehaviourScript()
        {
            string usingLine = "using UnityEngine;\n";
            string classLine = $"public class {DefaultGenericClassName}<T> : MonoBehaviour {{}}";

            string content = usingLine + classLine;

            File.WriteAllText($"{TestingDir}/{DefaultGenericClassName}.cs", content);
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

        public static void ChangeGUID(string className)
        {
            string content = File.ReadAllText($"{TestingDir}/{className}.cs");
            File.WriteAllText($"{TestingDir}/{className}`1.cs", content);
            AssetDatabase.DeleteAsset($"{TestingDir}/{className}.cs");
            AssetDatabase.Refresh();
        }

        public static void ChangeTypeNameOnly(string oldName, string newName)
        {
            string assetPath = $"{TestingDir}/{oldName}.cs";
            string content = File.ReadAllText(assetPath);
            content = content.Replace(oldName, newName);
            File.WriteAllText(assetPath, content);
            AssetDatabase.Refresh();
        }

        public static void ChangeArgumentName(string className, string newArgumentName)
        {
            string path = $"{TestingDir}/{className}.cs";

            string content = File.ReadAllText(path);
            content = content.Replace("<T>", $"<{newArgumentName}>");
            File.WriteAllText(path, content);
            AssetDatabase.Refresh();
        }

        public static void RemoveScript(string scriptName)
        {
            AssetDatabase.DeleteAsset($"{TestingDir}/{scriptName}.cs");
            AssetDatabase.Refresh();
        }

        public static bool ValidateMenuItem(string menuItem)
        {
            MethodInfo validateMenuItem =
                typeof(EditorApplication).GetMethod("ValidateMenuItem", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(validateMenuItem);

            return (bool) validateMenuItem.Invoke(null, new object[] { menuItem });
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
            GenericSOCreator.CreateAssetInteractively(GetDefaultGenericType(), new[] { typeToSet }, $"New {DefaultGenericClassName}1`1");
        }

        private static Type GetDefaultGenericType()
        {
            var csharpAssembly = Assembly.Load("Assembly-CSharp");
            return csharpAssembly.GetType($"{DefaultGenericClassName}1`1");
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

        public static void AssertNumberOfLogs(int expectedErrorLogs = 0, int expectedWarningLogs = 1)
        {
            (int errorCount, int warningCount, int _) = LogHelper.GetCountByType();

            Assert.That(errorCount, Is.EqualTo(expectedErrorLogs));

            // Only one warning must exist after the type is removed:
            // GameObject (named 'New Game Object') references runtime script in scene file. Fixing!
            Assert.That(warningCount, Is.EqualTo(expectedWarningLogs));
        }

        public static class ScriptableObject
        {
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

            public static void AssertThatConcreteClassIsRemoved(string argName, string secondArgName = null)
            {
                // dll does not exist
                Assert.IsFalse(File.Exists($"{Config.ScriptableObjectsPath}/{DefaultGenericClassName}1_{argName}.dll"));

                if (secondArgName != null)
                    Assert.IsFalse(File.Exists($"{Config.ScriptableObjectsPath}/{DefaultGenericClassName}1_{secondArgName}.dll"));

                // LoadAssetAtPath is null
                var asset = AssetDatabase.LoadMainAssetAtPath(DefaultAssetPath);
                Assert.IsNull(asset);
            }

            public static void AssertThatConcreteClassChanged(string newTypeName, Type argType)
            {
                // old dll does not exist
                Assert.IsFalse(File.Exists($"{Config.ScriptableObjectsPath}/{DefaultGenericClassName}1_{argType.Name}.dll"));

                // LoadAssetAtPath is null
                var asset = AssetDatabase.LoadMainAssetAtPath(DefaultAssetPath);
                Assert.IsNull(asset);

                // new type is added to menu items
                Assert.IsTrue(ValidateMenuItem($"Assets/Create/{newTypeName}<T>"));
            }
        }

        public static class Behaviour
        {
            public static void AssertThatConcreteClassIsUpdated(GameObject testGameObject, string className, string argName, Type argType)
            {
                var genericType = GetTestType($"{className}`1")
                    .MakeGenericType(argType);

                bool success = BehavioursDatabase.TryGetConcreteType(genericType, out Type concreteType);
                Assert.That(success, Is.True);
                Assert.That(concreteType, Is.Not.Null);

                Assert.That(testGameObject.GetGenericComponent(genericType), Is.Not.Null);

                var menuPaths = Unsupported.GetSubmenus("Component");
                Assert.That(menuPaths.Contains($"Component/Scripts/{className}<{argName}>"));
            }

            public static void AssertThatConcreteClassIsRemoved(GameObject testGameObject, string className, string fullArgName, string shortArgName)
            {
                Assert.IsFalse(File.Exists($"{Config.MonoBehavioursPath}/{className}_{fullArgName}.dll"));

                var menuPaths = Unsupported.GetSubmenus("Component");
                Assert.IsFalse(menuPaths.Contains($"Component/Scripts/{className}<{shortArgName}>"));

                var components = testGameObject.GetComponents<Component>();
                Assert.That(components.Length, Is.EqualTo(2));
                Assert.That(components[1], Is.Null);
            }

            public static void AssertThatConcreteClassChanged(GameObject testGameObject, string oldClassName, string newClassName, string fullArgName, string shortArgName)
            {
                AssertThatConcreteClassIsRemoved(testGameObject, oldClassName, fullArgName, shortArgName);

                Assert.That(File.Exists($"{Config.BehaviourSelectorsPath}/{newClassName}_1.dll"));

                var menuPaths = Unsupported.GetSubmenus("Component");
                Assert.That(menuPaths.Contains($"Component/Scripts/{newClassName}<T>"));
            }
        }
    }
}