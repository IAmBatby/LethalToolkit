using LethalLevelLoader;
using LethalToolkit.AssetBundleBuilder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
using Scene = UnityEngine.SceneManagement.Scene;

namespace LethalToolkit
{
    public class ExtendedLevelValidatorWindow : EditorWindow
    {
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

        public static ExtendedLevelValidatorWindow window;

        public UnityEngine.Object extendedLevelObject;

        public bool unclosedVertical;
        public bool unclosedHorizontal;

        public Color backgroundColor;
        public Color defaultTextColor;

        private int defaultFontSize;

        public enum ReportType { None, All, EntranceTeleport, AudioReverbTrigger }
        public ReportType reportType;


        public LethalToolkitSettings settings = LethalToolkitManager.Instance.LethalToolkitSettings;

        [MenuItem("LethalToolkit/Tools/ExtendedLevel Validator")]
        public static void OpenWindow()
        {

            window = GetWindow<ExtendedLevelValidatorWindow>("LethalToolkit: ExtendedLevel Validator");

        }

        public void OnGUI()
        {
            GUILayout.ExpandWidth(true);
            GUILayout.ExpandHeight(true);
            backgroundColor = DefaultBackgroundColor;

            defaultFontSize = GUI.skin.font.fontSize;

            GUI.skin.label.richText = true;
            GUI.skin.textField.richText = true;

            extendedLevelObject = LethalToolkitManager.Instance.LethalToolkitSettings.lastSelectedExtendedLevel;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ExtendedLevel", EditorStyles.boldLabel);
            extendedLevelObject = EditorGUILayout.ObjectField(extendedLevelObject, typeof(ExtendedLevel), true);
            EditorGUILayout.EndHorizontal();

            if (extendedLevelObject != null && extendedLevelObject is ExtendedLevel)
            {
                ExtendedLevel extendedLevel = (ExtendedLevel)extendedLevelObject;
                LethalToolkitManager.Instance.LethalToolkitSettings.lastSelectedExtendedLevel = extendedLevel;
                if (extendedLevel.selectableLevel == null)
                    AddTextLine("SelectableLevel: Null", TextDirection.Right);
                else
                {
                    AddTextLine("SelectableLevel: ".ToBold() + extendedLevel.selectableLevel.name, TextDirection.Right);
                    AddTextLine("Planet Name: ".ToBold() + extendedLevel.selectableLevel.PlanetName, TextDirection.Right);
                    Scene scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByName(extendedLevel.selectableLevel.sceneName);
                    if (scene != null)
                    {
                        AddTextLine("Scene Name: " + extendedLevel.selectableLevel.sceneName + "(Valid Scene)", TextDirection.Right);
                        if (scene.isLoaded == false)
                            AddTextLine("Please open scene to view full scene report.", TextDirection.Right);
                        else
                            SceneReport(scene);
                    }
                    else
                        AddTextLine("Scene Name: " + extendedLevel.selectableLevel.sceneName + "(Invalid Scene)", TextDirection.Right);
                }
            }
            else
                LethalToolkitManager.Instance.LethalToolkitSettings.lastSelectedExtendedLevel = null;

        }

        public string previousTypeFilter = string.Empty;
        public Vector2 scrollPos;

        public void SceneReport(Scene scene)
        {
            List<GameObject> rootObjects = scene.GetRootGameObjects().ToList();
            List<GameObject> allObjects = new List<GameObject>();

            foreach (GameObject rootObject in rootObjects)
            {
                allObjects.Add(rootObject);
                foreach (Transform transformChild in rootObject.GetComponentsInChildren<Transform>())
                    if (!allObjects.Contains(transformChild.gameObject))
                        allObjects.Add(transformChild.gameObject);
            }

            EditorGUILayout.Space(10);

            AddTextLine("Scene Root Objects", textDirection: TextDirection.Right, editorStyles: EditorStyles.boldLabel, color: settings.thirdColor);

            foreach (GameObject rootObject in rootObjects)
                AddTextLineAlternating("    " + rootObject.name, TextDirection.Right);

            EditorGUILayout.Space(5);

            reportType = (ReportType)EditorGUILayout.EnumPopup("Report Type", reportType);
            if (reportType == ReportType.All)
                SceneAllReport(allObjects);
            else if (reportType == ReportType.AudioReverbTrigger)
                SceneAudioReverbTriggerReport(allObjects);
        }

