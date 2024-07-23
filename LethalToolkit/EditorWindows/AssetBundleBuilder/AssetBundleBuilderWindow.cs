using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine.Events;
using HarmonyLib;
using UnityEditor.Build.Content;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

namespace LethalToolkit
{
    public class AssetBundleBuilderWindow : EditorWindow
    {
        public static AssetBundleBuilderSettings WindowData => AssetBundleBuilderSettings.Instance;

        private static bool buildAllBundles = true;
        private static Dictionary<string, bool> assetBundleDict { get { return (WindowData.assetBundlesDict); } set { WindowData.assetBundlesDict = value; } }
        private static Dictionary<string, bool> buildOptionsDict { get { return (WindowData.buildOptionsDict); } set { WindowData.buildOptionsDict = value; } }

        public static List<AssetBundleInfo> assetBundleInfos = new List<AssetBundleInfo>();

        public UnityEvent onBeforeBuildEvents;
        public UnityEvent onAfterBuildEvents;

        public static string SelectedVariantName
        {
            get
            {
                string returnString = string.Empty;
                if (WindowData.variantType == VariantType.lem)
                    returnString = ".lem";
                if (WindowData.variantType == VariantType.lethalbundle)
                    returnString = ".lethalbundle";
                return (returnString);
            }
        }

        enum compressionOption { NormalCompression = 0, FastCompression = 1, Uncompressed = 2 }
        compressionOption compressionMode = compressionOption.NormalCompression;
        bool _64BitsMode;
        bool debugBuiltAssetBundles;

        BuildAssetBundleOptions buildAssetBundleOptions;

        public Vector2 scrollPos;

        static LethalToolkitSettings settings => LethalToolkitManager.Settings;

        public static Dictionary<string, AssetBundleInfo> lastBuiltAssetBundleInfos;

        [SerializeField]
        public int value0;


        public static AssetBundleBuilderWindow window;

        [MenuItem("LethalToolkit/Tools/AssetBundle Builder")]
        public static void OpenWindow()
        {
            Debug.Log("Opening Windows");
            //
            WindowData.gameDirectory = LethalToolkitManager.Settings.lethalCompanyAssetBundleDirectory;
            WindowData.projectDirectory = LethalToolkitManager.Settings.unityAssetBundleDirectory;

            lastBuiltAssetBundleInfos = new Dictionary<string, AssetBundleInfo>();

            window = GetWindow<AssetBundleBuilderWindow>("LethalToolkit: AssetBundle Builder");
            ///
            ///
            //
            //
            //
            //
            //
            //
            //
            //
            //

            RefreshDictionaries();

            AssetBundleBuilderSettings.Instance.openedAmountCounter++;
        }

        public static void RefreshDictionaries()
        {
            if (assetBundleDict == null || (assetBundleDict != null && assetBundleDict.Count == 0))
            {
                Dictionary<string, bool> newDict = new Dictionary<string, bool>();
                foreach (string assetBundleName in AssetDatabase.GetAllAssetBundleNames())
                    newDict.Add(assetBundleName, false);
                assetBundleDict = new Dictionary<string, bool>(newDict);
            }
            /*if (buildOptionsDict == null || (buildOptionsDict != null && buildOptionsDict.Count == 0))
            {
                buildOptionsDict = new Dictionary<string, bool>();
                foreach (Enum buildEnum in Enum.GetValues(typeof(BuildAssetBundleOptions)))
                {
                    string decoratedString = buildEnum.ToString();
                    int counter = 0;
                    foreach (char c in buildEnum.ToString())
                    {
                        if (char.IsUpper(c) == true && counter != 0 && counter != buildEnum.ToString().Length - 1)
                            decoratedString = decoratedString.Insert(counter - 1, " ");
                    }
                    buildOptionsDict.Add(decoratedString.ToString(), false);
                }
            }*/
            buildOptionsDict = new Dictionary<string, bool>();
            foreach (Enum buildEnum in Enum.GetValues(typeof(BuildAssetBundleOptions)))
            {
                string decoratedString = buildEnum.ToString();

                List<char> referenceArray = decoratedString.ToList();
                if (referenceArray.Count != 0)
                {
                    List<int> insetIndexes = new List<int>();
                    for (int i = 0; i < referenceArray.Count - 1; i++)
                        if (i != 0 && char.IsUpper(referenceArray[i]))
                            insetIndexes.Add(i);

                    foreach (int i in insetIndexes)
                        decoratedString = decoratedString.Insert(i + insetIndexes.IndexOf(i), " ");

                    buildOptionsDict.Add(decoratedString, false);
                }
            }
        }

