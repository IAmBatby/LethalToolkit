using DunGen;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace LethalToolkit
{
    public enum VariantType { lem, lethalbundle, None };

    [FilePath("Project/LethalToolkit/assetBundleBuilderSettings.foo", FilePathAttribute.Location.PreferencesFolder)]
    public class AssetBundleBuilderSettings : ScriptableSingleton<AssetBundleBuilderSettings>, ISerializationCallbackReceiver
    {
        public static AssetBundleBuilderSettings Instance
        {
            get
            {
                AssetBundleBuilderSettings.instance.RefreshCallbacks();
                return (instance);
            }

        }

        public bool hasRefreshedCallbacks;

        [SerializeField] public bool buildAllBundles;

        [SerializeField] public int openedAmountCounter;

        [SerializeField] public VariantType variantType;

        [SerializeField] public string projectDirectory;

        [SerializeField] public string gameDirectory;

        [SerializeField] public string previousDateTime;



        public Dictionary<string, bool> assetBundlesDict;
        [SerializeField] private List<string> assetBundlesDictKeys;
        [SerializeField] private List<bool> assetBundlesDictValues;

        public Dictionary<string, bool> buildOptionsDict;
        [SerializeField] private List<string> buildOptionsDictKeys;
        [SerializeField] private List<bool > buildOptionsDictValues;

        public static List<string> buildOptionsDescriptions = new List<string>()
        {
            "",
            "Don't compress the data when creating the AssetBundle.",
            "",
            "",
            "Do not include type information within the AssetBundle.",
            "",
            "Force rebuild the assetBundles.",
            "Ignore the type tree changes when doing the incremental build check.",
            "Append the hash to the assetBundle name.",
            "Use chunk-based LZ4 compression when creating the AssetBundle.",
            "Do not allow the build to succeed if any errors are reporting during it.",
            "Do a dry run build.",
            "Disables Asset Bundle LoadAsset by file name.",
            "Disables Asset Bundle LoadAsset by file name with extension.",
            "Removes the Unity Version number in the Archive File & Serialized File headers during the build.",
            "Use the content of the asset bundle to calculate the hash."
        };


        public void RefreshCallbacks()
        {
            if (hasRefreshedCallbacks)
            {
                AssetBundleBuilderSettings.instance.Save(true);
                Application.quitting -= Save;
                Application.quitting += Save;
                AssemblyReloadEvents.beforeAssemblyReload -= Save;
                AssemblyReloadEvents.beforeAssemblyReload += Save;
                hasRefreshedCallbacks = true;
            }
        }

        public void Save()
        {
            Debug.Log("Saving!");
            AssetBundleBuilderSettings.instance.Save(true);
        }

        public void OnBeforeSerialize()
        {
            EditorHelpers.SerializeDictionary(ref assetBundlesDict, ref assetBundlesDictKeys, ref assetBundlesDictValues);
            EditorHelpers.SerializeDictionary(ref buildOptionsDict, ref buildOptionsDictKeys, ref buildOptionsDictValues);
        }

        public void OnAfterDeserialize()
        {
            EditorHelpers.DeserializeDictionary(ref assetBundlesDict, ref assetBundlesDictKeys, ref assetBundlesDictValues);
            EditorHelpers.DeserializeDictionary(ref buildOptionsDict, ref buildOptionsDictKeys, ref buildOptionsDictValues);
        }
    }
}
