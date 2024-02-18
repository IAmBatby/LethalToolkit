using DunGen;
using LethalLevelLoader;
using LethalToolkit.AssetBundleBuilder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
using Scene = UnityEngine.SceneManagement.Scene;

namespace LethalToolkit
{
    internal class ExtendedDungeonFlowValidatorWindow : UnityEditor.EditorWindow
    {
        public static ExtendedDungeonFlowValidatorWindow window;

        public UnityEngine.Object extendedDungeonFlowObject;

        public static DynamicTogglePopup dynamicPopup = new DynamicTogglePopup(new string[] { "None", "All", "Tiles", "Archetypes", "SpawnSyncedObjects" });

        public Color backgroundColor;
        public Color defaultTextColor;
        private int defaultFontSize;

        public Vector2 scrollPos;


        public LethalToolkitSettings settings = LethalToolkitManager.Instance.LethalToolkitSettings;

        [MenuItem("LethalToolkit/Tools/ExtendedDungeonFlow Validator")]
        public static void OpenWindow()
        {
            dynamicPopup.Clear();
            window = GetWindow<ExtendedDungeonFlowValidatorWindow>("LethalToolkit: ExtendedDungeonFlow Validator");
        }


        public void OnGUI()
        {
            GUILayout.ExpandWidth(true);
            GUILayout.ExpandHeight(true);
            backgroundColor = EditorHelpers.DefaultBackgroundColor;

            defaultFontSize = GUI.skin.font.fontSize;

            GUI.skin.label.richText = true;
            GUI.skin.textField.richText = true;

            extendedDungeonFlowObject = settings.lastSelectedExtendedDungeonFlow;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ExtendedDungeonFlow", EditorStyles.boldLabel);
            extendedDungeonFlowObject = EditorGUILayout.ObjectField(extendedDungeonFlowObject, typeof(ExtendedDungeonFlow), true);
            EditorGUILayout.EndHorizontal();

            if (extendedDungeonFlowObject != null && extendedDungeonFlowObject is ExtendedDungeonFlow)
            {
                ExtendedDungeonFlow extendedDungeonFlow = (ExtendedDungeonFlow)extendedDungeonFlowObject;
                settings.lastSelectedExtendedDungeonFlow = extendedDungeonFlow;

                EditorGUILayout.LabelField("Content Source Name: " + extendedDungeonFlow.contentSourceName);
                EditorGUILayout.LabelField("Dungeon Display Name: " + extendedDungeonFlow.dungeonDisplayName);
                EditorGUILayout.ObjectField(extendedDungeonFlow.dungeonFirstTimeAudio, typeof(AudioClip), true);

                if (extendedDungeonFlow.dungeonFlow != null)
                {

                    List<Tile> allTiles = extendedDungeonFlow.dungeonFlow.GetTiles();
                    List<GameObject> tilePrefabs = allTiles.Select(tile => tile.gameObject).Distinct().ToList();

                    dynamicPopup.Toggle(EditorGUILayout.Popup(dynamicPopup.CurrentSelection, dynamicPopup.CurrentSelectionIndex, dynamicPopup.ToggleOptions));

                    if (dynamicPopup.CurrentSelection == "Tiles")
                        TilesReport(extendedDungeonFlow, tilePrefabs);
                    else if (dynamicPopup.CurrentSelection == "SpawnSyncedObjects")
                        SpawnSyncedObjectsReport(extendedDungeonFlow, tilePrefabs);
                }
                else
                    EditorGUILayout.LabelField("DungeonFlow Asset Reference Is Null.", EditorStyles.boldLabel);
            }
        }

        public void TilesReport(ExtendedDungeonFlow extendedDungeonFlow, List<GameObject> tilePrefabs)
        {
            List<Tile> allTiles = extendedDungeonFlow.dungeonFlow.GetTiles();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(700));
            GUILayout.BeginHorizontal();

