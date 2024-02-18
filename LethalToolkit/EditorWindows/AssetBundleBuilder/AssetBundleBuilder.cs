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

namespace LethalToolkit.AssetBundleBuilder
{
    public class AssetBundleBuilderWindow : EditorWindow
    {
        private static bool buildAllBundles = true;
        private static Dictionary<string, bool> assetBundleDict;

        public static List<AssetBundleInfo> assetBundleInfos = new List<AssetBundleInfo>();

        public UnityEvent onBeforeBuildEvents;
        public UnityEvent onAfterBuildEvents;

        public enum VariantType { lem, lethalbundle, None };
        public static VariantType variantType;

        public static string SelectedVariantName
        {
            get
            {
                string returnString = string.Empty;
                if (variantType == VariantType.lem)
                    returnString = ".lem";
                if (variantType == VariantType.lethalbundle)
                    returnString = ".lethalbundle";
                return (returnString);
            }
        }

        public static string assetBundleDirectory = LethalToolkitManager.Instance.LethalToolkitSettings.lethalCompanyAssetBundleDirectory;
        public static string modulesDirectory = LethalToolkitManager.Instance.LethalToolkitSettings.unityAssetBundleDirectory;
        enum compressionOption { NormalCompression = 0, FastCompression = 1, Uncompressed = 2 }
        compressionOption compressionMode = compressionOption.NormalCompression;
        bool _64BitsMode;
        bool debugBuiltAssetBundles;

        public Vector2 scrollPos;

        static LethalToolkitSettings settings = LethalToolkitManager.Instance.LethalToolkitSettings;

        public static AssetBundleInfo lastAssetBundleInfo;


        public static List<string> blacklistedFileExtensions = new List<string>()
        {
            ".cs",
            ".dll"
        };

        public static AssetBundleBuilderWindow window;

        [MenuItem("LethalToolkit/Tools/AssetBundle Builder")]
        public static void OpenWindow()
        {
            CloseWindow();

            window = GetWindow<AssetBundleBuilderWindow>("LethalToolkit: AssetBundle Builder");
        }

        public static void CloseWindow()
        {
            if (window != null)
                window.Close();
            window = null;
        }

        public static void RefreshAssetBundlesDictionary()
        {
            assetBundleDict = new Dictionary<string, bool>();
            foreach (string assetBundleName in AssetDatabase.GetAllAssetBundleNames())
                if (assetBundleName.Contains(SelectedVariantName))
                    assetBundleDict.Add(assetBundleName, false);
        }

        public static void OnAssemblyReload()
        {
            if (window != null)
            {
                //CloseWindow();
                //OpenWindow();
            }    
        }

        void OnEnable()
        {
            RefreshAssetBundlesDictionary();
        }

        void OnDisable()
        {
            AssemblyReloadEvents.afterAssemblyReload -= OnAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAssemblyReload;
        }

