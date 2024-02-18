using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalToolkit
{
    public static class ToggleMeshColliders
    {
        public const string nameIdentifier = " (LethalToolkit Disabled Collider)";



        public static void DisableMeshColliders(AssetBundleInfo assetBundleInfo)
        {
            List<Collider> toggledMeshColliders = new List<Collider>();

            if (TryGetScene(assetBundleInfo, out Scene scene, out SelectableLevel selectableLevel))
            {
                foreach (GameObject rootObject in scene.GetRootGameObjects())
                    foreach (Collider meshCollider in rootObject.GetComponentsInChildren<Collider>())
                    {
                        if (meshCollider.gameObject.activeInHierarchy == true && meshCollider.enabled == true)
                            if (meshCollider.isTrigger == false)
                            {
                                if (!toggledMeshColliders.Contains(meshCollider))
                                {
                                    meshCollider.enabled = false;
                                    meshCollider.gameObject.name += nameIdentifier;
                                    toggledMeshColliders.Add(meshCollider);
                                }
                            }
                    }

                string debugString = "Disabled " + toggledMeshColliders.Count + " MeshColliders On Scene: " + scene.name + "\n";
                foreach (Collider meshCollider in toggledMeshColliders)
                    debugString += meshCollider.gameObject.name + "\n";
                Debug.Log(debugString);
            }
            else
                Debug.Log("Failed To Find Scene");
        }

        public static void EnableMeshColliders(AssetBundleInfo assetBundleInfo)
        {
            List<Collider> toggledMeshColliders = new List<Collider>();

            if (TryGetScene(assetBundleInfo, out Scene scene, out SelectableLevel selectableLevel))
            {
                foreach (GameObject rootObject in scene.GetRootGameObjects())
                    foreach (Collider meshCollider in rootObject.GetComponentsInChildren<Collider>())
                    {
                        if (meshCollider.gameObject.name.Contains(nameIdentifier))
                            {
                                meshCollider.enabled = true;
                                meshCollider.gameObject.name = meshCollider.gameObject.name.Replace(nameIdentifier, string.Empty);
                                toggledMeshColliders.Add(meshCollider);
                            }
                    }

                string debugString = "Re-Enabled" + toggledMeshColliders.Count + " MeshColliders On Scene: " + scene.name + "\n";
                foreach (Collider meshCollider in toggledMeshColliders)
                    debugString += meshCollider.gameObject.name + "\n";
                Debug.Log(debugString);
            }
            else
                Debug.Log("Failed To Find Scene");
        }

        public static bool TryGetScene(AssetBundleInfo assetBundleInfo, out Scene scene, out SelectableLevel selectableLevel)
        {
            selectableLevel = null;
            foreach (BundledAssetInfo bundledAsset in assetBundleInfo.directBundledAssetInfos)
                if (bundledAsset.assetType == typeof(SelectableLevel))
                {
                    selectableLevel = (SelectableLevel)AssetDatabase.LoadAssetAtPath(bundledAsset.assetPath, typeof(SelectableLevel));
                    if (selectableLevel != null && SceneManager.GetActiveScene().name == selectableLevel.sceneName)
                    {
                        scene = SceneManager.GetActiveScene();
                        return (true);
                    }
                }
            scene = default;
            return (false);
        }
    }
}
