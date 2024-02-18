using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace LethalToolkit
{
    [System.Serializable]
    public class AssetBundleInfoEvent : UnityEvent<AssetBundleInfo> { }

    [CreateAssetMenu(fileName = "LethalToolkitSettings", menuName = "LethalToolkit/LethalToolkitSettings")]
    public class LethalToolkitSettings : ScriptableObject
    {
        public static Dictionary<string, string> assetTypes = new Dictionary<string, string>()
        {
            {"Assembly-CSharp", "<b> <color=\"orange\"> (Lethal Company) </color> </b>" },
            {"LethalLevelLoader", "<b> <color=\"yellow\"> (LethalLevelLoader) </color> </b>" },
            {"LethalLib", "<b> <color=\"cyan\"> (LethalLib) </color> </b>" }
        };

        public string vanillaScenesFolderDir;

        public string unityAssetBundleDirectory;
        public string lethalCompanyAssetBundleDirectory;

        public AssetBundleInfoEvent onBeforeAssetBundleBuild;
        public AssetBundleInfoEvent onAfterAssetBundleBuild;

        public Color firstColor;
        public Color secondColor;
        public Color thirdColor;

        public Color lightRedColor;

        public float overrideComponentSize;
        public Vector2 overrideComponentOffset;

        public float assetNumberWidth;
        public float assetNameWidth;
        public float assetTypeWidth;
        public float assetSourceWidth;
        public float assetReferenceWidth;
        public float assetPathWidth;

        public float spaceMultiplier = 1f;

        public float gameObjectNameWidth;
        public float componentIconsWidth;

        public int hierarchyOffset;

        public int headerFontSize;
        public int textFontSize;

        public ExtendedLevel lastSelectedExtendedLevel;
        public ExtendedDungeonFlow lastSelectedExtendedDungeonFlow;

        public static void DebugAssetBundleAssets(AssetBundleInfo assetBundleInfo)
        {
            AssetBundleBuild assetBundleBuild = assetBundleInfo.assetBundleBuild;
            string debugString = "Report of all assets found in AssetBundle: " + "<b>" + assetBundleInfo.assetBundleBuild.assetBundleName + assetBundleInfo.assetBundleBuild.assetBundleVariant + "</b>" + "\n" + "\n";

            debugString += "<b> Directly Labeled Assets: </b>" + "\n";
            foreach (BundledAssetInfo directAsset in assetBundleInfo.directBundledAssetInfos)
                debugString += directAsset.assetName + " (" + directAsset.assetSource + ") (" + directAsset.contentReferenceType.ToString() + ") (" + directAsset.contentSourceType.ToString() + ")" + "\n";

            debugString += "\n" + "<b> Indirectly Labeled Assets: </b>" + "\n";
            foreach (BundledAssetInfo indirectAsset in assetBundleInfo.indirectBundledAssetInfos)
                debugString += indirectAsset.assetName + " (" + indirectAsset.assetSource + ") (" + indirectAsset.contentReferenceType.ToString() + ") (" + indirectAsset.contentSourceType.ToString() + ")" + "\n";

            Debug.Log(debugString);
        }

        public static string TryGetThemedAssetName(string assetPath)
        {
            string returnString = string.Empty;

            Object assetObject = (Object)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));

            if (assetObject != null)
            {
                returnString += assetObject.name;
                bool foundSpecialAssembly = false;
                foreach (string specialAssemblyName in assetTypes.Keys)
                    if (assetObject.GetType().Assembly.GetName().FullName.Contains(specialAssemblyName))
                    {
                        returnString += assetTypes[specialAssemblyName];
                        foundSpecialAssembly = true;
                    }

                if (foundSpecialAssembly == false)
                    returnString += "<b> (Unity) </b>";
                Debug.Log(assetPath);
                if (assetPath.Contains("LethalCompany/Game"))
                    returnString += "<b>(AssetRip Asset) </b>";
            }

            return (returnString);
        }

        public static void DebugAllAssetPaths(AssetBundleInfo assetBundleInfo)
        {
            Debug.Log("Debugging All Asset Paths!");
            string debugString = "All AssetPaths" + "\n" + "\n";

            //foreach (string assetPath in AssetDatabase.GetAllAssetPaths())
            string[] allAssets = AssetDatabase.FindAssets("t:Object");
            Debug.Log(allAssets.Length);
            foreach (string assetPath in allAssets)
                debugString += AssetDatabase.GUIDToAssetPath(assetPath) + "\n";

            Debug.Log(debugString);
        }

        public static void DisableMeshColliders(AssetBundleInfo assetBundleInfo)
        {
            ToggleMeshColliders.DisableMeshColliders(assetBundleInfo);
        }

        public static void EnableMeshColliders(AssetBundleInfo assetsBundleInfo)
        {
            ToggleMeshColliders.EnableMeshColliders(assetsBundleInfo);
        }
    }
}