            EditorHelpers.InsertObjectDataColumn("Tile Prefabs", 200f, tilePrefabs);
            EditorHelpers.InsertValueDataColumn("Instances Amount", 50f, tilePrefabs.Select(tile => GetAmount(allTiles, tile.GetComponent<Tile>()).ToString()).ToList());

            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        public static List<Tile> allTiles = new List<Tile>();
        public static List<SpawnSyncedObject> allSpawnSyncedObjects = new List<SpawnSyncedObject>();
        public static List<GameObject> spawnSyncedObjectPrefabs = new List<GameObject>();
        public static List<SpawnSyncedObject> spawnSyncedObjectPrefabSpawnSyncedObjectList = new List<SpawnSyncedObject>();
        public static List<GameObject> assetRipTilePrefabs = new List<GameObject>();
        public static List<GameObject> assetRipSpawnSyncedObjectSpawnPrefabs = new List<GameObject>();
        public void SpawnSyncedObjectsReport(ExtendedDungeonFlow extendedDungeonFlow, List<GameObject> tilePrefabs)
        {
            if (dynamicPopup.CheckToggle("SpawnSyncedObjects"))
            {
                Debug.Log("test");
                allTiles = extendedDungeonFlow.dungeonFlow.GetTiles();
                allSpawnSyncedObjects = extendedDungeonFlow.dungeonFlow.GetSpawnSyncedObjects();
                spawnSyncedObjectPrefabs = allSpawnSyncedObjects.Select(spawnSyncedObject => GetPrefabAsset(spawnSyncedObject.gameObject)).Distinct().ToList();
                spawnSyncedObjectPrefabSpawnSyncedObjectList = new List<SpawnSyncedObject>();

                assetRipTilePrefabs = EditorHelpers.GetPrefabsWithType(typeof(Tile));
                assetRipSpawnSyncedObjectSpawnPrefabs = new List<GameObject>();
                foreach (GameObject tilePrefab in assetRipTilePrefabs)
                    foreach (SpawnSyncedObject spawnSyncedObject in tilePrefab.GetComponentsInChildren<SpawnSyncedObject>())
                        if (spawnSyncedObject.spawnPrefab != null)
                            assetRipSpawnSyncedObjectSpawnPrefabs.Add(spawnSyncedObject.spawnPrefab);

                foreach (GameObject spawnSyncedObjectPrefab in spawnSyncedObjectPrefabs)
                {
                    SpawnSyncedObject spawnSyncedObject = spawnSyncedObjectPrefab.GetComponent<SpawnSyncedObject>();
                    if (spawnSyncedObject == null)
                        spawnSyncedObject = spawnSyncedObjectPrefab.GetComponentInChildren<SpawnSyncedObject>();

                    if (spawnSyncedObject != null)
                        spawnSyncedObjectPrefabSpawnSyncedObjectList.Add(spawnSyncedObject);
                    else
                        Debug.LogError("Couldnt Find SpawnSyncedObject!");

                }
            }



            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(700));
            GUILayout.BeginHorizontal();

            EditorHelpers.InsertObjectDataColumn("Spawn Synced Object", 200f, spawnSyncedObjectPrefabSpawnSyncedObjectList.Select(s => s.gameObject).ToList());
            //EditorHelpers.InsertObjectDataColumn("Tile Prefab", 200f, spawnSyncedObjectPrefabs);
            EditorHelpers.InsertObjectDataColumn("Spawn Prefab", 200f, spawnSyncedObjectPrefabSpawnSyncedObjectList.Select(spawnSyncedObject => spawnSyncedObject.spawnPrefab).ToList());
            EditorHelpers.InsertValueDataColumn("Instances Amount", 50f, spawnSyncedObjectPrefabs.Select(tile => GetAmount(allSpawnSyncedObjects.Select(spawn => GetPrefabAsset(spawn.gameObject)).ToList(), tile).ToString()).ToList());

            List<string> assetSourceDetails = new List<string>();
            foreach (SpawnSyncedObject spawnSyncedObject in spawnSyncedObjectPrefabSpawnSyncedObjectList)
            {
                if (AssetDatabase.GetAssetPath(spawnSyncedObject.spawnPrefab).Contains("LethalCompany/Game"))
                    assetSourceDetails.Add("Asset Rip");
                else
                    assetSourceDetails.Add("Custom");
            }

            EditorHelpers.InsertValueDataColumn("Spawn Prefab Source", 200f, assetSourceDetails);
            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        public int GetAmount<T>(List<T> list, T value)
        {
            int returnInt = 0;

            foreach (T item in list)
                if (value.Equals(item))
                    returnInt++;

            return (returnInt);
        }

        public GameObject GetPrefabAsset(GameObject prefabInstance)
        {
            return (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabInstance)));
        }
    }
}
