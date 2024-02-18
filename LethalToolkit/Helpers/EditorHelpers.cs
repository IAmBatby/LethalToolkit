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
        public static LethalToolkitSettings toolkitSettings => LethalToolkitManager.Instance.LethalToolkitSettings;

        public static Color GetAlternatingColor(int arrayIndex)
        {
            if (arrayIndex % 2 == 0)
                return (LethalToolkitManager.Instance.LethalToolkitSettings.firstColor);
            else
                return (LethalToolkitManager.Instance.LethalToolkitSettings.secondColor);
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
    }
}
