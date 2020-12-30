namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using UnityEngine;
    using Util;

    [Serializable]
    internal class ScriptableObjectInfo : BehaviourInfo
    {
        [SerializeField] private MenuItemMethod _menuItemMethod;

        public MenuItemMethod MenuItemMethod => _menuItemMethod;

        public ScriptableObjectInfo(string typeNameAndAssembly, string guid)
            : base(typeNameAndAssembly, guid)
        {
        }

        public ScriptableObjectInfo(string typeFullName, string assemblyName, string guid)
            : base(typeFullName, assemblyName, guid)
        {
        }

        public ScriptableObjectInfo(Type type, string typeGUID = null)
            : base(type, typeGUID)
        {
        }
    }

    [Serializable]
    internal struct MenuItemMethod
    {
        [SerializeField] private string _fileName;
        [SerializeField] private string _menuName;
        [SerializeField] private int _order;

        public string FileName => _fileName;

        public string MenuName => _menuName;

        public int Order => _order;

        public MenuItemMethod(Type parentType, string fileName, string menuName, int order)
        {
            _fileName = fileName ?? $"New {parentType.Name}";
            _menuName = menuName ?? CreatorUtil.GetShortNameWithBrackets(parentType, parentType.GetGenericArguments());
            _order = order;
        }
    }
}