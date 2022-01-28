namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;
    using ObjectSelector = UnityEditor.ObjectSelector;

    internal static class ObjectSelectorExtensions
    {
        /// <summary>
        /// This extension method is an alternative of <see cref="ObjectSelector.Show"/> that accepts a generic type
        /// and draws it correctly.
        /// </summary>
        public static void ShowGeneric(this ObjectSelector this_, Object obj, Object objectBeingEdited, Type requiredType, bool allowSceneObjects, string niceTypeName, Action<Object> onObjectSelectedUpdated = null)
        {
            this_.m_ObjectSelectorReceiver = null;
            this_.m_IsShowingAssets = true;
            this_.m_SkipHiddenPackages = true;
            this_.m_AllowedIDs = null;
            this_.m_ObjectBeingEdited = objectBeingEdited;

            // ReSharper disable once Unity.NoNullPropagation
            // Unity devs know better
            this_.m_LastSelectedInstanceId = obj?.GetInstanceID() ?? 0;

            this_.m_OnObjectSelectorClosed = null;
            this_.m_OnObjectSelectorUpdated = onObjectSelectedUpdated;

            // Do not allow to show scene objects if the object being edited is persistent
            this_.m_AllowSceneObjects = allowSceneObjects && ! typeof(ScriptableObject).IsAssignableFrom(requiredType)
                                        && (this_.m_ObjectBeingEdited == null
                                            || !EditorUtility.IsPersistent(this_.m_ObjectBeingEdited));

            // Set which tab should be visible at startup
            if (this_.m_AllowSceneObjects)
            {
                if (obj != null)
                {
                    if (obj is Component component)
                    {
                        obj = component.gameObject;
                    }

                    // Set the right tab visible (so we can see our selection)
                    this_.m_IsShowingAssets = EditorUtility.IsPersistent(obj);
                }
                else
                {
                    this_.m_IsShowingAssets = ! typeof(Component).IsAssignableFrom(requiredType);
                }
            }
            else
            {
                this_.m_IsShowingAssets = true;
            }

            // Set member variables
            this_.m_DelegateView = GUIView.current;
            // type filter requires unqualified names for built-in types, but will prioritize them over user types, so ensure user types are namespace-qualified

            // Set required type throw reflection to avoid ambiguity between m_RequiredType and m_RequiredTypes
            var requiredTypeStr = requiredType.IsGenericType ? GenericTypeHelper.GetConcreteType(requiredType).FullName : requiredType.FullName;
            this_.SetRequiredType(requiredTypeStr);

            this_.m_SearchFilter = string.Empty;
            this_.m_OriginalSelection = obj;
            this_.m_ModalUndoGroup = Undo.GetCurrentGroup();

            // Show custom selector if available

            // It seems this branch isn't used for generic unity objects, but it causes issues because ObjectSelectorSearch
            // is named differently in Unity2020 and 2021. Disabling it doesn't seem to break anything.
            /*if (ObjectSelectorSearch.HasEngineOverride())
            {
                this_.m_SearchSessionHandler.BeginSession(() =>
                {
                    Func<ObjectSelectorTargetInfo, Object[], ObjectSelectorSearchContext, bool> selectorConstraint = null;

                    if (this_.m_EditedProperty != null)
                    {
                        selectorConstraint = this_.GetSelectorHandlerFromProperty(this_.m_EditedProperty);
                    }

                    return new ObjectSelectorSearchContext
                    {
                        currentObject = obj,
                        editedObjects = this_.m_EditedProperty != null ? this_.m_EditedProperty.serializedObject.targetObjects : new[] { objectBeingEdited },
                        requiredTypes = new[] { requiredType },
                        requiredTypeNames = new[] { this_.m_RequiredType },
                        allowedInstanceIds = null,
                        visibleObjects = allowSceneObjects ? VisibleObjects.All : VisibleObjects.Assets,
                        selectorConstraint = selectorConstraint
                    };
                });

                var searchContext = (ObjectSelectorSearchContext)this_.m_SearchSessionHandler.context;

                void OnSelectionChanged(Object selectedObj)
                {
                    this_.m_LastSelectedInstanceId = selectedObj == null ? 0 : selectedObj.GetInstanceID();
                    this_.NotifySelectionChanged(false);
                }

                void OnSelectorClosed(Object selectedObj, bool canceled)
                {
                    this_.m_SearchSessionHandler.EndSession();

                    if (canceled)
                    {
                        // Undo changes we have done in the ObjectSelector
                        Undo.RevertAllDownToGroup(this_.m_ModalUndoGroup);
                        this_.m_LastSelectedInstanceId = 0;
                    }
                    else
                    {
                        this_.m_LastSelectedInstanceId = selectedObj == null ? 0 : selectedObj.GetInstanceID();
                    }

                    this_.m_EditedProperty = null;
                    this_.NotifySelectorClosed(false);
                }

                if (ObjectSelectorSearch.SelectObject(searchContext, OnSelectorClosed, OnSelectionChanged))
                    return;
            }*/

            // Freeze to prevent flicker on OSX.
            // Screen will be updated again when calling
            // SetFreezeDisplay(false) further down.
            ContainerWindow.SetFreezeDisplay(true);

            this_.ShowWithMode(ShowMode.AuxWindow);

            this_.titleContent = EditorGUIUtility.TrTextContent("Select " + niceTypeName);

            // Deal with window size
            Rect p = this_.m_Parent == null ? new Rect(0, 0, 1, 1) : this_.m_Parent.window.position;
            p.width = EditorPrefs.GetFloat("ObjectSelectorWidth", 200);
            p.height = EditorPrefs.GetFloat("ObjectSelectorHeight", 390);
            this_.position = p;
            this_.minSize = new Vector2(ObjectSelector.kMinWidth, ObjectSelector.kMinTopSize + ObjectSelector.kPreviewExpandedAreaHeight + 2 * ObjectSelector.kPreviewMargin);
            this_.maxSize = new Vector2(10000, 10000);
            this_.SetupPreview();

            // Focus
            this_.Focus();
            ContainerWindow.SetFreezeDisplay(false);

            this_.m_FocusSearchFilter = true;

            // Add after unfreezing display because AuxWindowManager.cpp assumes that aux windows are added after we get 'got/lost'- focus calls.
            if (this_.m_Parent != null)
                this_.m_Parent.AddToAuxWindowList();

            // Initial selection
            int initialSelection = obj != null ? obj.GetInstanceID() : 0;

            if (initialSelection != 0)
            {
                var assetPath = AssetDatabase.GetAssetPath(initialSelection);
                if (this_.m_SkipHiddenPackages && !PackageManagerUtilityInternal.IsPathInVisiblePackage(assetPath))
                    this_.m_SkipHiddenPackages = false;
            }

            if (ObjectSelector.ShouldTreeViewBeUsed(requiredTypeStr))
            {
                this_.m_ObjectTreeWithSearch.Init(this_.position, this_, this_.CreateAndSetTreeView, this_.TreeViewSelection, this_.ItemWasDoubleClicked, initialSelection, 0);
            }
            else
            {
                // To frame the selected item we need to wait to initialize the search until our window has been setup
                this_.InitIfNeeded();
                this_.m_ListArea.InitSelection(new[] { initialSelection });
                if (initialSelection != 0)
                    this_.m_ListArea.Frame(initialSelection, true, false);
            }
        }

        private static void SetRequiredType(this ObjectSelector this_, string type)
        {
            var requiredTypeField = typeof(ObjectSelector).GetField("m_RequiredType", BindingFlags.NonPublic | BindingFlags.Instance);

            if (requiredTypeField != null)
            {
                requiredTypeField.SetValue(this_, type);
                return;
            }

            const string requiredTypesFieldName = "m_RequiredTypes";
            requiredTypeField = typeof(ObjectSelector).GetField(requiredTypesFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (requiredTypeField == null)
            {
                Debug.LogWarning($"{requiredTypesFieldName} field was not found in the ObjectSelector class.");
                return;
            }

            requiredTypeField.SetValue(this_, new[] { type });
        }
    }
}