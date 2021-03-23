namespace GenericUnityObjects.Editor.ScriptableObjects.SelectionWindow
{
    using System;

    internal interface ITypeSelectionWindow
    {
        void OnCreate(Action<Type[]> onTypesSelected, string[] genericArgNames, Type[][] genericParamConstraints);
    }
}