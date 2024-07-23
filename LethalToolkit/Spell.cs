using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalToolkit
{
    public enum SpellType { Fire, Water, Earth, Air };
    [CreateAssetMenu(menuName = "grimblesepicmagic/Spell")]
    public class Spell : ScriptableObject
    {
        public string Name = "";
        public SpellType spellType;
        public float ManaCost;
        public Sprite spellIcon;
        public GameObject spawnablePrefab;

        public delegate void BeforeSpellCasted(GameObject spawnedSpellObject);
        public BeforeSpellCasted onBeforeSpellCasted;
    }

    public class SpellBundleLoader
    {
        AssetBundle newBundle;
        List<Spell> allSpells = new List<Spell>();

        public void LoadBundle()
        {
            foreach (Spell newSpell in newBundle.LoadAllAssets<Spell>())
                if (newSpell.spawnablePrefab != null)
                    allSpells.Add(newSpell);
        }


        public List<Spell> GetSpellsOfType(SpellType specifiedSpellType)
        {
            List<Spell> returnList = new List<Spell>();

            foreach (Spell spell in allSpells)
                if (spell.spellType == specifiedSpellType)
                    returnList.Add(spell);

            return (returnList);
        }
    }

    public class Spellbook : MonoBehaviour
    {
        public void CastSpell(Spell spell)
        {
            spell.spawnablePrefab.SetActive(false);

            GameObject spawnedSpellObject = GameObject.Instantiate(spell.spawnablePrefab);

            spell.onBeforeSpellCasted?.Invoke(spawnedSpellObject);

            spawnedSpellObject.SetActive(true);

            spell.spawnablePrefab.SetActive(true);
        }
    }


    public class Tome : ScriptableObject
    {
        public Spell minorSpell;
        public Spell majorSpell;
        public Spell buffSpell;
        public Spell defenseSpell;
    }

}
