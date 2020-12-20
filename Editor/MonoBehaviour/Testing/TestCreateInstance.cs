namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using UnityEngine;

    public static class TestCreateInstance
    {
        public static void Create()
        {
            GenericBehavioursDatabase.CreateInstance();
        }

        public static void ClearDatabase()
        {
            GenericBehavioursDatabase.Clear();
        }
    }
}