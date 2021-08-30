namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.IO;
    using UnityEngine.Assertions;
    using UnityEditor;
    using UnityEngine;

    internal static class IconFinder
    {
        public static bool TryGetCustomIcon(string genericTypeGUID, out Texture2D customIcon, bool isScriptableObject)
        {
            customIcon = null;

            string assetPath = AssetDatabase.GUIDToAssetPath(genericTypeGUID);

            if (string.IsNullOrEmpty(assetPath))
                return false;

            if (TryGetCustomIcon(assetPath, out customIcon))
                return true;

            // If generated type inherits from MonoBehaviour, a default script icon must be set, but for scriptable
            // objects, it is not the case.
            return ! isScriptableObject && TryGetDefaultIconFromMonoBehaviourScript(assetPath, out customIcon);
        }

        private static bool TryGetDefaultIconFromMonoBehaviourScript(string assetPath, out Texture2D customIcon)
        {
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

            if (monoScript is null)
            {
                customIcon = null;
                return false;
            }

            customIcon = AssetPreview.GetMiniThumbnail(monoScript);
            return true;
        }

        private static bool TryGetCustomIcon(string assetPath, out Texture2D customIcon)
        {
            // Unity doesn't expose any method to get icon guid or Texture2D from MonoScript, so we have to parse the .meta file manually.
            // AssetPreview.GetMiniThumbnail returns the icon, but in 2021 it has the DontSave flag and can't be used to set as a custom icon for an asset.

            string iconLine = GetIconLine(assetPath);

            if (iconLine == null)
            {
                customIcon = null;
                return false;
            }

            if (!GetGUIDFromIconLine(iconLine, out string guid))
            {
                customIcon = null;
                return false;
            }

            string texturePath = AssetDatabase.GUIDToAssetPath(guid);
            customIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            return customIcon != null;
        }

        private static bool GetGUIDFromIconLine(string iconLine, out string guid)
        {
            int guidIndex = iconLine.IndexOf("guid: ", StringComparison.Ordinal);

            // There may be instanceID instead of guid on the icon line.
            if (guidIndex == -1 || iconLine.Length < guidIndex + 37)
            {
                guid = null;
                return false;
            }

            guid = iconLine.Substring(guidIndex + 6, 32);
            return true;
        }

        private static string GetIconLine(string assetPath)
        {
            string[] metaContentLines = File.ReadAllLines($"{assetPath}.meta");

            foreach (string line in metaContentLines)
            {
                if (line.StartsWith("  icon: "))
                    return line;
            }

            return null;
        }
    }
}