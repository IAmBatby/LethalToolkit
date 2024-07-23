using LethalToolkit.AssetBundleBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LethalToolkit
{
    public class EditorHelpers
    {
        public static LethalToolkitSettings toolkitSettings => LethalToolkitManager.Settings;


        private static Color _DefaultBackgroundColor;
        public static Color DefaultBackgroundColor
        {
            get
            {
                if (_DefaultBackgroundColor.a == 0)
                {
                    var method = typeof(EditorGUIUtility)
                        .GetMethod("GetDefaultBackgroundColor", BindingFlags.NonPublic | BindingFlags.Static);
                    _DefaultBackgroundColor = (Color)method.Invoke(null, null);
                }
                return _DefaultBackgroundColor;
            }
        }

        public static Color GetAlternatingColor(int arrayIndex)
        {
            if (arrayIndex % 2 == 0)
                return (LethalToolkitManager.Settings.firstColor);
            else
                return (LethalToolkitManager.Settings.secondColor);
        }

        public static GUIStyle GetNewStyle(bool enableRichText = true, int fontSize = -1)
        {
            GUIStyle newStyle = new GUIStyle();
            newStyle.richText = enableRichText;

            if (fontSize != -1)
                newStyle.fontSize = fontSize;

            return newStyle;
        }

        public static GUIStyle GetNewStyle(Color backgroundColor, bool enableRichText = true, int fontSize = -1)
        {
            GUIStyle newStyle = GetNewStyle(backgroundColor, enableRichText, fontSize);
            newStyle.Colorize(backgroundColor);

            return newStyle;
        }

        public static void InsertValueDataColumn<T>(string headerText, float columnWidth, List<T> dataList)
        {
            if (headerText == null || headerText == string.Empty || dataList == null) return;

            EditorGUILayout.BeginVertical(BackgroundStyle.Get(toolkitSettings.thirdColor), GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField(headerText.Colorize(Color.white), GetNewStyle(fontSize: toolkitSettings.headerFontSize));

            for (int i = 0; i < dataList.Count; i++)
                InsertDynamicValueLabel(dataList[i], GetNewStyle(fontSize: toolkitSettings.textFontSize), GetAlternatingColor(i), GUILayout.ExpandWidth(false));

            EditorGUILayout.EndVertical();
        }

        public static void InsertObjectDataColumn<T>(string headerText, float columnWidth, List<T> dataList) where T : UnityEngine.Object
        {
            if (headerText == null || headerText == string.Empty || dataList == null) return;

            EditorGUILayout.BeginVertical(BackgroundStyle.Get(toolkitSettings.thirdColor), GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField(headerText.Colorize(Color.white), GetNewStyle(fontSize: toolkitSettings.headerFontSize));

            for (int i = 0; i < dataList.Count; i++)
                InsertDynamicObjectLabel(dataList[i], GetNewStyle(fontSize: toolkitSettings.textFontSize), GetAlternatingColor(i), GUILayout.ExpandWidth(false));

            EditorGUILayout.EndVertical();
        }

        public static void InsertDynamicObjectLabel<T>(T type, GUIStyle style, Color color, params GUILayoutOption[] layoutOptions) where T : UnityEngine.Object
        {
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(color), GUILayout.ExpandWidth(false));
            EditorGUILayout.ObjectField(type, typeof(T), true, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
        }

        public static void InsertDynamicValueLabel<T>(T label, GUIStyle style, Color color, params GUILayoutOption[] layoutOptions)
        {
            GUIStyle guiStyle = BackgroundStyle.Get(color);
            guiStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.BeginHorizontal(guiStyle, GUILayout.ExpandWidth(false));

            if (label is string)
                EditorGUILayout.LabelField((label as string).ToBold().Colorize(Color.white), style, layoutOptions);
            else if (label is int)
                EditorGUILayout.IntField(Convert.ToInt32(label), style, layoutOptions);

            EditorGUILayout.EndHorizontal();
        }

        Color cachedColor;
        public void AddTextLine(string text, TextDirection textDirection = TextDirection.None, GUIStyle editorStyles = null, int width = 0, int height = 0)
        {
            if (cachedColor == null)
                cachedColor = DefaultBackgroundColor;
            if (cachedColor == null)
            {
                Debug.LogError("Cached Colour Null!");
                cachedColor = Color.white;
            }

            GUIStyle newStyle = new GUIStyle();
            if (editorStyles == null)
                editorStyles = newStyle;

            editorStyles.richText = true;
            text = text.Colorize(EditorStyles.boldLabel.normal.textColor);

            if (textDirection == TextDirection.BeginVertical)
            {
                EditorGUILayout.BeginVertical(BackgroundStyle.Get(cachedColor));
                EditorGUILayout.LabelField(text, editorStyles);
                EditorGUILayout.EndVertical();
            }
            else if (textDirection == TextDirection.BeginHorizontal)
            {
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(cachedColor));
                EditorGUILayout.LabelField(text, editorStyles);
                EditorGUILayout.EndHorizontal();
            }
            else if (textDirection == TextDirection.None)
                EditorGUILayout.LabelField(text, editorStyles, GUILayout.ExpandWidth(false));

            cachedColor = DefaultBackgroundColor;
        }

        public void AddTextLine(string text, Color color, TextDirection textDirection = TextDirection.BeginHorizontal, GUIStyle editorStyles = null)
        {
            cachedColor = color;
            AddTextLine(text, textDirection, editorStyles);
        }

        public static List<GameObject> GetPrefabsWithType(Type type)
        {
            List<GameObject> returnList = new List<GameObject>();
            IEnumerable<GameObject> allPrefabs;
            allPrefabs = UnityEditor.AssetDatabase.FindAssets("t:GameObject")
            .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
            .Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(x));
            foreach (GameObject prefab in allPrefabs)
            {
                if (prefab.GetComponent(type) != null)
                    returnList.Add(prefab);
                else if (prefab.GetComponentInChildren(type) != null)
                    returnList.Add(prefab);
            }
                return (returnList);
        }

        public static void SerializeDictionary<K, V>(ref Dictionary<K, V> dictionary, ref List<K> keys, ref List<V> values)
        {
            if (dictionary == null)
                dictionary = new Dictionary<K, V>();
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<K, V> kvp in dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public static void DeserializeDictionary<K, V>(ref Dictionary<K, V> dictionary, ref List<K> keys, ref List<V> values)
        {
            if (dictionary == null)
                dictionary = new Dictionary<K, V>();
            else
                dictionary.Clear();

            for (int i = 0; i != Math.Min(keys.Count, values.Count); i++)
                dictionary.Add(keys[i], values[i]);
        }
    }
}
