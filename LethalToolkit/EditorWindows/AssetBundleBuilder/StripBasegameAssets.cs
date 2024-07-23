using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

namespace LethalToolkit
{
    public static class StripBasegameAssets
    {
        public static Dictionary<Item, Item> fakeItems;

        public static void StripSelectableLevelAssets(SelectableLevel selectableLevel)
        {
            fakeItems = new Dictionary<Item, Item>();
            string debugString = "Stripped " + fakeItems.Count + "Items";

            List<Item> selectableLevelScrap = selectableLevel.spawnableScrap.Select(s => s.spawnableItem).ToList();
            List<Item> basegameScrap = new List<Item>();


            foreach (SpawnableItemWithRarity scrap in new List<SpawnableItemWithRarity>(selectableLevel.spawnableScrap))
            {
                if (scrap.spawnableItem != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(scrap.spawnableItem);
                    if (assetPath.Contains("LethalCompany/Game"))
                    {
                        Item fakeItem = ScriptableObject.CreateInstance<Item>();
                        fakeItem.name = scrap.spawnableItem.name;
                        fakeItem.itemName = scrap.spawnableItem.name;
                        AssetDatabase.CreateAsset(fakeItem, LethalToolkitManager.LethalToolkitFolder + fakeItem.name + ".asset");
                        fakeItems.Add(fakeItem, scrap.spawnableItem);
                        debugString += scrap.spawnableItem.name;
                        scrap.spawnableItem = fakeItem;
                    }
                }
            }

            Debug.Log(debugString);
            //Test
        }

        public static void RestoreStrippedSelectableLevelAssets(SelectableLevel selectableLevel)
        {
            string debugString = "Restored " + fakeItems.Count + "Items";
            if (fakeItems != null)
            {
                foreach (SpawnableItemWithRarity scrap in new List<SpawnableItemWithRarity>(selectableLevel.spawnableScrap))
                    if (fakeItems.TryGetValue(scrap.spawnableItem, out Item realItem))
                    {
                        Item fakeScrap = scrap.spawnableItem;
                        scrap.spawnableItem = realItem;
                        debugString += scrap.spawnableItem.name;
                        fakeItems.Remove(fakeScrap);
                        AssetDatabase.DeleteAsset(LethalToolkitManager.LethalToolkitFolder + fakeScrap.name + ".asset");
                    }
            }

            Debug.Log(debugString);
        }
    }
}