        void OnEnable()
        {
            //
            //
            //
            //
            //
            //
            //
            //
            //
            //
            //
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Opened (" + AssetBundleBuilderSettings.Instance.openedAmountCounter + ") Times.");

            if (assetBundleDict == null)
                Debug.LogError("Asset Bundle Dict Null!");

            GUILayout.ExpandWidth(true);
            GUILayout.ExpandHeight(true);

            GUILayout.Label("General Settings", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Project Output Directory", "The directory where the asset bundles will be saved."), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            WindowData.projectDirectory = EditorGUILayout.TextField(WindowData.projectDirectory, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Game Output Directory", "The directory where the asset bundles will be saved."), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            WindowData.gameDirectory = EditorGUILayout.TextField(WindowData.gameDirectory, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Variant Type", "Select the used AssetBundle variant type."), GUILayout.Width(145));
            WindowData.variantType = (VariantType)EditorGUILayout.EnumPopup(WindowData.variantType, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(25);

            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(settings.thirdColor));
            GUILayout.Label("Build Options", EditorStyles.boldLabel, GUILayout.ExpandHeight(false), GUILayout.Width(270));
            buildOptionsDict[BuildAssetBundleOptions.None.ToString()] = EditorGUILayout.Toggle(buildOptionsDict[BuildAssetBundleOptions.None.ToString()]);
            EditorGUILayout.EndHorizontal();

            int counter1 = 0;
            foreach (KeyValuePair<string, bool> keyValuePair in new Dictionary<string, bool>(buildOptionsDict))
            {
                Color color;
                if (counter1 % 2 == 0) { color = settings.firstColor; } else { color = settings.secondColor; }

                if (counter1 != 0 && buildOptionsDict[BuildAssetBundleOptions.None.ToString()] == true)
                {
                    EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(color));
                    if (buildOptionsDict[keyValuePair.Key] == true)
                    {
                        EditorGUILayout.LabelField(keyValuePair.Key, EditorStyles.boldLabel, GUILayout.ExpandHeight(false), GUILayout.Width(270));
                        buildOptionsDict[keyValuePair.Key] = EditorGUILayout.Toggle(keyValuePair.Value, GUILayout.ExpandHeight(false), GUILayout.Width(20));
                        EditorGUILayout.LabelField(AssetBundleBuilderSettings.buildOptionsDescriptions[counter1], EditorStyles.boldLabel);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(keyValuePair.Key, GUILayout.ExpandHeight(false), GUILayout.Width(270));
                        buildOptionsDict[keyValuePair.Key] = EditorGUILayout.Toggle(keyValuePair.Value, GUILayout.ExpandHeight(false), GUILayout.Width(20));
                        EditorGUILayout.LabelField(AssetBundleBuilderSettings.buildOptionsDescriptions[counter1]);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                counter1++;
            }

            buildOptionsDict[BuildAssetBundleOptions.None.ToString()] = !buildOptionsDict[BuildAssetBundleOptions.None.ToString()];

            EditorGUILayout.Space(25);

            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(settings.thirdColor));
            GUILayout.Label("Asset Bundle Selection", EditorStyles.boldLabel, GUILayout.ExpandHeight(false), GUILayout.Width(270));
            WindowData.buildAllBundles = EditorGUILayout.Toggle(WindowData.buildAllBundles);
            EditorGUILayout.EndHorizontal();

            int counter = 0;
            if (WindowData.buildAllBundles == true && assetBundleDict != null)
                foreach (KeyValuePair<string,bool> keyValuePair in new Dictionary<string, bool>(assetBundleDict))
                    if (keyValuePair.Key != null)
                    {
                        Color color;
                        if (counter % 2 == 0) { color = settings.firstColor; } else { color = settings.secondColor; }
                        EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(color));
                        if (assetBundleDict[keyValuePair.Key] == true)
                            EditorGUILayout.LabelField(keyValuePair.Key, EditorStyles.boldLabel, GUILayout.ExpandHeight(false), GUILayout.Width(270));
                        else
                            EditorGUILayout.LabelField(keyValuePair.Key, GUILayout.ExpandHeight(false), GUILayout.Width(270));
                        assetBundleDict[keyValuePair.Key] = EditorGUILayout.Toggle(keyValuePair.Value, GUILayout.ExpandHeight(false), GUILayout.Width(270));
                        EditorGUILayout.EndHorizontal();
                        counter++;
                    }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Build AssetBundles", GUILayout.ExpandHeight(false), GUILayout.Width(240)))
                BuildAssetBundles();
            if (string.IsNullOrEmpty(WindowData.previousDateTime) == false)
            {
                DateTime.TryParse(WindowData.previousDateTime, out DateTime currentDateTime);
                EditorGUILayout.LabelField("Last Built: " + currentDateTime.ToLongTimeString() + " (" + currentDateTime.ToShortDateString() + ")");
                //EditorGUILayout.LabelField("Last Built: " + currentDateTime.Hour + "." + currentDateTime.Minute + "." + currentDateTime.Second + " (" + currentDateTime.Date + ")");
            }

            if (lastBuiltAssetBundleInfos != null)
                foreach (KeyValuePair<string, AssetBundleInfo> lastBuiltBundles in lastBuiltAssetBundleInfos)
                {
                    AssetBundleInfo lastAssetBundleInfo = lastBuiltBundles.Value;
                    List<BundledAssetInfo> allAssets = new List<BundledAssetInfo>(lastAssetBundleInfo.directBundledAssetInfos.Concat(lastAssetBundleInfo.indirectBundledAssetInfos));
                    LethalToolkitSettings settings = LethalToolkitManager.Settings;

                    EditorGUILayout.Space(5);


                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Asset Bundle Info: " + lastAssetBundleInfo.assetBundleBuild.assetBundleName + lastAssetBundleInfo.assetBundleBuild.assetBundleVariant, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(600));

                    EditorGUILayout.Space(5);
                    GUILayout.BeginHorizontal();

                    Debug.Log("Listing Columns");

                    EditorHelpers.InsertValueDataColumn("Asset Number", settings.assetPathWidth, allAssets.Select(asset => "#" + allAssets.IndexOf(asset).ToString()).ToList());
                    EditorHelpers.InsertValueDataColumn("Asset Name", settings.assetNameWidth, allAssets.Select(asset => asset.assetName).ToList());
                    EditorHelpers.InsertValueDataColumn("Asset Type", settings.assetTypeWidth, allAssets.Select(asset => asset.assetType.ToString()).ToList());
                    EditorHelpers.InsertValueDataColumn("Asset Source", settings.assetSourceWidth, allAssets.Select(asset => asset.assetSource).ToList());
                    EditorHelpers.InsertValueDataColumn("Assembly", settings.assetSourceWidth, allAssets.Select(asset => asset.contentSourceType.ToString()).ToList());
                    EditorHelpers.InsertValueDataColumn("AssetBundle Collection Type", settings.assetReferenceWidth, allAssets.Select(asset => asset.contentReferenceType.ToString()).ToList());
                    EditorHelpers.InsertValueDataColumn("Asset Path", settings.assetPathWidth, allAssets.Select(asset => asset.assetPath).ToList());

                    GUILayout.EndHorizontal();

                    GUILayout.EndScrollView();
                    EditorGUILayout.Space(10);
                }
            //
        }

