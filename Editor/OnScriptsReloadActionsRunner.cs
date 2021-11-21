﻿namespace GenericUnityObjects.Editor
{
    using System;
    using GenericUnityObjects.Util;
    using UnityEditor.Callbacks;
    using Util;

    internal static class OnScriptsReloadActionsRunner
    {
        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if (PersistentStorage.DelayActionsOnScriptsReload)
            {
                PersistentStorage.DelayActionsOnScriptsReload = false;
                EditorCoroutineHelper.DelayFrames(OnScriptsReload, 1);
                return;
            }


            // 1. GenericTypesAnalyzer finds a new generic MonoBehaviour and generates a DLL.
            // 2. Scripts are recompiled because of the DLL addition.
            // 3. This event is fired before GenericTypesAnalyzer is called again, and sets the custom icon.
            // Icons cannot be set immediately after generating a DLL because MonoScript is not generated by Unity yet.
            IconSetter.SetIcons();

            try
            {
                GenericTypesAnalyzer.AnalyzeGenericTypes();
            }
            catch (ApplicationException)
            {
                DebugUtility.Log("ApplicationException was thrown even though it is not expected here. Running the method again in 1 second.");
                EditorCoroutineHelper.Delay(OnScriptsReload, 1f);
                return;
            }

            PersistentStorage.OnScriptsReload();
        }
    }
}