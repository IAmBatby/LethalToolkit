using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LethalToolkit.AssetBundleBuilder
{
    public static class DebugAssetBundleBuildAssets
    {

        public static void DebugAssetBundleAssets(AssetBundleBuild assetBundleBuild)
        {
            foreach (string assetName in assetBundleBuild.assetNames)
                Debug.Log(assetName);
        }
    }
}