        void BuildAssetBundles()
        {
            List<string> enabledAssetBundles = new List<string>();

            foreach (KeyValuePair<string, bool> assetBundle in assetBundleDict)
                if (assetBundle.Value == true || WindowData.buildAllBundles == false)
                    enabledAssetBundles.Add(assetBundle.Key);

            List<AssetBundleInfo> assetBundles = GetAssetBundleBuilds(enabledAssetBundles);



            if (assetBundles.Count != 0)
            {
                lastBuiltAssetBundleInfos = new Dictionary<string, AssetBundleInfo>();

                foreach (AssetBundleInfo assetBundle in assetBundles)
                {
                    Debug.Log("Building Bundle: " + assetBundle.assetBundleBuild.assetBundleName + assetBundle.assetBundleBuild.assetBundleVariant);

                    LethalToolkitManager.Settings.onBeforeAssetBundleBuild?.Invoke(assetBundle);

                    lastBuiltAssetBundleInfos.Add(assetBundle.fullAssetBundleName, assetBundle);

                    BuildAssetBundlesParameters newParam = new BuildAssetBundlesParameters();

                    foreach (KeyValuePair<string, bool> kvp in WindowData.buildOptionsDict)
                        if (kvp.Value == true && Enum.TryParse(kvp.Key, false, out BuildAssetBundleOptions result))
                        {
                            newParam.options = newParam.options | result;
                            Debug.Log("Build Option " + kvp.Key + " Is Set To: " + newParam.options.HasFlag(result));
                        }




                    newParam.outputPath = WindowData.projectDirectory;
                    newParam.bundleDefinitions = new AssetBundleBuild[] { assetBundle.assetBundleBuild };
                    AssetBundleManifest newManifest = BuildPipeline.BuildAssetBundles(newParam);

                    


                    if (newManifest != null)
                    {
                        var outputFiles = Directory.EnumerateFiles(WindowData.projectDirectory, "*", SearchOption.TopDirectoryOnly);
                        Debug.Log("Output of the build:\n\t" + string.Join("\n\t", outputFiles));
                    }
                }

                foreach (AssetBundleInfo assetBundle in assetBundles)
                    LethalToolkitManager.Settings.onAfterAssetBundleBuild?.Invoke(assetBundle);
            }

            WindowData.previousDateTime = DateTime.Now.ToString();
        }

