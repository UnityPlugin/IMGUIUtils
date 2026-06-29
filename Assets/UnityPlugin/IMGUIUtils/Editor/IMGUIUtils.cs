using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace UnityPlugin
{
    public class IMGUIUtils
    {
        static Dictionary<string, GUIContent> _guiContentCache = new();

        public static GUIContent GetGUIContent(string name)
        {
            if (!_guiContentCache.TryGetValue(name, out var label))
            {
                label = new GUIContent(name);
                _guiContentCache[name] = label;
            }
            return label;
        }

        public static T ObjectField<T>(string name, T obj) where T : Object
        {
            return EditorGUILayout.ObjectField(GetGUIContent(name), obj, typeof(T), false) as T;
        }

        #region Event

        static FieldInfo _lastControlIdField;

        public static int GetLastControlID()
        {
            if (_lastControlIdField == null)
            {
                var editorGuiUtilType = typeof(EditorGUIUtility);
                _lastControlIdField = editorGuiUtilType.GetField("s_LastControlID", BindingFlags.Static | BindingFlags.NonPublic);
            }

            return (int)_lastControlIdField.GetValue(null);
        }


        public static bool IsLastControlClick()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                var rect = GUILayoutUtility.GetLastRect();
                if (rect.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Scope

        #region Foldout

        static Dictionary<string, bool> _foldOutCache = new();

        public static FoldoutScope Foldout(string label, bool useHeaderGroup = false)
        {
            var scope = new FoldoutScope();
            scope.Begin(label, useHeaderGroup);
            return scope;
        }

        public struct FoldoutScope : IDisposable
        {
            public string key;
            public GUIContent name;
            public bool fold;
            public bool headerGroup;

            public void Begin(string label, bool useHeaderGroup = false)
            {
                key = label;
                headerGroup = useHeaderGroup;

                if (!_foldOutCache.TryGetValue(key, out fold))
                {
                    fold = true;
                }
                name = GetGUIContent(key);

                if (headerGroup)
                {
                    fold = EditorGUILayout.BeginFoldoutHeaderGroup(fold, name);
                }
                else
                {
                    fold = EditorGUILayout.Foldout(fold, name, true);
                }
                EditorGUI.indentLevel++;
            }

            public void Dispose()
            {
                if (headerGroup)
                {
                    EditorGUILayout.EndFadeGroup();
                }

                EditorGUI.indentLevel--;
                _foldOutCache[key] = fold;
            }
        }

        #endregion

        #region Editable

        public static EditableScope Editable(bool value, bool alphaColor = true)
        {
            var scope = new EditableScope();
            scope.Begin(value, alphaColor);
            return scope;
        }

        public struct EditableScope : IDisposable
        {
            public Color originContentColor;
            public Color originBackgroundColor;
            public bool enabled;

            public void Begin(bool value, bool alphaColor = true)
            {
                originContentColor = GUI.contentColor;
                originBackgroundColor = GUI.backgroundColor;

                enabled = GUI.enabled;
                GUI.enabled = value;

                if (!value && !alphaColor)
                {
                    var tmpContentColor = GUI.contentColor;
                    var tmpBackgroundColor = GUI.backgroundColor;
                    tmpContentColor.a *= 2;
                    tmpBackgroundColor.a *= 2;
                    GUI.contentColor = tmpContentColor;
                    GUI.backgroundColor = tmpBackgroundColor;
                }
            }

            public void Dispose()
            {
                GUI.contentColor = originContentColor;
                GUI.backgroundColor = originBackgroundColor;

                GUI.enabled = enabled;
            }
        }

        #endregion

        #region Indent

        public static IndentScope Indent(int value = 1)
        {
            var scope = new IndentScope();
            scope.Begin(value);
            return scope;
        }

        public struct IndentScope : IDisposable
        {
            public int indent;

            public void Begin(int value = 1)
            {
                indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel += value;
            }

            public void Dispose()
            {
                EditorGUI.indentLevel = indent;
            }
        }

        #endregion

        #region Horizontal

        public static HorizontalScope Horizontal(bool boxStyle = false, bool clearIndent = false, params GUILayoutOption[] options)
        {
            var scope = new HorizontalScope();
            scope.Begin(clearIndent, boxStyle, options);
            return scope;
        }

        public struct HorizontalScope : IDisposable
        {
            public int indent;

            public void Begin(bool boxStyle = false, bool clearIndent = false, params GUILayoutOption[] options)
            {
                if (boxStyle)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, options);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal(options);
                }

                indent = EditorGUI.indentLevel;
                if (clearIndent) EditorGUI.indentLevel = 0;
            }

            public void Dispose()
            {
                EditorGUI.indentLevel = indent;
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion

        #region PropertyHorizontal

        public static PropertyHorizontalScope PropertyHorizontal(string label, params GUILayoutOption[] options)
        {
            var scope = new PropertyHorizontalScope();
            scope.Begin(label, options);
            return scope;
        }

        public struct PropertyHorizontalScope : IDisposable
        {
            public string key;
            public GUIContent name;
            public int indent;

            public void Begin(string label, params GUILayoutOption[] options)
            {
                key = label;
                name = GetGUIContent(label);
                EditorGUILayout.BeginHorizontal(options);
                EditorGUILayout.PrefixLabel(name);

                indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                GUILayout.Space(2);
            }

            public void Dispose()
            {
                EditorGUI.indentLevel = indent;
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion

        #region Vertical

        public static VerticalScope Vertical(bool boxStyle = false, bool clearIndent = false, params GUILayoutOption[] options)
        {
            var scope = new VerticalScope();
            scope.Begin(boxStyle, clearIndent, options);
            return scope;
        }

        public struct VerticalScope : IDisposable
        {
            public int indent;

            public void Begin(bool boxStyle = false, bool clearIndent = false, params GUILayoutOption[] options)
            {
                if (boxStyle)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox, options);
                }
                else
                {
                    EditorGUILayout.BeginVertical(options);
                }

                indent = EditorGUI.indentLevel;
                if (clearIndent) EditorGUI.indentLevel = 0;
            }

            public void Dispose()
            {
                EditorGUI.indentLevel = indent;
                EditorGUILayout.EndVertical();
            }
        }

        #endregion

        #region Scroll

        static Dictionary<string, Vector2> _scrollPos = new();

        public static ScrollScope Scroll(string label, bool showLabel, params GUILayoutOption[] options)
        {
            var scope = new ScrollScope();
            scope.Begin(label, showLabel, options);
            return scope;
        }

        public static ScrollScope Scroll(string label, params GUILayoutOption[] options)
        {
            return Scroll(label, false, options);
        }

        public static ScrollScope Scroll(ref Vector2 pos, params GUILayoutOption[] options)
        {
            var scope = new ScrollScope();
            scope.Begin(ref pos, options);
            return scope;
        }

        public struct ScrollScope : IDisposable
        {
            public string key;
            public GUIContent name;
            public Vector2 scrollPosition;

            public void Begin(string label, bool showLabel = false, params GUILayoutOption[] options)
            {
                key = label;
                if (!string.IsNullOrEmpty(key))
                {

                    if (showLabel)
                    {
                        name = GetGUIContent(key);
                        EditorGUILayout.PrefixLabel(name);
                    }

                    _scrollPos.TryGetValue(key, out scrollPosition);
                }
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, options);
            }

            public void Begin(ref Vector2 pos, params GUILayoutOption[] options)
            {
                scrollPosition = pos;
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, options);
            }

            public void Dispose()
            {
                EditorGUILayout.EndScrollView();

                if (!string.IsNullOrEmpty(key))
                {
                    _scrollPos[key] = scrollPosition;
                }
            }
        }

        #endregion

        #region Color

        public static ColorScope Color(Color? color = null, Color? contentColor = null, Color? backgroundColor = null)
        {
            var scope = new ColorScope();
            scope.Begin(color, contentColor, backgroundColor);
            return scope;
        }

        public struct ColorScope : IDisposable
        {
            public Color originColor;
            public Color originContentColor;
            public Color originBackgroundColor;

            public void Begin(Color? color = null, Color? contentColor = null, Color? backgroundColor = null)
            {
                originColor = GUI.color;
                originContentColor = GUI.contentColor;
                originBackgroundColor = GUI.backgroundColor;

                GUI.color = color ?? GUI.color;
                GUI.contentColor = contentColor ?? GUI.contentColor;
                GUI.backgroundColor = backgroundColor ?? GUI.backgroundColor;
            }

            public void Dispose()
            {
                GUI.color = originColor;
                GUI.contentColor = originContentColor;
                GUI.backgroundColor = originBackgroundColor;
            }
        }

        #endregion

        #region Change

        public static bool IsChange { get; private set; }

        public static ChangeCheckScope ChangeCheck()
        {
            var scope = new ChangeCheckScope();
            scope.Begin();
            return scope;
        }

        public struct ChangeCheckScope : IDisposable
        {
            public void Begin()
            {
                EditorGUI.BeginChangeCheck();
            }

            public void Dispose()
            {
                IsChange = EditorGUI.EndChangeCheck();
            }
        }

        #endregion

        #endregion
    }
}