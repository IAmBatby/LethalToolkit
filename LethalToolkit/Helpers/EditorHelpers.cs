using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LethalToolkit
{
    public static class EditorHelpers
    {
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

            if (fontSize != 1)
                newStyle.fontSize = fontSize;

            return newStyle;
        }


        public static GUIStyle GetNewStyle(Color backgroundColor, bool enableRichText = true, int fontSize = -1)
        {
            GUIStyle newStyle = GetNewStyle(backgroundColor, enableRichText, fontSize);
            newStyle.Colorize(backgroundColor);

            return newStyle;
        }
    }
}
