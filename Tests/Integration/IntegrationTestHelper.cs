namespace GenericUnityObjects.EditorTests.Integration
{
    using System.IO;
    using UnityEditor;

    internal static class IntegrationTestHelper
    {
        public const string TestingDir = "Assets/Testing";
        public const string DefaultClassName = "GenericSOTest";

        private static string GetAssetName(int number)
        {
            return $"{DefaultClassName}{number}.cs";
        }

        public static void AddScript(int scriptNum, bool withAttribute = true, params string[] genericAssetMenuParams)
        {
            string usingLine = "using GenericUnityObjects;\n";
            string attributeLine = $"[CreateGenericAssetMenu({string.Join(", ", genericAssetMenuParams)})]\n";
            string classLine = $"public class {DefaultClassName}{scriptNum}<T> : GenericScriptableObject {{}}";

            string content = withAttribute ? usingLine + attributeLine + classLine : usingLine + classLine;

            File.WriteAllText($"{TestingDir}/{GetAssetName(scriptNum)}", content);
            AssetDatabase.Refresh();
        }

        public static void ChangeTypeAndFileName(int scriptNum, string newName)
        {
            string content = File.ReadAllText($"{TestingDir}/{GetAssetName(scriptNum)}");
            content = content.Replace(DefaultClassName, newName);
            File.Delete($"{TestingDir}/{GetAssetName(scriptNum)}");
            File.WriteAllText($"{TestingDir}/{newName}{scriptNum}.cs", content);
            File.Move($"{TestingDir}/{DefaultClassName}{scriptNum}.cs.meta", $"{TestingDir}/{newName}{scriptNum}.cs.meta");
            AssetDatabase.Refresh();
        }

        public static void ChangeGUID(int scriptNum)
        {
            string content = File.ReadAllText($"{TestingDir}/{DefaultClassName}{scriptNum}.cs");
            File.WriteAllText($"{TestingDir}/{DefaultClassName}{scriptNum}`1.cs", content);
            AssetDatabase.DeleteAsset($"{TestingDir}/{DefaultClassName}{scriptNum}.cs");
            AssetDatabase.Refresh();
        }

        public static void ChangeTypeNameOnly(int scriptNum, string newName)
        {
            string assetPath = $"{TestingDir}/{GetAssetName(scriptNum)}";
            string content = File.ReadAllText(assetPath);
            content = content.Replace(DefaultClassName, newName);
            File.WriteAllText(assetPath, content);
            AssetDatabase.Refresh();
        }

        public static void ChangeArgumentName(int scriptNum, string newArgumentName)
        {
            string content = File.ReadAllText($"{TestingDir}/{GetAssetName(scriptNum)}");
            content = content.Replace("<T>", $"<{newArgumentName}>");
            File.WriteAllText($"{TestingDir}/{GetAssetName(scriptNum)}", content);
            AssetDatabase.Refresh();
        }

        public static void RemoveScript(int scriptNum)
        {
            AssetDatabase.DeleteAsset($"{TestingDir}/{GetAssetName(scriptNum)}");
            AssetDatabase.Refresh();
        }
    }
}