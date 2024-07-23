using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace LethalToolkit
{
    public enum ContentSourceType { Custom, AssetRip }
    public enum ContentReferenceType { Explicit, Implicit }


    [System.Serializable]
    public class AssetBundleInfo
    {
        public AssetBundleBuild assetBundleBuild;
        public string fullAssetBundleName
        {
            get
            {
                string returnString = string.Empty;
                if (assetBundleBuild.assetBundleName != null)
                    returnString += assetBundleBuild.assetBundleName;
                if (assetBundleBuild.assetBundleVariant != null)
                    returnString += assetBundleBuild.assetBundleVariant;
                return (returnString);
            }
        }
        public List<BundledAssetInfo> directBundledAssetInfos = new List<BundledAssetInfo>();
        public List<BundledAssetInfo> indirectBundledAssetInfos = new List<BundledAssetInfo>();
        public List<BundledAssetInfo> bundledAssetInfos => directBundledAssetInfos.Concat(indirectBundledAssetInfos).ToList();
        public bool isSceneBundle;

        public static List<string> blacklistedFileExtensions = new List<string>()
        {
            ".cs",
            ".dll"
        };

        public List<string> AssetPaths
        {
            get
            {
                List<string> paths = new List<string>();
                foreach (BundledAssetInfo directAsset in directBundledAssetInfos)
                    paths.Add(directAsset.assetPath);
                foreach (BundledAssetInfo indirectAsset in indirectBundledAssetInfos)
                    paths.Add(indirectAsset.assetPath);
                return (paths);
            }
        }

        public List<BundledAssetInfo> Assets => directBundledAssetInfos.Concat(indirectBundledAssetInfos).ToList();

        public AssetBundleInfo()
        {
            assetBundleBuild = new AssetBundleBuild();
        }

        public bool ValidAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) == true)
                return (false);

            string assetAssetBundleName = string.Empty;
            assetAssetBundleName += AssetDatabase.GetImplicitAssetBundleName(assetPath);
            assetAssetBundleName += AssetDatabase.GetImplicitAssetBundleVariantName(assetPath);
            assetAssetBundleName = assetAssetBundleName.Replace(".", string.Empty);
            
            if (assetAssetBundleName != string.Empty)
            {
                //Debug.Log(assetPath + ": " + assetAssetBundleName + " , " + fullAssetBundleName.Replace(".", string.Empty));
                if (assetAssetBundleName != fullAssetBundleName.Replace(".", string.Empty))
                    return (false);
            }

            if ((AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(SceneAsset)) != isSceneBundle)
                return (false);

            if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(MonoScript))
                return (false);

            if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(AssemblyDefinition))
                return (false);

            if (AssetPaths.Contains(assetPath))
                return (false);

            return (true);
        }
    }

    [System.Serializable]
    public class BundledAssetInfo
    {
        public static Dictionary<string, string> assetTypes = new Dictionary<string, string>()
        {
            {"Assembly-CSharp", "Lethal Company" },
            {"LethalLevelLoader", "LethalLevelLoader" },
            {"LethalLib", "LethalLib" }
        };

        public string assetPath;
        public string assetName;
        public string assetSource;

        public Type assetType;

        public ContentSourceType contentSourceType;
        public ContentReferenceType contentReferenceType;

        public BundledAssetInfo(string newAssetPath, ContentReferenceType newContentReferenceType)
        {
            Object assetObject = (Object)AssetDatabase.LoadAssetAtPath(newAssetPath, typeof(Object));

            if (assetObject == null)
            {
                Debug.LogError("Failed to create new BundledAssetInfo! Provided Path Was: " + newAssetPath);
                return;
            }

            assetPath = newAssetPath;
            assetName = assetObject.name;

            assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

            contentReferenceType = newContentReferenceType;

            bool foundSpecialAssembly = false;
            foreach (string specialAssemblyName in assetTypes.Keys)
                if (assetObject.GetType().Assembly.GetName().FullName.Contains(specialAssemblyName))
                {
                    assetSource = assetTypes[specialAssemblyName];
                    foundSpecialAssembly = true;
                }

            if (foundSpecialAssembly == false)
                assetSource = "Unity";

            if (assetPath.Contains("LethalCompany/Game"))
                contentSourceType = ContentSourceType.AssetRip;
            else
                contentSourceType = ContentSourceType.Custom;

        }
    }
}