        public void SceneAudioReverbTriggerReport(List<GameObject> allObjects)
        {
            List<AudioReverbTrigger> audioReverbTriggers = new List<AudioReverbTrigger>();
            foreach (GameObject gameObject in allObjects)
                foreach (AudioReverbTrigger audioReverbTrigger in gameObject.GetComponentsInChildren<AudioReverbTrigger>())
                    audioReverbTriggers.Add(audioReverbTrigger);

            EditorGUILayout.Space(5f);
            AddTextLine("Audio Reverb Triggers", textDirection: TextDirection.Right, editorStyles: EditorStyles.boldLabel, color: settings.thirdColor);
            EditorGUILayout.Space(2.5f);

            string newTextLine = string.Empty;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(700));
            foreach (AudioReverbTrigger audioReverbTrigger in audioReverbTriggers)
            {
                newTextLine = audioReverbTrigger.gameObject.name;
                if (newTextLine != null && newTextLine != string.Empty && (previousTypeFilter == string.Empty || newTextLine.Contains(previousTypeFilter)))
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(settings.gameObjectNameWidth));
                    Color backgroundColor;
                    if (audioReverbTriggers.IndexOf(audioReverbTrigger) % 2 == 0) { backgroundColor = settings.firstColor; } else { backgroundColor = settings.secondColor; }
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(backgroundColor), GUILayout.Width(settings.gameObjectNameWidth));

                    AddTextLine("#" + audioReverbTriggers.IndexOf(audioReverbTrigger), TextDirection.None);

                    //AddTextLine(newTextLine.ToBold(), TextDirection.None);
                    EditorGUILayout.ObjectField(audioReverbTrigger, typeof(AudioReverbTrigger), true, GUILayout.ExpandWidth(false));

                    if (audioReverbTrigger.NetworkObject == null)
                        AddTextLine("NetworkObject Null!".Colorize(settings.lightRedColor).ToBold(), TextDirection.None);
                    else
                        AddTextLine(audioReverbTrigger.NetworkObject.gameObject.name, TextDirection.None);

                    if (audioReverbTrigger.reverbPreset == null)
                        AddTextLine("AudioReverbPreset Null!".Colorize(settings.lightRedColor).ToBold(), TextDirection.None);
                    else
                        EditorGUILayout.ObjectField(audioReverbTrigger.reverbPreset, typeof(ReverbPreset), true, GUILayout.ExpandWidth(false));

                    AddTextLine("UsePreset: ".ToBold() + audioReverbTrigger.usePreset, TextDirection.None);

                    Collider collider = audioReverbTrigger.GetComponent<Collider>();

                    if (collider == null)
                        AddTextLine("No Collider".ToBold(), TextDirection.None);
                    else
                        AddTextLine("Has Collider: ".ToBold() + collider.gameObject.name, TextDirection.None);

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        public void SceneAllReport(List<GameObject> allObjects)
        {

            EditorGUILayout.BeginVertical();
            AddTextLine("All Game Objects", textDirection: TextDirection.Right, editorStyles: EditorStyles.boldLabel, color: settings.thirdColor);
            EditorGUILayout.Space(2.5f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.ObjectContent(null, typeof(Transform)).image, GetComponentIconStyle());
            previousTypeFilter = EditorGUILayout.TextField(previousTypeFilter, GUI.skin.GetStyle("SearchTextField"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2.5f);
            EditorGUILayout.EndVertical();

            //EditorGUILayout.Space(5);


            GUIStyle headerStyle = new GUIStyle();
            headerStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.BeginHorizontal(GUILayout.Width(settings.gameObjectNameWidth));
            AddTextLine("Hierarchy".ToBold(), editorStyles: headerStyle, color: settings.thirdColor);
            AddTextLine("Components".ToBold(), editorStyles: headerStyle, color: settings.thirdColor);
            EditorGUILayout.EndHorizontal();



            string newTextLine = string.Empty;
            //GUIStyle newStyle = GUI.skin.GetStyle("ProfilerPaneSubLabel");
            GUIStyle newStyle = new GUIStyle(EditorStyles.label);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(350));
            foreach (GameObject sceneObject in allObjects)
            {
                newTextLine = GetGameObjectListing(sceneObject, false);
                if (newTextLine != null && newTextLine != string.Empty && (previousTypeFilter == string.Empty || newTextLine.Contains(previousTypeFilter)))
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(settings.gameObjectNameWidth));
                    Color backgroundColor;
                    if (allObjects.IndexOf(sceneObject) % 2 == 0) { backgroundColor = settings.firstColor; } else { backgroundColor = settings.secondColor; }
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(backgroundColor), GUILayout.Width(settings.gameObjectNameWidth));

                    AddTextLineAlternating(newTextLine.ToBold(), TextDirection.None);

