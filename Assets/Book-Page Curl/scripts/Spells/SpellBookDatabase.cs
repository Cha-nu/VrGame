using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Spell Book Database")]
public class SpellBookDatabase : ScriptableObject
{
    public List<SpellDefinition> spells = new List<SpellDefinition>();

    public int Count => spells.Count;

    public SpellDefinition Get(int index)
    {
        if (index < 0 || index >= spells.Count) return null;
        return spells[index];
    }
}
