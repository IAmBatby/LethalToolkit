using LethalToolkit.AssetBundleBuilder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;
using Color = UnityEngine.Color;

namespace LethalToolkit
{
    public static class Extensions
    {
        public static LethalToolkitSettings toolkitSettings => LethalToolkitManager.Settings;

        public static string ToBold(this string input)
        {
            return new string("<b>" + input + "</b>");
        }

        public static string Colorize(this string input)
        {
            string hexColor = "#" + "FFFFFF";
            return new string("<color=" + hexColor + ">" + input + "</color>");
        }

        public static string Colorize(this string input, Color color)
        {
            string hexColor = "#" + ColorUtility.ToHtmlStringRGB(color);
            return new string("<color=" + hexColor + ">" + input + "</color>");
        }

        public static GUIStyle Colorize(this GUIStyle input, Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            input.normal.background = texture;
            return input;
        }

        public static Type GetListType<T>(this List<T> list)
        {
            return (typeof(T));
        }
    }
}
