using DunGen;
using DunGen.Adapters;
using DunGen.Graph;
using LethalLevelLoader;
using LethalToolkit.AssetBundleBuilder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static Unity.Netcode.NetworkObject;
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
        public static ExtendedLevel currentlySelectedExtendedLevel;

        public bool unclosedVertical;
        public bool unclosedHorizontal;

        public Color backgroundColor;
        public Color defaultTextColor;

        private int defaultFontSize;

        public enum ReportType { None, All, EntranceTeleport, AudioReverbTrigger }
        public ReportType reportType;

        public DynamicTogglePopup dynamicPopup = new DynamicTogglePopup(new string[] { "None", "All", "EntranceTeleport", "AudioReverbTrigger", "SmokeTest" });


        public LethalToolkitSettings settings = LethalToolkitManager.Instance.LethalToolkitSettings;

        [MenuItem("LethalToolkit/Tools/ExtendedLevel Validator")]
        public static void OpenWindow()
        {
            if (window != null)
            {
                window.Close();
                window = null;
            }
            sceneHierarchy = null;
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
                currentlySelectedExtendedLevel = extendedLevel;
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
            {
                LethalToolkitManager.Instance.LethalToolkitSettings.lastSelectedExtendedLevel = null;
                currentlySelectedExtendedLevel = null;
            }

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

            dynamicPopup.Toggle(EditorGUILayout.Popup(dynamicPopup.CurrentSelection, dynamicPopup.CurrentSelectionIndex, dynamicPopup.ToggleOptions));

            if (dynamicPopup.CurrentSelection == "All")
                SceneAllReport(allObjects, scene);
            else if (dynamicPopup.CurrentSelection == "AudioReverbTrigger")
                SceneAudioReverbTriggerReport(allObjects);
            else if (dynamicPopup.CurrentSelection == "SmokeTest")
                SmokeTest();
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

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(700));
            GUILayout.BeginHorizontal();
            EditorHelpers.InsertValueDataColumn("Component Number", settings.assetPathWidth, audioReverbTriggers.Select(asset => "#" + audioReverbTriggers.IndexOf(asset).ToString()).ToList());
            EditorHelpers.InsertObjectDataColumn("Component Name", settings.assetNameWidth, audioReverbTriggers);
            EditorHelpers.InsertObjectDataColumn("NetworkObject", settings.assetNameWidth, audioReverbTriggers.Select(asset => asset.NetworkObject).ToList());
            EditorHelpers.InsertValueDataColumn("Use Preset", settings.assetNameWidth, audioReverbTriggers.Select(asset => asset.usePreset.ToString()).ToList());
            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        public static Dictionary<GameObject, bool> sceneHierarchy;
        public void SceneAllReport(List<GameObject> allObjects, Scene scene)
        {
            if (sceneHierarchy == null)
            {
                sceneHierarchy = new Dictionary<GameObject, bool>();
                foreach (GameObject obj in allObjects)
                    sceneHierarchy.Add(obj, false);
            }
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


            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(350));
            HierarchyFoldout(scene);

            List<GameObject> showInHierarchyList = new List<GameObject>(activeHierarchy.Concat(scene.GetRootGameObjects()));

            foreach (GameObject go in new List<GameObject>(showInHierarchyList))
                foreach (Transform child in  go.transform)
                {
                    if (!showInHierarchyList.Contains(child.gameObject))
                        showInHierarchyList.Add(child.gameObject);
                }

            int counter = 0;
            foreach (GameObject gameObject in allObjects)
            {
                if (showInHierarchyList.Contains(gameObject) && sceneHierarchy.Keys.ToList().Contains(gameObject))
                {
                    EditorGUILayout.BeginVertical(BackgroundStyle.Get(EditorHelpers.GetAlternatingColor(counter)));
                    if (gameObject.transform.childCount > 0)
                        sceneHierarchy[gameObject] = EditorGUILayout.Foldout(sceneHierarchy[gameObject], GetSpacedOffset(GetParentCount(gameObject) + settings.hierarchyOffset) + gameObject.name);
                    else
                        EditorGUILayout.LabelField(GetSpacedOffset(GetParentCount(gameObject)) + gameObject.name);
                    EditorGUILayout.EndVertical();
                    counter++;
                }
            }
            EditorGUILayout.EndScrollView();



            /*string newTextLine = string.Empty;
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

                    HierarchyFoldout(scene);
                    //AddTextLineAlternating(newTextLine.ToBold(), TextDirection.None);

                    List<Texture> textures = new List<Texture>();
                    foreach (var component in sceneObject.GetComponents(typeof(Component)))
                    {
                        if (component != null)
                        {
                            Texture texture = EditorGUIUtility.ObjectContent(null, component.GetType()).image;
                            if (texture != null)
                                textures.Add(texture);
                        }
                    }
                    AddTextures(textures);

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndScrollView();*/

        }
        public void SmokeTest()
        {
            EditorGUILayout.Space(15);
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(settings.thirdColor));
            AddTextLine("Dungeon Tests".ToBold(), Color.white, TextDirection.None);
            EditorGUILayout.EndHorizontal();

            GameObject dungeonGenerator = GameObject.FindGameObjectWithTag("DungeonGenerator");
            if (dungeonGenerator != null)
            {
                EditorGUILayout.BeginHorizontal();
                AddTextLine("Found DungeonGenerator: ".ToBold(), Color.white, TextDirection.None);
                EditorGUILayout.ObjectField(dungeonGenerator, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
                RuntimeDungeon runtimeDungeon = dungeonGenerator.GetComponent<RuntimeDungeon>();
                if (runtimeDungeon != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    AddTextLine("Found RuntimeDungeon: ".ToBold(), Color.white, TextDirection.None);
                    EditorGUILayout.ObjectField(runtimeDungeon, typeof(RuntimeDungeon), true);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    AddTextLine("Generate On Start".ToBold(), Color.white, TextDirection.None);
                    EditorGUILayout.Toggle(string.Empty, runtimeDungeon.GenerateOnStart);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    AddTextLine("DungeonFlow: ".ToBold(), Color.white, TextDirection.None);
                    EditorGUILayout.ObjectField(runtimeDungeon.Generator.DungeonFlow, typeof(DungeonFlow), true);
                    EditorGUILayout.EndHorizontal();
                    if (runtimeDungeon.Root != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        AddTextLine("Found Root: ".ToBold(), Color.white, TextDirection.None);
                        EditorGUILayout.ObjectField(runtimeDungeon.Root, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        AddTextLine("Root Position: ".ToBold(), Color.white, TextDirection.None);
                        EditorGUILayout.Vector3Field(string.Empty, runtimeDungeon.Root.transform.position);
                        EditorGUILayout.EndHorizontal();
                    }
                }
                UnityNavMeshAdapter navMeshAdapter = dungeonGenerator.GetComponent<UnityNavMeshAdapter>();
                if (navMeshAdapter != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    AddTextLine("Found Unity NavMeshAdapter: ".ToBold(), Color.white, TextDirection.None);
                    EditorGUILayout.ObjectField(runtimeDungeon, typeof(RuntimeDungeon), true);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.Space(15);
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(settings.thirdColor));
            AddTextLine("NavMeshSurface Tests".ToBold(), Color.white, TextDirection.None);
            EditorGUILayout.EndHorizontal();

            GameObject environment = GameObject.FindGameObjectWithTag("OutsideLevelNavMesh");
            if (environment != null)
            {
                EditorGUILayout.BeginHorizontal();
                AddTextLine("Found Environment: ".ToBold(), Color.white, TextDirection.None);
                EditorGUILayout.ObjectField(environment, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
                NavMeshSurface surface = environment.GetComponent<NavMeshSurface>();
                if (surface != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    AddTextLine("Found NavMeshSurface: ".ToBold(), Color.white, TextDirection.None);
                    EditorGUILayout.ObjectField(surface, typeof(NavMeshSurface), true);
                    EditorGUILayout.EndHorizontal();
                    if (surface.navMeshData != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        AddTextLine("Found NavMeshData: ".ToBold(), Color.white, TextDirection.None);
                        EditorGUILayout.ObjectField(surface.navMeshData, typeof(NavMeshData), true);
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }


            EditorGUILayout.Space(15);
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(settings.thirdColor));
            AddTextLine("AINode Tests".ToBold(), Color.white, TextDirection.None);
            EditorGUILayout.EndHorizontal();

            List<SpawnableOutsideObject> spawnableOutsideObjects = currentlySelectedExtendedLevel.selectableLevel.spawnableOutsideObjects.Select(s => s.spawnableObject).ToList();
            List<string> spawnableOutsideObjectTags = new List<string>();

            foreach (SpawnableOutsideObject spawnableOutsideObject in spawnableOutsideObjects)
                foreach (string tag in spawnableOutsideObject.spawnableFloorTags)
                    if (!spawnableOutsideObjectTags.Contains(tag))
                        spawnableOutsideObjectTags.Add(tag);

            List<GameObject> aiNodes = (from x in GameObject.FindGameObjectsWithTag("OutsideAINode") orderby Vector3.Distance(x.transform.position, Vector3.zero) select x).ToList();
            NavMeshHit navMeshHit = default(NavMeshHit);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(300));
            EditorGUILayout.BeginVertical();
            foreach (GameObject aiNode in aiNodes)
            {
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorHelpers.GetAlternatingColor(aiNodes.IndexOf(aiNode))));
                EditorGUILayout.ObjectField(aiNode, typeof(GameObject), true);
                NavMesh.SamplePosition(aiNode.transform.position, out NavMeshHit hit, 50f, -1);
                if (hit.hit == false || hit.distance > 10f)
                {
                    if (hit.distance == Mathf.Infinity)
                        AddTextLine("Distance: ".ToBold() + "Too Far".Colorize(Color.red), TextDirection.None);
                    else if (hit.distance < 0.1f)
                        AddTextLine("Distance: ".ToBold() + hit.distance.ToString("F2").Colorize(Color.yellow), TextDirection.None);
                    else
                        AddTextLine("Distance: ".ToBold() + hit.distance.ToString("F2").Colorize(Color.red), TextDirection.None);
                }
                else
                    AddTextLine("Distance: ".ToBold() + hit.distance.ToString("F2").Colorize(Color.green), TextDirection.None);
                if (Physics.Raycast(aiNode.transform.position, hit.position, out RaycastHit raycastHit, Mathf.Infinity))
                {
                    if (spawnableOutsideObjectTags.Contains(raycastHit.collider.gameObject.tag))
                        AddTextLine("Floor Tag: ".ToBold() + raycastHit.collider.gameObject.tag.Colorize(Color.green), TextDirection.None);
                    else
                        AddTextLine("Floor Tag: ".ToBold() + raycastHit.collider.gameObject.tag.Colorize(Color.red), TextDirection.None);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(15);
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(settings.thirdColor));
            AddTextLine("Entrance Teleport Tests".ToBold(), Color.white, TextDirection.None);
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(300));
            EditorGUILayout.BeginVertical();

            List<EntranceTeleport> entranceTeleports = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>().OrderBy(e => e.entranceId).ToList();

            foreach (EntranceTeleport entrance in  entranceTeleports)
            {
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(EditorHelpers.GetAlternatingColor(entranceTeleports.IndexOf(entrance))));

                EditorGUILayout.ObjectField(entrance, typeof(EntranceTeleport), true);
                if (entrance.isEntranceToBuilding == true)
                    AddTextLine("IsEntranceToBuilding: ".ToBold() + entrance.isEntranceToBuilding.ToString().Colorize(Color.green));
                else
                    AddTextLine("IsEntranceToBuilding: ".ToBold() + entrance.isEntranceToBuilding.ToString().Colorize(Color.red));
                AddTextLine("Entrance ID: ".ToBold() + entrance.entranceId);
                if (entrance.audioReverbPreset != -1)
                    AddTextLine("AudioReverbPreset: ".ToBold() + entrance.audioReverbPreset.ToString().Colorize(Color.green));
                else
                    AddTextLine("AudioReverbPreset: ".ToBold() + entrance.audioReverbPreset.ToString().Colorize(Color.red));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();


            EditorGUILayout.Space(15);
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(settings.thirdColor));
            AddTextLine("Misc. Tests".ToBold(), Color.white, TextDirection.None);
            EditorGUILayout.EndHorizontal();

            GameObject mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            if (mapPropsContainer != null)
            {
                EditorGUILayout.BeginHorizontal();
                AddTextLine("Found MapPropsContainer: ".ToBold(), Color.white, TextDirection.None);
                EditorGUILayout.ObjectField(mapPropsContainer, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }
            else
                AddTextLine("Could Not Find GameObject With MapPropsContainer Tag!".ToBold(), Color.red, TextDirection.None);

            GameObject itemShipLandingPosition = GameObject.FindGameObjectWithTag("ItemShipLandingNode");
            if (itemShipLandingPosition != null)
            {
                EditorGUILayout.BeginHorizontal();
                AddTextLine("Found ItemShipLandingPosition: ".ToBold(), Color.white, TextDirection.None);
                EditorGUILayout.ObjectField(itemShipLandingPosition, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }
        }

        public static List<GameObject> activeHierarchy
        {
            get
            {
                List<GameObject> list = new List<GameObject>();
                foreach (KeyValuePair<GameObject, bool> pair in sceneHierarchy)
                    if (pair.Value == true)
                        list.Add(pair.Key);
                return (list);
            }
        }

        public Color GetHierarchyColour(GameObject gameObject)
        {
            if (SceneManager.GetActiveScene().GetRootGameObjects().Contains(gameObject))
                return (EditorHelpers.GetAlternatingColor(sceneHierarchy.Keys.ToList().IndexOf(gameObject)));
            else
                if (activeHierarchy.Contains(gameObject))
                return (EditorHelpers.GetAlternatingColor(sceneHierarchy.Keys.ToList().IndexOf(gameObject)));
            else
                return (Color.black);
        }

        public void HierarchyFoldout(Scene scene)
        {
            foreach (GameObject gameObject in scene.GetRootGameObjects())
            {
                if (sceneHierarchy[gameObject] == true)
                    foreach (Transform child in gameObject.transform)
                        if (sceneHierarchy.ContainsKey(child.gameObject))
                        {
                            if (child.childCount != 0)
                                ParentFold(child.gameObject);
                        }
            }
        }
        
        public void ParentFold(GameObject gameObject)
        {
            if (sceneHierarchy[gameObject] == true)
                foreach (Transform child in gameObject.transform)
                    if (sceneHierarchy.ContainsKey(child.gameObject))
                    {
                        if (child.childCount != 0 && sceneHierarchy[child.gameObject] == true)
                            ParentFold(child.gameObject);
                    }
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
        public void AddTextLine(string text, TextDirection textDirection = TextDirection.None, GUIStyle editorStyles = null, int width = 0, int height = 0)
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