        void OnGUI()
        {
            if (assetBundleDict == null || assetBundleDict.Count == 0)
                RefreshAssetBundlesDictionary();

            GUILayout.ExpandWidth(true);
            GUILayout.ExpandHeight(true);

            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Output Path", "The directory where the asset bundles will be saved."), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            assetBundleDirectory = EditorGUILayout.TextField(assetBundleDirectory, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Modules Path", "The directory where the asset bundles will be saved."), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            modulesDirectory = EditorGUILayout.TextField(modulesDirectory, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Variant Type", "Select the used AssetBundle variant type."), GUILayout.Width(145));
            variantType = (VariantType)EditorGUILayout.EnumPopup(variantType, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUILayout.Label("Options", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Compression Mode", "Select the compression option for the asset bundle. Faster the compression is, faster the assets will load and less CPU it will use, but the Bundle will be bigger."), GUILayout.Width(145));
            compressionMode = (compressionOption)EditorGUILayout.EnumPopup(compressionMode, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("64 Bits Asset Bundle (Not recommended)", "Better performances but incompatible with 32 bits computers."), GUILayout.ExpandHeight(false), GUILayout.Width(270));
            _64BitsMode = EditorGUILayout.Toggle(_64BitsMode);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Build All Bundles."), GUILayout.ExpandHeight(false), GUILayout.Width(270));
            buildAllBundles = EditorGUILayout.Toggle(buildAllBundles);
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUILayout.Label("Asset Bundle Selection", EditorStyles.boldLabel);
            List<bool> reassignBools = new List<bool>(assetBundleDict.Values);

            Dictionary<string, bool> reassignDict = new Dictionary<string, bool>();

            if (buildAllBundles == false)
            {
                for (int i = 0; i < assetBundleDict.Count; i++)
                {
                    if (assetBundleDict.ElementAt(i).Key.Contains(SelectedVariantName) || (SelectedVariantName == string.Empty && !assetBundleDict.ElementAt(i).Key.Contains(".")))
                    {
                        Color color;
                        if (i % 2 == 0) { color = settings.firstColor; } else { color = settings.secondColor; }
                        EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(color));
                        EditorGUILayout.LabelField(new GUIContent(assetBundleDict.ElementAt(i).Key), GUILayout.ExpandHeight(false));
                        assetBundleDict[assetBundleDict.ElementAt(i).Key] = EditorGUILayout.Toggle(assetBundleDict.ElementAt(i).Value);
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.Space(5);

            GUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.ExpandHeight(true) });
            if (GUILayout.Button("Build AssetBundles", GUILayout.ExpandHeight(false), GUILayout.Width(240)))
            {
                BuildAssetBundles();
            }
            GUILayout.EndHorizontal();

            if (lastAssetBundleInfo != null)
            {
                List<BundledAssetInfo> allAssets = new List<BundledAssetInfo>(lastAssetBundleInfo.directBundledAssetInfos.Concat(lastAssetBundleInfo.indirectBundledAssetInfos));
                LethalToolkitSettings settings = LethalToolkitManager.Instance.LethalToolkitSettings;

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
            }
        }

        void BuildAssetBundles()
        {
            List<string> enabledAssetBundles = new List<string>();

            foreach (KeyValuePair<string, bool> assetBundle in assetBundleDict)
                if (assetBundle.Value == true || buildAllBundles == true)
                    enabledAssetBundles.Add(assetBundle.Key);

            List<AssetBundleInfo> assetBundles = GetAssetBundleBuilds(enabledAssetBundles);

            foreach (AssetBundleInfo assetBundle in assetBundles)
                LethalToolkitManager.Instance.LethalToolkitSettings.onBeforeAssetBundleBuild?.Invoke(assetBundle);

            lastAssetBundleInfo = assetBundles.Last();


            BuildAssetBundlesParameters newParam = new BuildAssetBundlesParameters();
            newParam.outputPath = assetBundleDirectory;
            newParam.bundleDefinitions = assetBundles.Select(bundle => bundle.assetBundleBuild).ToArray();

            AssetBundleManifest newManifest = BuildPipeline.BuildAssetBundles(newParam);
            if (newManifest != null)
            {
                var outputFiles = Directory.EnumerateFiles(assetBundleDirectory, "*", SearchOption.TopDirectoryOnly);
                Debug.Log("Output of the build:\n\t" + string.Join("\n\t", outputFiles));
            }

            foreach (AssetBundleInfo assetBundle in assetBundles)
                LethalToolkitManager.Instance.LethalToolkitSettings.onAfterAssetBundleBuild?.Invoke(assetBundle);
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
                if (!string.IsNullOrEmpty(assetBundleName))
                {
                    AssetBundleInfo newAssetBundleInfo = new AssetBundleInfo();
                    AssetBundleBuild newAssetBundleBuild = new AssetBundleBuild();

                    if ((SelectedVariantName != string.Empty && assetBundleName.Contains(SelectedVariantName)) || (SelectedVariantName == string.Empty && !assetBundleName.Contains(".")))
                    {
                        newAssetBundleBuild.assetBundleName = assetBundleName.Replace(SelectedVariantName, string.Empty);
                        newAssetBundleBuild.assetBundleVariant = assetBundleName.Substring(assetBundleName.IndexOf(SelectedVariantName));
                    }
                    else
                        newAssetBundleBuild.assetBundleName = assetBundleName;

                    //Debug.Log(newAssetBundleBuild.assetBundleName + " | " + newAssetBundleBuild.assetBundleVariant);
                    newAssetBundleBuild.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
                    foreach (string path in newAssetBundleBuild.assetNames)
                    {
                        //Debug.Log("direct asset: " + path);
                        newAssetBundleInfo.directBundledAssetInfos.Add(new BundledAssetInfo(path, ContentReferenceType.Explicit));
                    }

                    foreach (string dependantAssetName in AssetDatabase.GetDependencies(newAssetBundleBuild.assetNames, recursive: false))
                        if (!newAssetBundleBuild.assetNames.Contains(dependantAssetName) && LegalFileExtension(dependantAssetName) == true)
                        {
                            //Debug.Log("Found Dependency: " + dependantAssetName);
                            newAssetBundleBuild.assetNames = newAssetBundleBuild.assetNames.AddItem(dependantAssetName).ToArray();
                        }

                    foreach (string assetPath in newAssetBundleBuild.assetNames)
                        if (!newAssetBundleInfo.AssetPaths.Contains(assetPath))
                            newAssetBundleInfo.indirectBundledAssetInfos.Add(new BundledAssetInfo(assetPath, ContentReferenceType.Implicit));

                    newAssetBundleInfo.assetBundleBuild = newAssetBundleBuild;
                    returnList.Add(newAssetBundleInfo);
                }
            }

            return (returnList);
        }


        public static bool LegalFileExtension(string assetName)
        {
            bool returnBool = true;
            foreach (string fileExtension in blacklistedFileExtensions)
                if (assetName.Contains(fileExtension))
                    returnBool = false;
            return (returnBool);
        }

        void MoveToModules()
        {
            if (modulesDirectory != string.Empty)
            {
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(assetBundleDirectory);
                    foreach (FileInfo lemFile in directoryInfo.GetFiles("*SelectedVariantName"))
                    {
                        Debug.Log("Attempting To Copy .lethalbundle File!");
                        string inputPath = assetBundleDirectory + lemFile.Name;
                        string outputPath = modulesDirectory + lemFile.Name;
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
        private static GUIStyle style = new GUIStyle();
        private static Texture2D texture = new Texture2D(1, 1);


        public static GUIStyle Get(Color color)
        {
            texture.SetPixel(0, 0, color);
            texture.Apply();
            style.normal.background = texture;
            return style;
        }
    }
}
