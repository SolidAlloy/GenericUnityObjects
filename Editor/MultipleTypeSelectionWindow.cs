namespace GenericScriptableObjects.Editor
{
    using System;
    using System.Linq;
    using SolidUtilities.Editor.EditorIconsRelated;
    using SolidUtilities.Editor.Extensions;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Extensions;
    using TypeReferences;
    using TypeReferences.Editor.TypeDropdown;
    using UnityEditor;
    using UnityEngine;
    using Util;

    public class MultipleTypeSelectionWindow : TypeSelectionWindow
    {
        [SerializeField, UseDefaultAssembly(ExcludeNone = true, SerializableOnly = true)]
        private TypeReference[] _typeRefs;

        private Action<Type[]> _onTypesSelected;
        private SerializedObject _serializedObject;
        private ContentCache _contentCache;

        private Vector2 _windowSize;

        protected override void OnCreate(Action<Type[]> onTypesSelected, int typesCount)
        {
            _onTypesSelected = onTypesSelected;
            _typeRefs = new TypeReference[typesCount];
            _serializedObject = new SerializedObject(this);
            _contentCache = new ContentCache();
            _windowSize = new Vector2(300f, GetWindowHeight(typesCount));
            this.Resize(_windowSize.x, _windowSize.y);
            this.CenterOnMainWin();
        }

        protected override void OnGUI()
        {
            DrawHeader();

            SerializedProperty typesArray = _serializedObject.FindProperty(nameof(_typeRefs));
            for (int i = 0; i < typesArray.arraySize; i++)
            {
                EditorGUILayout.PropertyField(
                    typesArray.GetArrayElementAtIndex(i),
                    _contentCache.GetItem($"Type Parameter #{i+1}"));
            }

            EditorDrawHelper.DrawBorders(_windowSize.x, _windowSize.y, HeaderStyle.BorderColor);

            if ( ! GUILayout.Button("Create Asset"))
                return;

            if (_typeRefs.Any(typeRef => typeRef.Type == null))
            {
                Debug.LogWarning("Choose all the type parameters first!");
            }
            else
            {
                _onTypesSelected(_typeRefs.CastToType());
            }
        }

        private void DrawHeader()
        {
            Rect headerArea = GUILayoutUtility.GetRect(0f, HeaderStyle.OuterButtonSize, DrawHelper.ExpandWidth(true));
            EditorGUI.DrawRect(headerArea, HeaderStyle.BackgroundColor);
            (Rect _, Rect outerButtonRect) = headerArea.CutVertically(HeaderStyle.OuterButtonSize, true);

            var innerButtonRect = new Vector2(HeaderStyle.ButtonSize, HeaderStyle.ButtonSize).Center(outerButtonRect);

            if (HeaderStyle.CloseButton(innerButtonRect))
                Close();
        }

        private static float GetWindowHeight(int typeFieldsCount)
        {
            float oneTypeFieldHeight = EditorStyles.popup.CalcHeight(GUIContent.none, 0f);
            const float buttonHeight = 24f;
            const float spacing = 2f;
            float windowHeight = (oneTypeFieldHeight + spacing) * typeFieldsCount + buttonHeight + HeaderStyle.OuterButtonSize;
            return windowHeight;
        }
    }

    public static class HeaderStyle
    {
        public const float ButtonSize = 16f;
        public const float OuterButtonSize = 20f;

        public static readonly Color BorderColor = new Color(0.306f, 0.306f, 0.306f);


        private static readonly GUIContent CloseIcon = DarkSkin
            ? EditorGUIUtility.IconContent("d_winbtn_win_close")
            : EditorGUIUtility.IconContent("winbtn_win_close");

        private static readonly GUIStyle ButtonStyle = GetButtonStyle();

        private static readonly Color BackgroundColorDark = new Color(0.157f, 0.157f, 0.157f);
        private static readonly Color BackgroundColorLight = new Color(0.647f, 0.647f, 0.647f);

        public static Color BackgroundColor => DarkSkin ? BackgroundColorDark : BackgroundColorLight;

        private static bool DarkSkin => EditorGUIUtility.isProSkin;

        public static bool CloseButton(Rect buttonRect) => GUI.Button(buttonRect, CloseIcon, ButtonStyle);

        private static GUIStyle GetButtonStyle()
        {
            var backgroundTexture = new Texture2D(16, 16);
            backgroundTexture.SetPixels(Enumerable.Repeat(BackgroundColor, 16*16).ToArray());
            backgroundTexture.Apply();
            var buttonBackground = new EditorIcon(backgroundTexture);

            return new GUIStyle
            {
                normal = { background = buttonBackground.Default },
                hover = { background = buttonBackground.Highlighted },
                onHover = { background = buttonBackground.Highlighted },
            };
        }
    }
}