                    List<Texture> textures = new List<Texture>();
                    foreach (var component in sceneObject.GetComponents(typeof(Component)))
                    {
                        Texture texture = EditorGUIUtility.ObjectContent(null, component.GetType()).image;
                        if (texture != null)
                            textures.Add(texture);
                    }
                    AddTextures(textures);

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndScrollView();

        }

        public string GetGameObjectListing(GameObject gameObject, bool withComponents = true)
        {
            string returnString = string.Empty;

            returnString += "    " + GetSpacedOffset(Mathf.RoundToInt(GetParentCount(gameObject) * LethalToolkitManager.Instance.LethalToolkitSettings.spaceMultiplier));
            returnString += gameObject.name;

            if (withComponents == true)
            {
                foreach (var component in gameObject.GetComponents(typeof(Component)))
                {
                    string componentType = component.GetType().ToString();
                    if (componentType.Contains("."))
                        componentType = componentType.Substring(componentType.LastIndexOf(".") + 1);
                    returnString += " (" + componentType + ")";
                }
            }

            return (returnString);
        }

        public int GetParentCount(GameObject gameObject)
        {
            Transform parent = gameObject.transform.parent;
            int returnCount = 0;

            while (parent != null)
            {
                returnCount++;
                parent = parent.parent;
            }

            return (returnCount);
        }

        public string GetSpacedOffset(int spaceAmount)
        {
            string returnString = string.Empty;

            for (int i = 0; i < spaceAmount; i++)
                returnString += "    ";

            return (returnString);
        }

        public void AddTextures(List<Texture> textures)
        {
            foreach (Texture image in textures)
            {
                //EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                //EditorGUILayout.Space(5, false);
                GUILayout.Label(image, GetComponentIconStyle(), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                //EditorGUILayout.EndVertical();
            }
        }

        public enum TextDirection { Down, Right, None }
        Color cachedColor;
        public void AddTextLine(string text, TextDirection textDirection = TextDirection.Right, GUIStyle editorStyles = null, int width = 0, int height = 0)
        {
            if (cachedColor == null)
                cachedColor = backgroundColor;

            GUIStyle newStyle = new GUIStyle();
            if (editorStyles == null)
                editorStyles = newStyle;

            editorStyles.richText = true;
            text = text.Colorize(EditorStyles.boldLabel.normal.textColor);

            if (textDirection == TextDirection.Down)
            {
                EditorGUILayout.BeginVertical(BackgroundStyle.Get(cachedColor));
                EditorGUILayout.LabelField(text, editorStyles);
                EditorGUILayout.EndVertical();
            }
            else if (textDirection == TextDirection.Right)
            {
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(cachedColor));
                EditorGUILayout.LabelField(text, editorStyles);
                EditorGUILayout.EndHorizontal();
            }
            else if (textDirection == TextDirection.None)
                    EditorGUILayout.LabelField(text, editorStyles, GUILayout.ExpandWidth(false));

            cachedColor = backgroundColor;
        }

        public void AddTextLine(string text, Color color, TextDirection textDirection = TextDirection.Right, GUIStyle editorStyles = null)
        {
            cachedColor = color;
            AddTextLine(text, textDirection, editorStyles);
        }

        public void AddTextLineAlternating(string text, TextDirection textDirection = TextDirection.Right, GUIStyle editorStyles = null)
        {
            if (textDirection != TextDirection.None)
                AddTextLine(text, GetFlipColor(), textDirection, editorStyles);
            else
                AddTextLine(text, textDirection, editorStyles);
        }

        public bool flip;
        public Color GetFlipColor()
        {
            Color firstColor = LethalToolkitManager.Instance.LethalToolkitSettings.firstColor;
            Color secondColor = LethalToolkitManager.Instance.LethalToolkitSettings.secondColor;

            if (flip == true)
            {
                flip = false;
                return (firstColor);
            }
            else
            {
                flip = true;
                return (secondColor);
            }
        }

        public void SetNextLineColour(Color color)
        {
            
        }

        public GUIStyle GetComponentIconStyle()
        {
            float componentTextureSize = defaultFontSize + settings.overrideComponentSize;
            Vector2 componentOffsetSize = new Vector2((defaultFontSize / 2), defaultFontSize / 2) + settings.overrideComponentOffset;
            GUIStyle newStyle = new GUIStyle();
            newStyle.fixedHeight = componentTextureSize;
            newStyle.fixedWidth = componentTextureSize;
            newStyle.contentOffset = componentOffsetSize;
            return (newStyle);
        }
    }
}