        public void LabelFieldHeader(string text)
        {
            GUIStyle headerStyle = BackgroundStyle.Get(settings.thirdColor);
            headerStyle.fontSize = settings.headerFontSize;

            EditorGUILayout.LabelField(text.ToBold().Colorize(EditorStyles.boldLabel.normal.textColor), headerStyle);
        }

        public static List<AssetBundleInfo> GetAssetBundleBuilds(List<string> assetBundleNames)
        {
            List<AssetBundleInfo> returnList = new List<AssetBundleInfo>();


            foreach (string assetBundleName in assetBundleNames)
            {
                if (string.IsNullOrEmpty(assetBundleName) == false && assetBundleName.Contains(SelectedVariantName))
                {
                    AssetBundleInfo newAssetBundleInfo = new AssetBundleInfo();

                    newAssetBundleInfo.assetBundleBuild.assetBundleName = assetBundleName.Replace(SelectedVariantName, string.Empty);
                    newAssetBundleInfo.assetBundleBuild.assetBundleVariant = assetBundleName.Substring(assetBundleName.IndexOf(SelectedVariantName));

                    Debug.Log("New AssetBundleInfo: " + newAssetBundleInfo.assetBundleBuild.assetBundleName + newAssetBundleInfo.assetBundleBuild.assetBundleVariant);

                    List<string> directAssetPathsFromAssetBundle = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName).ToList();
                    List<string> indirectAssetPathsFromAssetBundle = AssetDatabase.GetDependencies(directAssetPathsFromAssetBundle.ToArray(), recursive: false).ToList();

                    foreach (string assetPath in directAssetPathsFromAssetBundle.Concat(indirectAssetPathsFromAssetBundle))
                    {
                        Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                        if (type != null && type == typeof(SceneAsset))
                            newAssetBundleInfo.isSceneBundle = true;
                    }

                    foreach (string path in directAssetPathsFromAssetBundle)
                        if (newAssetBundleInfo.ValidAsset(path) == true)
                            newAssetBundleInfo.directBundledAssetInfos.Add(new BundledAssetInfo(path, ContentReferenceType.Explicit));

                    foreach (string path in indirectAssetPathsFromAssetBundle)
                        if (newAssetBundleInfo.ValidAsset(path) == true)
                            newAssetBundleInfo.indirectBundledAssetInfos.Add(new BundledAssetInfo(path, ContentReferenceType.Implicit));

                    foreach (BundledAssetInfo directAsset in new List<BundledAssetInfo>(newAssetBundleInfo.directBundledAssetInfos))
                        if (directAsset == null)
                            newAssetBundleInfo.directBundledAssetInfos.Remove(directAsset);

                    foreach (BundledAssetInfo indirectAsset in new List<BundledAssetInfo>(newAssetBundleInfo.indirectBundledAssetInfos))
                        if (indirectAsset == null)
                            newAssetBundleInfo.indirectBundledAssetInfos.Remove(indirectAsset);

                    newAssetBundleInfo.assetBundleBuild.assetNames = newAssetBundleInfo.AssetPaths.ToArray();

                    if (newAssetBundleInfo.AssetPaths.Count != 0)
                        returnList.Add(newAssetBundleInfo);
                }
            }



