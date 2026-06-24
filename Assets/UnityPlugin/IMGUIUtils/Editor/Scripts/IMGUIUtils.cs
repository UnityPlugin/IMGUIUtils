using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IMGUI
{
    public class Utils
    {
        static Dictionary<string, GUIContent> _guiContentCache = new();
        static Dictionary<string, bool> _foldOutCache = new();

        public static GUIContent GetGUIContent(string name)
        {
            if (!_guiContentCache.TryGetValue(name, out var label))
            {
                label = new GUIContent(name);
                _guiContentCache[name] = label;
            }
            return label;
        }

        public static T ObjectField<T>(string name, Object obj) where T : Object
        {
            return EditorGUILayout.ObjectField(GetGUIContent(name), obj, typeof(T), false) as T;
        }

        public static void Foldout(string name, Action action)
        {
            if (!_foldOutCache.TryGetValue(name, out var v))
            {
                v = true;
            }
            v = EditorGUILayout.Foldout(v, GetGUIContent(name), true);

            if (v)
            {
                Indent(action);
            }

            _foldOutCache[name] = v;
        }

        public static void Editable(Action action, bool value, bool alphaColor = true)
        {
            var tmp = GUI.enabled;
            GUI.enabled = value;

            var originContentColor = GUI.contentColor;
            var originBackgroundColor = GUI.backgroundColor;

            if (!value && !alphaColor)
            {
                var tmpContentColor = GUI.contentColor;
                var tmpBackgroundColor = GUI.backgroundColor;
                tmpContentColor.a *= 2;
                tmpBackgroundColor.a *= 2;
                GUI.contentColor = tmpContentColor;
                GUI.backgroundColor = tmpBackgroundColor;
            }

            DoAction(action);

            GUI.contentColor = originContentColor;
            GUI.backgroundColor = originBackgroundColor;

            GUI.enabled = tmp;
        }

        public static void Indent(Action action)
        {
            EditorGUI.indentLevel++;
            DoAction(action);
            EditorGUI.indentLevel--;
        }

        public static void PropertyHorizontal(string label, Action action, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            EditorGUILayout.PrefixLabel(GetGUIContent(label));

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            GUILayout.Space(2);
            DoAction(action);

            EditorGUI.indentLevel = indent;
            EditorGUILayout.EndHorizontal();
        }

        public static void Horizontal(Action action, bool clearIndent = false, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);

            var indent = EditorGUI.indentLevel;
            if (clearIndent) EditorGUI.indentLevel = 0;

            DoAction(action);

            if (clearIndent) EditorGUI.indentLevel = indent;

            EditorGUILayout.EndHorizontal();
        }

        public static void Vertical(Action action, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);

            DoAction(action);

            EditorGUILayout.EndVertical();
        }

        public static void Color(Action action, Color? color = null, Color? contentColor = null, Color? backgroundColor = null)
        {
            var originColor = GUI.color;
            var originContentColor = GUI.contentColor;
            var originBackgroundColor = GUI.backgroundColor;

            GUI.color = color ?? GUI.color;
            GUI.contentColor = contentColor ?? GUI.contentColor;
            GUI.backgroundColor = backgroundColor ?? GUI.backgroundColor;

            DoAction(action);

            GUI.color = originColor;
            GUI.contentColor = originContentColor;
            GUI.backgroundColor = originBackgroundColor;
        }

        public static void Change(Action action, Action onChange)
        {
            EditorGUI.BeginChangeCheck();

            DoAction(action);

            var result = EditorGUI.EndChangeCheck();

            if (result)
            {
                DoAction(onChange);
            }
        }

        static void DoAction(Action action)
        {
            try
            {
                if (action != null) action();
            }
            catch (ExitGUIException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}