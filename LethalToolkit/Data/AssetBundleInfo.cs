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
        public List<BundledAssetInfo> directBundledAssetInfos = new List<BundledAssetInfo>();
        public List<BundledAssetInfo> indirectBundledAssetInfos = new List<BundledAssetInfo>();
        public List<BundledAssetInfo> bundledAssetInfos => directBundledAssetInfos.Concat(indirectBundledAssetInfos).ToList();

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
                Debug.LogError("Failed to create new BundledAssetInfo!");
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