            return (returnList);
        }

        public static string GetFullAssetBundleName(string assetPath)
        {
            string returnString = string.Empty;
            if (string.IsNullOrEmpty(assetPath) == false)
            {
                returnString += AssetDatabase.GetImplicitAssetBundleName(assetPath);
                returnString += AssetDatabase.GetImplicitAssetBundleVariantName(assetPath);
            }
            return (returnString);
        }

        void MoveToModules()
        {
            if (WindowData.gameDirectory != string.Empty)
            {
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(WindowData.projectDirectory);
                    foreach (FileInfo lemFile in directoryInfo.GetFiles("*SelectedVariantName"))
                    {
                        Debug.Log("Attempting To Copy .lethalbundle File!");
                        string inputPath = WindowData.projectDirectory + lemFile.Name;
                        string outputPath = WindowData.gameDirectory + lemFile.Name;
                        FileInfo inputFile = new FileInfo(inputPath);
                        FileInfo outputFile = new FileInfo(outputPath);

                        Debug.Log("Input File Is: " + "\n" + inputPath + "\n" + "Output File Is: " + "\n" + outputPath);
                        if (File.Exists(outputPath))
                            outputFile.Delete();

                        inputFile.CopyTo(outputPath);
                    }
                }
                catch (IOException ioex)
                {
                    Console.WriteLine(ioex.Message);
                }
            }
        }


        public void PopulateBundledAssetInfoList(List<string> information, float width)
        {
            int counter = 0;
            GUIStyle newStyle = new GUIStyle();
            newStyle.richText = true;
            newStyle.fontSize = settings.textFontSize;
            newStyle.alignment = TextAnchor.MiddleCenter;
            foreach (string info in information)
            {
                Color backgroundColor;
                if (counter % 2 == 0) { backgroundColor = settings.firstColor; } else { backgroundColor = settings.secondColor; }
                EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(backgroundColor));
                EditorGUILayout.LabelField(info.ToBold().Colorize(EditorStyles.boldLabel.normal.textColor), newStyle, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                counter++;
            }
        }
    }

    public static class BackgroundStyle
    {
        public static GUIStyle Get(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            GUIStyle style = new GUIStyle();
            texture.SetPixel(0, 0, color);
            texture.Apply();
            style.normal.background = texture;
            return (style);
        }
    }
}
