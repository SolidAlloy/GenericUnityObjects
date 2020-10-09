using SolidUtilities.Editor.EditorWindows;
using UnityEditor;
using UnityEngine;

namespace GenericScriptableObjects
{
    public class CustomGeneric<T> : Generic<T>
    {
        public T VariableTypeField;

        [MenuItem("test")]
        protected override void CreateAsset()
        {
            base.CreateAsset();
        }
    }
}